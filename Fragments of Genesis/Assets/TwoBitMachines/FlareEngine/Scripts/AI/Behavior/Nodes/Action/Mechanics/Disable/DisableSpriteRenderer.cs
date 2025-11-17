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
        public class DisableSpriteRenderer : Action
        {
                [SerializeField] private SpriteRenderer reference;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (reference == null)
                        {
                                reference = gameObject.GetComponent<SpriteRenderer>();
                        }
                        if (reference == null)
                        {
                                return NodeState.Failure;
                        }
                        reference.enabled = false;
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
                                Labels.InfoBoxTop(45 , "Disable this SpriteRenderer." +
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
