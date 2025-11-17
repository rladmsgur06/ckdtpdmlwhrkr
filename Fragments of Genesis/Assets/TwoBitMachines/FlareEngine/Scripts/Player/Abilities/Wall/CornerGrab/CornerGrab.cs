#region Editor
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [System.Serializable]
        public class CornerGrab //anim signals: wall, wallHold, wallCornerGrab
        {
                [SerializeField] public GrabType grabType;
                [SerializeField] public float grabOffset;
                [SerializeField] public bool cornerGrab = false;
                [SerializeField] public string climbDown = "Down";
                [SerializeField] public ClimbType exitType = ClimbType.PlayerDirection;
                [SerializeField] public CornerGrabJump grabJump = new CornerGrabJump();
                [SerializeField] public CornerGrabAnimation grabAnimation = new CornerGrabAnimation();

                [System.NonSerialized] private Vector2 topCorner;
                public bool isGrabbing => grabAnimation.isGrabbing;
                public bool isJumping => grabJump.isJumping;

                public void Initialize (Player player)
                {
                        grabAnimation.Initialize(player);
                }

                public void Reset ()
                {
                        grabAnimation.Reset();
                        grabJump.Reset();
                }

                public void CompleteJump (AbilityManager player, ref Vector2 velocity)
                {
                        grabJump.AutoJumpToCorner(player, ref velocity);
                }

                public bool Grab (Wall wall, AbilityManager player, ref Vector2 velocity)
                {
                        int wallDirection = wall.Direction(player);

                        if (grabAnimation.IsPlaying(wall, player, player.world, ref velocity)) //                    climbing corner animation
                        {
                                return true;
                        }
                        if (!cornerGrab || isJumping || player.ground || Escape(player, wallDirection)) //           cant climb corner anymore
                        {
                                return false;
                        }

                        if (!FindTopCorner(player.world.box, wallDirection, velocity.y, ref topCorner)) //    no corner found
                        {
                                return false;
                        }

                        if (MovePlayerToCorner(player, wallDirection, topCorner.y, ref velocity))
                        {
                                TriggerCornerClimb(wall, player, wallDirection, topCorner, ref velocity);
                        }
                        return true;
                }

                private bool MovePlayerToCorner (AbilityManager player, int wallDirection, float topCornerY, ref Vector2 velocity)
                {
                        grabJump.Reset();
                        player.signals.Set("wall");
                        player.signals.Set("wallHold");
                        player.signals.Set("wallHoldGrab");
                        player.signals.Set("wallLeft", wallDirection < 0);
                        player.signals.Set("wallRight", wallDirection > 0);
                        player.signals.ForceDirection(wallDirection);

                        float distanceToCorner = topCornerY + grabOffset - player.world.box.top;
                        velocity.y = Time.deltaTime <= 0 ? 0 : (distanceToCorner / Time.deltaTime) * 0.35f; //       move towards corner 
                        velocity.x = 0.1f * wallDirection;
                        return Mathf.Abs(distanceToCorner) <= player.world.box.sizeY * 0.15f;
                }

                private void TriggerCornerClimb (Wall wall, AbilityManager player, int wallDirection, Vector2 topCorner, ref Vector2 velocity)
                {
                        if (grabType == GrabType.Jump && player.jumpButtonActive)
                        {
                                grabJump.StartJump(wall, player, wallDirection, topCorner.y, ref velocity);
                        }
                        if (grabType == GrabType.PullUpAuto || (grabType == GrabType.PullUp && player.jumpButtonActive) || wall.rotateToWall.latched)
                        {
                                wall.rotateToWall.Rectify(player);
                                grabAnimation.StartAnimation(player, wallDirection, topCorner);
                        }
                }

                public bool FindTopCorner (BoxInfo box, int wallDirection, float velocityY, ref Vector2 topCorner)
                {
                        Vector2 topCenter = box.topCenter;
                        float shift = velocityY > 0 ? velocityY * Time.deltaTime + 0.1f : 0.1f; //              shift origin up if velocity is positive to catch corner before player moves above it
                        Vector2 origin = box.TopExactCorner(wallDirection) + box.up * shift + box.right * 0.1f * wallDirection;
                        RaycastHit2D hit = Physics2D.Raycast(origin, box.down, box.sizeY * 0.75f + shift, WorldManager.collisionMask);

                        if (hit && hit.distance > 0 && Vector2.Angle(hit.normal, Vector2.up) < 1f) //          must hit and be a flat surface
                        {
                                topCorner = hit.point;
                                Vector2 corner = new Vector2(box.collider.transform.position.x, topCorner.y);
                                bool cornerHighEnough = !Physics2D.Raycast(corner, box.down, box.sizeY, WorldManager.collisionMask); //                player height must be larger than top corner relative to ground
                                bool noCeiling = !Physics2D.Raycast(hit.point + hit.normal * 0.01f, box.up, box.sizeY, WorldManager.collisionMask); // make sure there is no ceiling at landing area
                                bool noWallExists = !Physics2D.Raycast(new Vector2(topCenter.x, origin.y), box.right * wallDirection, box.sizeX, WorldManager.collisionMask); //         Important for composite colliders. Validate there is no wall at corner point
                                return cornerHighEnough && noCeiling && noWallExists;
                        }
                        return false;
                }

                public bool Escape (AbilityManager player, int wallDirection)
                {
                        if (exitType == ClimbType.None)
                                return false;

                        bool useButtons = exitType == ClimbType.Button;
                        bool downA = player.inputs.Holding(useButtons ? climbDown : "Left");
                        bool downB = player.inputs.Holding(useButtons ? climbDown : "Right");

                        if (useButtons || wallDirection == -1)
                        {
                                return downB;
                        }
                        else
                        {
                                return downA;
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool foldOut;
                [SerializeField, HideInInspector] private bool addAnimation;
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private bool onStartFoldOut;
                [SerializeField, HideInInspector] private bool onEndFoldOut;

                public static void OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        SerializedProperty cornerGrab = parent.Get("cornerGrab");
                        SerializedProperty grabAnim = cornerGrab.Get("grabAnimation");
                        int exitTypeGrab = cornerGrab.Enum("exitType");
                        int grabType = cornerGrab.Enum("grabType");
                        int height = grabType > 0 ? 1 : 0;

                        if (FoldOut.Bar(cornerGrab, FoldOut.boxColorLight).Label("Corner Grab", FoldOut.titleColor, false).BRE("cornerGrab").FoldOut())
                        {
                                GUI.enabled = cornerGrab.Bool("cornerGrab");
                                FoldOut.Box(4 + height, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);
                                {
                                        cornerGrab.Field("Type", "grabType");
                                        cornerGrab.Field("Exit Grab", "exitType", execute: exitTypeGrab > 0);
                                        cornerGrab.FieldAndDropDownList(inputList, "Exit Grab", "exitType", "climbDown", execute: exitTypeGrab == 0);
                                        grabAnim.Field("Final Position", "finalPosition");
                                        cornerGrab.Field("Grab Offset", "grabOffset");
                                        grabAnim.Field("AnimationTime", "animationTime", execute: grabType > 0);
                                }

                                if (FoldOut.FoldOutButton(cornerGrab.Get("eventsFoldOut")))
                                {
                                        Fields.EventFoldOut(grabAnim.Get("onAnimationStart"), cornerGrab.Get("onStartFoldOut"), "On Animation Start", shiftX: 0, color: FoldOut.boxColorLight);
                                        Fields.EventFoldOut(grabAnim.Get("onAnimationEnd"), cornerGrab.Get("onEndFoldOut"), "On Animation End", shiftX: 0, color: FoldOut.boxColorLight);
                                }
                                GUI.enabled = true;
                        }

                        if (cornerGrab.Bool("cornerGrab") && cornerGrab.Enum("grabType") > 0 && cornerGrab.Bool("foldOut"))
                        {
                                SerializedProperty array = grabAnim.Get("animation");
                                Layout.Update(0.70f);

                                for (int i = 0; i < array.arraySize; i++)
                                {
                                        FoldOut.Box(1, Tint.Blue);
                                        {
                                                array.Element(i).TitleIsField("sprite", "y");
                                                Labels.FieldText("Offset");
                                        }
                                        Layout.VerticalSpacing(5);
                                }
                                if (FoldOut.CornerButton(Tint.Blue)) // add state
                                {
                                        array.arraySize++;
                                }
                                if (FoldOut.CornerButton(Tint.Delete, 22, 22, icon: "Delete"))
                                {
                                        array.arraySize--;
                                }
                                Layout.VerticalSpacing(2);
                                Layout.Update();
                        }
                }

#pragma warning restore 0414
#endif
                #endregion

        }

        public enum GrabType
        {
                Jump,
                PullUp,
                PullUpAuto
        }
}
