#region 
#if UNITY_EDITOR
using System.Threading;
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class KillAI : Action
        {
                [SerializeField] private float duration = 5f;

                [System.NonSerialized] private float counter = 0;
                [System.NonSerialized] private Collider2D collider2DRef;
                [System.NonSerialized] private SpriteRenderer spriteRenderer;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                if (collider2DRef == null)
                                        collider2DRef = this.gameObject.GetComponent<Collider2D>();
                                if (spriteRenderer == null)
                                        spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
                                if (collider2DRef != null)
                                        collider2DRef.enabled = false;
                                if (spriteRenderer != null)
                                        spriteRenderer.enabled = false;
                                root.pauseCollision = true;
                                counter = 0;

                        }
                        root.velocity = Vector2.zero;

                        if (TwoBitMachines.Clock.Timer(ref counter, duration))
                        {
                                root.pauseCollision = false;
                                if (collider2DRef != null)
                                        collider2DRef.enabled = true;
                                if (spriteRenderer != null)
                                        spriteRenderer.enabled = true;
                                return NodeState.Success;
                        }
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Disable the AI's Collider and Sprite Renderer." +
                                        "\n \nReturns Running, Success");
                        }
                        FoldOut.Box(1, color, offsetY: -2);
                        parent.Field("Duration", "duration");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
