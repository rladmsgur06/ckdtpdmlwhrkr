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
        public class UntilFail : Decorator
        {
                public override NodeState RunNodeLogic (Root root)
                {
                        if (children.Count == 0)
                                return NodeState.Success;

                        NodeState nodeState = children[0].RunChild(root);

                        return nodeState == NodeState.Failure ? NodeState.Success : NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai , SerializedObject parent , Color color , bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(35 ,
                                        "This will run the child node until it returns Failure."
                                );
                        }

                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
