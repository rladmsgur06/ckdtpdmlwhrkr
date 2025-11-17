#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.Interactables;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class WaterFloat : Action
        {
                [SerializeField] public Water water;
                [SerializeField, Range(0.25f, 1f)] public float buoyancy = 1f;
                [SerializeField, Range(0.01f, 0.5f)] public float damping = 0.2f;
                [SerializeField, Range(0f, 0.2f)] public float impact = 0.15f;

                [System.NonSerialized] private float springVelocity;
                [System.NonSerialized] private Vector2 waveTopPoint;
                [System.NonSerialized] private int particleIndex;
                [System.NonSerialized] private float centerBottom;
                [System.NonSerialized] private float center;
                [System.NonSerialized] private float x;
                [System.NonSerialized] private int passengers = 0;
                [System.NonSerialized] private bool applyImpactVel;
                [System.NonSerialized] private float impactCounter = 0;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (water == null)
                        {
                                return NodeState.Failure;
                        }

                        if (root.movingPlatform.box != null) // moving platform
                        {
                                x = root.transform.position.x;
                                center = root.transform.position.y;
                                float height = root.movingPlatform.box.size.y * root.transform.localScale.y;
                                centerBottom = center - height * 0.5f;
                                root.velocity.y -= buoyancy * 10f * Root.deltaTime; // gravity, incase moving platform goes out of water
                                if (passengers != root.movingPlatform.passengerCount)
                                {
                                        passengers = root.movingPlatform.passengerCount;
                                        water.ApplyImpact(particleIndex, -impact);
                                        applyImpactVel = true;
                                        impactCounter = 0;
                                }
                        }
                        else if (root.world.box != null) // character
                        {
                                x = root.transform.position.x;
                                center = root.world.box.center.y;
                                centerBottom = root.world.box.bottom;
                        }
                        else
                        {
                                return NodeState.Failure;
                        }

                        if (water.FoundWater(new Vector2(x, centerBottom), false, ref waveTopPoint, ref particleIndex, force: true))
                        {
                                if (applyImpactVel)
                                {
                                        root.velocity.y = -Compute.Lerp(1f, 0, 1f, ref impactCounter, out bool complete);
                                        if (complete || center < (waveTopPoint.y - 1.25f))
                                        {
                                                applyImpactVel = false;
                                        }
                                }
                                else
                                {
                                        Buoyancy(ref root.velocity);
                                }
                                return NodeState.Running;
                        }
                        return NodeState.Failure;
                }

                private void Buoyancy (ref Vector2 velocity)
                {
                        float acceleration = buoyancy * (waveTopPoint.y - center) - springVelocity * damping;
                        springVelocity += acceleration * Root.deltaTime * 10f;
                        velocity.y = springVelocity;

                        if (Root.deltaTime != 0 && (centerBottom + velocity.y * Root.deltaTime) >= waveTopPoint.y)
                        {
                                velocity.y = (((waveTopPoint.y - 0.1f) - centerBottom) / Root.deltaTime) * 0.5f; // Clamp player y velocity if being shot out of water too fast by spring/buoyancy
                                springVelocity = 0;
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "The AI will float on the waterline." +
                                        "\n \nReturns Running, Failure");
                        }

                        FoldOut.Box(4, color, offsetY: -2);
                        parent.Field("Water", "water");
                        parent.Field("Buoyancy", "buoyancy");
                        parent.Field("Damping", "damping");
                        parent.Field("Impact", "impact");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
