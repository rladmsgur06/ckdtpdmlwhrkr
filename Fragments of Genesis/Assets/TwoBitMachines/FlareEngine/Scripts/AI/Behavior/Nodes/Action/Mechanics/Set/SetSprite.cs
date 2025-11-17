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
        public class SetSprite : Action
        {
                [SerializeField] public Sprite sprite;
                [SerializeField] private SpriteRenderer spriteRenderer;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (spriteRenderer == null)
                        {
                                spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                        }
                        if (spriteRenderer != null)
                        {
                                spriteRenderer.sprite = sprite;
                        }
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Set the sprite." +
                                            "\n \nReturns Success");
                        }
                        FoldOut.Box(1, color, offsetY: -2);
                        {
                                parent.Field("Sprite", "sprite");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
