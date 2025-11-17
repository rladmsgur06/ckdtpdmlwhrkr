#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class FaceTarget : Action
        {
                [SerializeField] public Blackboard target;
                [SerializeField] public FaceType type;
                [SerializeField, HideInInspector] public SpriteRenderer spriteRenderer;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (target == null)
                        {
                                return NodeState.Failure;
                        }
                        if (spriteRenderer == null)
                        {
                                spriteRenderer = transform.gameObject.GetComponent<SpriteRenderer>();
                        }

                        bool targetIsLeft = target.GetTarget().x <= root.position.x;

                        if (this.target.hasNoTargets)
                        {
                                return NodeState.Success;
                        }

                        if (type == FaceType.SpriteFlip)
                        {
                                if (spriteRenderer == null)
                                {
                                        return NodeState.Failure;
                                }
                                if (targetIsLeft)
                                {
                                        spriteRenderer.flipX = true;
                                        root.direction = -1;
                                }
                                else if (!targetIsLeft)
                                {
                                        spriteRenderer.flipX = false;
                                        root.direction = 1;
                                }
                        }
                        else
                        {
                                Vector3 angle = transform.localEulerAngles;
                                if (targetIsLeft && transform.localEulerAngles.y == 0)
                                {
                                        transform.localEulerAngles = new Vector3(angle.x, 180f, angle.z);
                                        root.direction = -1;
                                }
                                else if (!targetIsLeft && transform.localEulerAngles.y != 0)
                                {
                                        transform.localEulerAngles = new Vector3(angle.x, 0f, angle.z);
                                        root.direction = 1;
                                }
                        }
                        return NodeState.Success;
                }

                public enum FaceType
                {
                        SpriteFlip,
                        FlipByAngle
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override void OnSceneGUI (UnityEditor.Editor editor)
                {
                        if (spriteRenderer == null)
                        {
                                spriteRenderer = transform.gameObject.GetComponent<SpriteRenderer>();
                        }
                }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "The AI will point towards the specified target in the x-axis." +
                                        "\n \nReturns Success, Failure");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        AIBase.SetRef(ai.data, parent.Get("target"), 0);
                        parent.Field("FaceType", "type");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

}
