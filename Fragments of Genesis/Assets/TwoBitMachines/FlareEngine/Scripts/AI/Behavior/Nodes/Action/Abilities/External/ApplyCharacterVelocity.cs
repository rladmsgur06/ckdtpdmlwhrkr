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
        public class ApplyCharacterVelocity : Action
        {
                [SerializeField] public CollisionStatusType cType;
                [SerializeField] public Character reference;
                [SerializeField] public ApplyDirection direction;
                [SerializeField] public float scale = 0.5f;

                public override NodeState RunNodeLogic (Root root)
                {
                        bool set = false;
                        if (cType == CollisionStatusType.OtherCharacter)
                        {
                                if (reference == null)
                                        return NodeState.Failure;
                                ApplyVelocity(root, reference.initialVelocity);
                                set = true;
                        }
                        else
                        {
                                for (int i = 0; i < ThePlayer.Player.players.Count; i++)
                                {
                                        ApplyVelocity(root, ThePlayer.Player.players[i].initialVelocity);
                                        set = true;
                                }
                        }

                        return set ? NodeState.Running : NodeState.Failure;
                }

                private void ApplyVelocity (Root root, Vector2 velocity)
                {
                        if (direction == ApplyDirection.XDirection)
                        {
                                root.velocity.x = velocity.x * scale;
                        }
                        else if (direction == ApplyDirection.YDirection)
                        {
                                root.velocity.y = velocity.y * scale;
                        }
                        else
                        {
                                root.velocity = velocity * scale;
                        }
                }

                public enum ApplyDirection
                {
                        XDirection,
                        YDirection,
                        Both
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(65, "Apply a character's velocity to this AI. Warning, this may not be the character's final velocity for this frame." +
                                        "\n \nReturns Running, Failure");
                        }
                        int cType = parent.Enum("cType");
                        FoldOut.Box(cType == 1 ? 4 : 3, color, offsetY: -2);
                        parent.Field("From", "cType");
                        parent.Field("World Collision", "reference", execute: cType == 1);
                        parent.Field("Direction", "direction");
                        parent.Field("Scale Velocity", "scale");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
