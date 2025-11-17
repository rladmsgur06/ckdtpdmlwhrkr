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
        public class CollisionStatus : Conditional
        {
                [SerializeField] public ReferenceType type;
                [SerializeField] public CollisionStatusType cType;
                [SerializeField] public Character reference;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (cType == CollisionStatusType.OtherCharacter)
                        {
                                if (reference == null)
                                        return NodeState.Failure;
                                return Evaluate(reference.world) ? NodeState.Success : NodeState.Failure;
                        }
                        else
                        {
                                for (int i = 0; i < ThePlayer.Player.players.Count; i++)
                                {
                                        if (Evaluate(ThePlayer.Player.players[i].world))
                                        {
                                                return NodeState.Success;
                                        }
                                }
                        }
                        return NodeState.Failure;
                }

                public bool Evaluate (WorldCollision world)
                {
                        if (world == null)
                                return false;

                        if (type == ReferenceType.TouchingGround)
                        {
                                return world.onGround;
                        }
                        if (type == ReferenceType.TouchingWall)
                        {
                                return world.leftWall || world.rightWall;
                        }
                        if (type == ReferenceType.TouchingLeftWall)
                        {
                                return world.leftWall;
                        }
                        if (type == ReferenceType.TouchingRightWall)
                        {
                                return world.rightWall;
                        }
                        if (type == ReferenceType.TouchingCeiling)
                        {
                                return world.onCeiling;
                        }
                        if (type == ReferenceType.StandingMovingPlatform)
                        {
                                return world.mp.standing;
                        }
                        if (type == ReferenceType.CeilingMovingPlatform)
                        {
                                return world.mp.ceiling;
                        }
                        if (type == ReferenceType.HoldingMovingPlatform)
                        {
                                return world.mp.holding;
                        }
                        if (type == ReferenceType.TouchingThisTransformCeiling)
                        {
                                return world.onCeiling && world.verticalTransform != null && world.verticalTransform == this.transform;
                        }
                        if (type == ReferenceType.TouchingThisTransformGround)
                        {
                                return world.onGround && world.verticalTransform != null && world.verticalTransform == this.transform;
                        }
                        if (type == ReferenceType.TouchingThisTransformWall)
                        {
                                return world.onWall && world.wallTransform != null && world.wallTransform == this.transform;
                        }
                        if (type == ReferenceType.playerJumped)
                        {
                                return world.hasJumped;
                        }
                        if (type == ReferenceType.OnSlopeDown)
                        {
                                return world.climbingSlopeDown;
                        }
                        if (type == ReferenceType.OnSlopeUp)
                        {
                                return world.climbingSlopeUp;
                        }
                        if (type == ReferenceType.WasOnGround)
                        {
                                return world.wasOnGround;
                        }
                        if (type == ReferenceType.WasOnSlope)
                        {
                                return world.wasOnSlope;
                        }
                        if (type == ReferenceType.OnPlatformEdge)
                        {
                                return world.onGround && !world.climbingSlopeDown && !world.climbingSlopeUp && world.missedAVertical;
                        }
                        if (type == ReferenceType.PositiveVelocityY)
                        {
                                return world.oldVelocity.y > 0;
                        }
                        if (type == ReferenceType.NegativeVelocityY)
                        {
                                return world.oldVelocity.y < 0;
                        }
                        if (type == ReferenceType.IsHiding)
                        {
                                return world.wasHiding || world.isHidingExternal;
                        }
                        return false;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(35, "Check what a character/AI is interacting with.");
                        }
                        int cType = parent.Enum("cType");
                        FoldOut.Box(cType == 1 ? 3 : 2, color, offsetY: -2);
                        parent.Field("Type", "type");
                        parent.Field("For", "cType");
                        parent.Field("World Collision", "reference", execute: cType == 1);
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        public enum ReferenceType
        {
                TouchingGround,
                TouchingWall,
                TouchingLeftWall,
                TouchingRightWall,
                TouchingCeiling,
                TouchingThisTransformCeiling,
                TouchingThisTransformGround,
                TouchingThisTransformWall,
                StandingMovingPlatform,
                CeilingMovingPlatform,
                HoldingMovingPlatform,
                playerJumped,
                OnSlopeDown,
                OnSlopeUp,
                WasOnGround,
                WasOnSlope,
                OnPlatformEdge,
                PositiveVelocityY,
                NegativeVelocityY,
                IsHiding
        }

        public enum CollisionStatusType
        {
                Player,
                OtherCharacter
        }
}
