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
        public class WaterEnter : Action
        {
                [SerializeField] public Water water;
                [SerializeField] public UnityEventEffect onEnter = new UnityEventEffect();
                [SerializeField] public UnityEventEffect onExit = new UnityEventEffect();
                [SerializeField, Range(0f, 0.2f)] public float impactEnter = 0.15f;
                [SerializeField, Range(0f, 0.2f)] public float impactExit = 0.15f;
                [SerializeField] public string onEnterWE;
                [SerializeField] public string onExitWE;

                [System.NonSerialized] private Vector2 waveTopPoint;
                [System.NonSerialized] private int particleIndex;
                [System.NonSerialized] private int activeIndex;
                [System.NonSerialized] private bool entered;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (water == null)
                        {
                                return NodeState.Failure;
                        }
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                entered = false;
                                activeIndex = 0;
                                particleIndex = 0;
                                waveTopPoint = Vector2.zero;
                        }

                        if (water.FoundWater(root.position, false, ref waveTopPoint, ref particleIndex, force: true))
                        {
                                activeIndex = particleIndex;
                                if (!entered)
                                {
                                        entered = true;
                                        water.ApplyImpact(particleIndex, -impactEnter);
                                        onEnter.Invoke(ImpactPacket.impact.Set(onEnterWE, transform, root.world.box.collider, waveTopPoint, null, root.world.box.up, root.direction, 0));
                                }
                        }
                        else if (entered)
                        {
                                entered = false;
                                water.ApplyImpact(activeIndex, -impactExit);
                                onExit.Invoke(ImpactPacket.impact.Set(onExitWE, transform, root.world.box.collider, root.position, null, root.world.box.down, root.direction, 0));
                        }

                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public bool eventsFoldOut;
                public bool onEnterFoldOut;
                public bool onExitFoldOut;

                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "The AI will float on the waterline." +
                                        "\n \nReturns Running, Failure");
                        }

                        FoldOut.Box(3, color, extraHeight: 5, offsetY: -2);
                        {
                                parent.Field("Water", "water");
                                parent.Field("Impact Enter", "impactEnter");
                                parent.Field("Impact Exit", "impactExit");
                        }
                        if (FoldOut.FoldOutButton(parent.Get("eventsFoldOut")))
                        {
                                Fields.EventFoldOutEffect(parent.Get("onEnter"), parent.Get("onEnterWE"), parent.Get("onEnterFoldOut"), "On Enter", color: FoldOut.boxColor);
                                Fields.EventFoldOutEffect(parent.Get("onExit"), parent.Get("onExitWE"), parent.Get("onExitFoldOut"), "On Exit", color: FoldOut.boxColor);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
