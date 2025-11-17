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
        public class GoToPreviousState : Action
        {
                public override NodeState RunNodeLogic (Root root)
                {
                        if (root.stateNode != null)
                        {
                                root.stateNode.goToSuccess = true;
                                root.stateNode.onSuccess = root.previousStateName;
                        }
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState ()
                {
                        return false;
                }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(60, "Return to the previous state before this one. Do not use in the first state." +
                                    "\n \nReturns Success");
                        }
                        return true;
                }

#pragma warning restore 0414
#endif
                #endregion
        }

}
