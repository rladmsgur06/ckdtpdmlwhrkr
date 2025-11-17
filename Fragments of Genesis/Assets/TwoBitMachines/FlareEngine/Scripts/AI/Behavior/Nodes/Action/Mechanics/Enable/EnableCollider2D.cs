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
        public class EnableCollider2D : Action
        {
                [SerializeField] private Collider2D reference;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (reference == null)
                        {
                                reference = gameObject.GetComponent<Collider2D>();
                        }
                        if (reference == null)
                        {
                                return NodeState.Failure;
                        }
                        reference.enabled = true;
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai , SerializedObject parent , Color color , bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45 , "Enable this collider2D." +
                                        "\n \nReturns Success, Failure");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
