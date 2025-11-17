#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class IndefiniteWalk : Action
        {
                [SerializeField] public float speed;
                [SerializeField] public bool detectAirGap;
                [System.NonSerialized] private bool hitAirGap = false;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (hitAirGap && root.onGround && !root.world.missedAVertical)
                        {
                                hitAirGap = false;
                        }

                        float oldSpeed = speed;

                        if (root.world.leftWall)
                        {
                                speed = Mathf.Abs(speed);
                        }

                        if (root.world.rightWall)
                        {
                                speed = -Mathf.Abs(speed);
                        }

                        if (detectAirGap && root.world.onGround && root.world.missedAVertical && !hitAirGap)
                        {
                                hitAirGap = true;
                                speed *= -1f;
                        }

                        if (oldSpeed != speed)
                        {
                                return NodeState.Success;
                        }
                        root.velocity.x += speed;

                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(65, "If the AI hits a wall or air gap, it will change walking direction." +
                                        "\n \nReturns Running, Success");
                        }
                        FoldOut.Box(2, color, offsetY: -2);
                        parent.Field("Speed", "speed");
                        parent.FieldToggle("Detect Air Gap", "detectAirGap");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
