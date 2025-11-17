#region
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class Timer : Decorator
        {
                [SerializeField] public ClockRandom type;
                [SerializeField] public float wait = 1f;
                [SerializeField] public float min = 2f;
                [SerializeField] public float max = 5f;
                [SerializeField] public string waitAnimation;
                [SerializeField] public UnityEvent onBeginClock;
                [System.NonSerialized] public float counter;
                [System.NonSerialized] public float actualWait;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (children.Count == 0)
                                return NodeState.Failure;

                        NodeState originalState = children[0].RunChild(root);

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                counter = 0;
                                actualWait = type == ClockRandom.Fixed ? wait : Random.Range(min, max);
                                onBeginClock.Invoke();
                        }
                        root.signals.Set(waitAnimation);
                        if (TwoBitMachines.Clock.Timer(ref counter, actualWait))
                        {
                                return NodeState.Success;
                        }
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool eventFoldout;
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(67,
                                        "This will execute a child node for the specified time. If randomized, a new time is chosen each new clock cycle. The on begin event will execute during the first frame. The wait animation signal is optional."
                                );
                        }
                        int index = parent.Enum("type");
                        FoldOut.Box(3, color, offsetY: -2);
                        parent.Field("Type", "type");
                        parent.Field("Wait", "wait", execute: index == 0);
                        parent.FieldDouble("Range", "min", "max", execute: index == 1);
                        parent.Field("Wait Animation", "waitAnimation");
                        Layout.VerticalSpacing(3);
                        Fields.EventFoldOut(parent.Get("onBeginClock"), parent.Get("eventFoldout"), "On Begin Clock", color: color);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
