#region Editor
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("")]
        public class Wall : Ability
        {
                [SerializeField] public WallType type;
                [SerializeField] public bool climbFromGround;
                [SerializeField] public WallSlide wallSlide = new WallSlide();
                [SerializeField] public WallClimb wallClimb = new WallClimb();
                [SerializeField] public CornerGrab cornerGrab = new CornerGrab();
                [SerializeField] public CornerHang cornerHang = new CornerHang();
                [SerializeField] public RotateToWall rotateToWall = new RotateToWall();

                public override void Initialize (Player player)
                {
                        cornerGrab.Initialize(player);
                }

                public override void Reset (AbilityManager player)
                {
                        cornerGrab.Reset();
                        wallSlide.Reset(player);
                        wallClimb.Reset(player);
                        rotateToWall.Reset(player);
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        if (cornerGrab.isGrabbing && !player.pushBackActive)
                        {
                                return false;
                        }
                        Reset(player);
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                        {
                                Reset(player);
                                return false;
                        }
                        if (player.ground && !cornerGrab.isGrabbing)
                        {
                                if (!climbFromGround)
                                {
                                        Reset(player);
                                        return false;
                                }
                                if (climbFromGround && !player.world.wasOnGround && !player.world.onWall)
                                {
                                        Reset(player);
                                        return false;
                                }
                                if (climbFromGround && type == WallType.CornerGrab && player.jumpButtonHold)
                                {
                                        Reset(player);
                                        return false;
                                }
                                if (player.world.missedAHorizontal && (!rotateToWall.enable || !rotateToWall.RotatedSurface(player)))
                                {
                                        Reset(player);
                                        return false;
                                }
                        }
                        return WallIsActive(player) && (rotateToWall.Available(player) || WallIsSquare(player)); // only climb right angle walls
                }

                public override void EarlyExecute (AbilityManager player, ref Vector2 velocity)
                {
                        if (cornerGrab.isJumping)
                        {
                                cornerGrab.CompleteJump(player, ref velocity);
                        }
                        else if (wallClimb.isJumping)
                        {
                                wallClimb.CompleteJump(player, ref velocity);
                        }
                        else if (wallSlide.isJumping)
                        {
                                wallSlide.CompleteJump(player, ref velocity);
                        }
                        if (wallClimb.clearBarrierY)
                        {
                                wallClimb.ClearBarrierY(player);
                        }
                        if (rotateToWall.isJumping)
                        {
                                rotateToWall.CompleteJump(player, ref velocity);
                        }
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        if (cornerGrab.Grab(this, player, ref velocity))
                        {
                                return;
                        }
                        if (cornerHang.Hang(this, player, ref velocity))
                        {
                                return;
                        }
                        if (rotateToWall.Rotate(this, player, ref velocity))
                        {
                                return;
                        }
                        if (type == WallType.Climb)
                        {
                                wallClimb.Climb(this, player, ref velocity);
                        }
                        if (type == WallType.Slide)
                        {
                                wallSlide.Slide(this, player, ref velocity);
                        }
                }

                private bool WallIsActive (AbilityManager player)
                {
                        return (player.world.onWall || cornerGrab.isGrabbing || rotateToWall.Available(player)) && !IsWallJumping() && CanClimbWall(player);
                }

                private bool IsWallJumping ()
                {
                        return wallClimb.isJumping || wallSlide.isJumping || cornerGrab.isJumping;
                }

                private bool WallIsSquare (AbilityManager player)
                {
                        return Mathf.Abs(Vector2.Dot(player.world.wallNormal, player.world.box.up)) < 0.01f;
                }

                private bool CanClimbWall (AbilityManager player)
                {
                        return player.world.wallTransform == null || (!player.world.wallTransform.CompareTag("NoClimb") && !player.world.wallTransform.CompareTag("Block"));
                }

                public int Direction (AbilityManager player)
                {
                        return rotateToWall.latched ? rotateToWall.sign : player.world.leftWall ? -1 : 1;
                }

                public override void PostCollisionExecute (AbilityManager player, Vector2 velocity)
                {
                        if (!pause && player.world.useMovingPlatform && WallIsActive(player))
                        {
                                MovingPlatform.LatchToPlatform(player.world, player.world.wallTransform, LatchMPType.Holding);
                        }
                }

                public static bool CheckForGround (BoxInfo box, float velX, float velY)
                {
                        if (velY > 0)
                        {
                                return false;
                        }

                        velY *= Time.deltaTime;

                        for (int i = 0; i < box.rays.y; i++)
                        {
                                Vector2 origin = box.bottomLeft + box.right * (box.spacing.x * i + velX * Time.deltaTime);
                                RaycastHit2D hit = Physics2D.Raycast(origin, box.down, Mathf.Abs(velY) + box.skin.y, box.world.collisionLayer);
                                if (hit && hit.distance > 0)
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414

                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Wall", barColor, labelColor))
                        {
                                int type = parent.Enum("type");
                                if (type > 1)
                                {
                                        FoldOut.Box(2, FoldOut.boxColorLight, offsetY: -2);
                                        {
                                                parent.Field("Type", "type");
                                                parent.FieldToggle("Start On Ground", "climbFromGround");
                                        }
                                        Layout.VerticalSpacing(3);
                                }
                                else
                                {
                                        FoldOut.Box(2, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);
                                        {
                                                parent.Field("Type", "type");
                                                parent.FieldToggle("Start On Ground", "climbFromGround");
                                        }
                                }

                                WallSlide.OnInspector(parent, type);
                                WallClimb.OnInspector(parent, inputList, type);
                                RotateToWall.OnInspector(parent, inputList);
                                CornerFoldOut(parent, inputList);
                                CornerGrab.OnInspector(controller, parent, inputList, barColor, labelColor);
                        }

                        return true;
                }

                private void CornerFoldOut (SerializedObject parent, string[] inputList)
                {
                        SerializedProperty cornerHang = parent.Get("cornerHang");
                        int hangExitType = cornerHang.Enum("exitType");

                        if (FoldOut.Bar(cornerHang, FoldOut.boxColorLight).Label("Corner Hang", FoldOut.titleColor, false).BRE("cornerHang").FoldOut())
                        {
                                FoldOut.Box(hangExitType == 0 ? 3 : 2, FoldOut.boxColorLight, offsetY: -2);
                                GUI.enabled = cornerHang.Bool("cornerHang");
                                cornerHang.Field("Offset", "cornerHangOffset");
                                cornerHang.Field("Exit Hang", "exitType");
                                cornerHang.DropDownDoubleList(inputList, "Up, Down", "climbUp", "climbDown", execute: hangExitType == 0);
                                GUI.enabled = true;
                                Layout.VerticalSpacing(3);
                        }
                }

#pragma warning restore 0414
#endif
                #endregion

        }

        public enum WallType
        {
                Slide,
                Climb,
                CornerGrab
        }
}
