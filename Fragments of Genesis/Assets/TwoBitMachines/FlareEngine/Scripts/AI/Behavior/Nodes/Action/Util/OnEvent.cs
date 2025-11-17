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
        public class OnEvent : Action
        {
                [SerializeField] public UnityEvent onEvent;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                onEvent.Invoke();
                        }
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Invoke an event. " +
                                        "\n \nReturns Success");
                        }
                        Fields.EventFoldOut(parent.Get("onEvent"), "Event", color: color, offsetY: -2);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
