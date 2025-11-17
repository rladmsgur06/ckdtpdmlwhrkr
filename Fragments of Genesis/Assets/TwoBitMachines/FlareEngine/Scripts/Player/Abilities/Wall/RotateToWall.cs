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
        public class RotateToWall
        {
                [SerializeField] public bool enable;
                [SerializeField] public float speed = 3.5f;
                [SerializeField] public bool canJump;
                [SerializeField] public float jumpAway = 15f;
                [SerializeField] public float pressRate = 0.25f;

                [SerializeField] public ClimbType climbType;
                [SerializeField] private string climbUp = "Up";
                [SerializeField] private string climbDown = "Down";
                [SerializeField] private string pressToClimb = "None";

                [SerializeField] public float climbRate;
                [SerializeField] public string climbWE;
                [SerializeField] public UnityEventEffect onClimb;

                [System.NonSerialized] public int sign;
                [System.NonSerialized] public bool latched = false;
                [System.NonSerialized] public bool isJumping = false;
                [System.NonSerialized] private float latchCounter;
                [System.NonSerialized] private float climbRateCounter;
                [System.NonSerialized] private float pressRateCounter;

                public void Reset (AbilityManager player)
                {
                        Rectify(player);
                        latched = false;
                        isJumping = false;
                        pressRateCounter = 0;
                }

                public void Rectify (AbilityManager player)
                {
                        if (latched || isJumping)
                        {
                                player.world.transform.localEulerAngles = new Vector3(0, 0, 0);
                        }
                }

                public bool Available (AbilityManager player)
                {
                        return enable && (latched || RotatedSurface(player));
                }

                public bool RotatedSurface (AbilityManager player)
                {
                        if (player.world.wallTransform != null && player.world.wallTransform.CompareTag("RotatedWall"))
                        {
                                return true;
                        }
                        if (player.world.verticalTransform != null && player.world.verticalTransform.CompareTag("RotatedWall"))
                        {
                                BoxInfo box = player.world.box;
                                int tempSign = player.playerDirection;
                                Vector2 direction = box.right * tempSign;
                                RaycastHit2D hit = Physics2D.Raycast(box.center, direction, box.size.x * 1.25f, WorldManager.collisionMask);
                                if (hit && hit.distance > 0 && hit.transform.CompareTag("RotatedWall"))
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                public bool Rotate (Wall wall, AbilityManager player, ref Vector2 velocity)
                {
                        if (!enable)
                        {
                                return false;
                        }
                        if (!latched && !RotatedSurface(player))
                        {
                                return false;
                        }

                        BoxInfo box = player.world.box;
                        int tempSign = latched ? sign : player.playerDirection;
                        Vector2 direction = box.right * tempSign;
                        RaycastHit2D hit = Physics2D.Raycast(box.center, direction, box.size.x * 1.25f, WorldManager.collisionMask);
                        // Debug.DrawRay (box.center, box.right * tempSign * box.size.x * 1.25f, Color.red);

                        if (hit && hit.distance > 0)
                        {
                                if (!latched)
                                {
                                        player.signals.ForceDirection(player.playerDirection);
                                        sign = player.playerDirection;
                                        climbRateCounter = 1000f;
                                        latchCounter = 0.25f;
                                        isJumping = false;
                                        latched = true;
                                }

                                Clock.TimerExpired(ref latchCounter, 1f);
                                Vector2 side = box.center + direction * box.size.x * 0.52f;
                                player.world.transform.position += (Vector3) (hit.point - side) * (latchCounter / 1f);
                                RotatePlayer(player, hit.point, -direction, hit.normal);
                                GroundCheck(player, ref velocity);
                                player.world.transform.Translate(Vector3.up * velocity.y * Time.deltaTime);

                                bool climbing = velocity.y != 0;
                                player.world.SetWall(hit, sign);
                                player.signals.Set("wallRotate");
                                player.signals.Set("wallRotateHold", !climbing);
                                player.signals.Set("wallRotateClimb", climbing);
                                player.signals.Set("wallRotateClimbDown", climbing && velocity.y < 0);
                                velocity = Vector2.zero;

                                if (climbRate > 0 && climbing && Clock.Timer(ref climbRateCounter, climbRate))
                                {
                                        float wallDirection = sign < 0 ? -1f : 1f;
                                        ImpactPacket impact = ImpactPacket.impact.Set(climbWE, player.world.transform, player.world.boxCollider, player.world.transform.position, null, player.world.box.right * wallDirection, player.playerDirection, 0);
                                        onClimb.Invoke(impact);
                                }
                        }
                        else
                        {
                                Reset(player);
                        }

                        if (latched && Jump(player, ref velocity))
                        {
                                return false;
                        }
                        return latched;
                }

                private bool Jump (AbilityManager player, ref Vector2 velocity)
                {
                        if (!canJump || player.inputX == 0 || !Compute.SameSign(-sign, player.inputX))
                        {
                                return false;
                        }
                        if (player.jumpButtonPressed && Vector2.Angle(player.world.box.up, Vector2.up) <= 50)
                        {
                                velocity = new Vector2(player.inputX * player.speed, jumpAway);
                                player.CheckForAirJumps();

                                latched = false;
                                isJumping = true;
                                player.world.mp.Launch(ref velocity);
                                return true;
                        }
                        return false;
                }

                private void RotatePlayer (AbilityManager player, Vector2 pivot, Vector2 direction, Vector2 slopeNormal, float rotateRate = 15f)
                {
                        float angle = Vector2.Angle(direction, slopeNormal);
                        float maxAngle = Mathf.Clamp(angle * Time.deltaTime * rotateRate, 0, angle);
                        maxAngle = angle > 0 && angle < 1.5f ? angle : maxAngle;
                        player.world.transform.RotateAround(pivot, Vector3.forward, maxAngle * direction.CrossSign(slopeNormal));
                }

                public void CompleteJump (AbilityManager player, ref Vector2 velocity)
                {
                        if (!isJumping)
                                return;

                        BoxInfo box = player.world.box;
                        if (player.world.onGround)
                        {
                                RotatePlayer(player, box.bottomCenter, box.up, Vector2.up, 20f);
                        }
                        else
                        {
                                RotatePlayer(player, box.center, box.up, Vector2.up, 8f);
                        }
                        if (Vector2.Angle(box.up, Vector2.up) == 0)
                        {
                                Reset(player);
                        }
                        if (velocity.y <= 0 && (player.world.onGround || player.world.wasOnGround))
                        {
                                velocity.y -= 15f; // prevents glitchy alternating animations from jump to idle, avoid
                        }
                }

                public float Inputs (AbilityManager player, int wallDirection)
                {
                        if (climbType == ClimbType.None)
                        {
                                return 0;
                        }

                        bool useButtons = climbType == ClimbType.Button;
                        bool up = player.inputs.Holding(useButtons ? climbUp : "Left");
                        bool down = player.inputs.Holding(useButtons ? climbDown : "Right");

                        if (!useButtons && !up && player.inputX < 0)
                        {
                                up = true;
                        }
                        if (!useButtons && !down && player.inputX > 0)
                        {
                                down = true;
                        }
                        if (useButtons || wallDirection == -1)
                        {
                                return (up ? speed : 0) + (down ? -speed : 0);
                        }
                        else
                        {
                                return (up ? -speed : 0) + (down ? speed : 0);
                        }
                }

                private void GroundCheck (AbilityManager player, ref Vector2 velocity)
                {
                        velocity.y = 0;

                        if (pressToClimb != "None")
                        {
                                pressRateCounter += Time.deltaTime;
                                if (player.inputs.Pressed(pressToClimb))
                                {
                                        pressRateCounter = 0;
                                }
                                if (pressRateCounter >= pressRate)
                                {
                                        return;
                                }
                        }

                        velocity.y = Inputs(player, (int) this.sign);

                        if (velocity.y == 0)
                                return;

                        WorldCollision world = player.world;
                        BoxInfo box = world.box;
                        world.box.Update();

                        bool hitSurface = false;
                        float sign = Mathf.Sign(velocity.y);
                        float magnitude = Mathf.Abs(velocity.y) * Time.deltaTime + box.skin.y;
                        Vector2 corner = sign > 0 ? box.topLeft : box.bottomLeft;

                        for (int i = 0; i < box.rays.y; i++)
                        {
                                Vector2 origin = corner + box.right * (box.spacing.x * i);
                                RaycastHit2D hit = Physics2D.Raycast(origin, box.up * sign, magnitude, world.collisionLayer);

                                if (!hit || hit.transform.CompareTag("RotatedWall") || (hit.collider is EdgeCollider2D && world.IgnoreEdge2D(hit.transform, sign)))
                                {
                                        continue;
                                }

                                magnitude = Mathf.Max(hit.distance, box.skin.y);
                                velocity.y = (hit.distance - box.skin.y) * sign;

                                hitSurface = true;
                                world.onGround = sign <= 0;
                                world.groundNormal = hit.normal;
                                world.verticalTransform = hit.transform;
                        }

                        if (hitSurface && sign < 0)
                        {
                                Reset(player);
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool foldOut;
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private bool climbFoldOut;

                public static void OnInspector (SerializedObject parent, string[] inputList)
                {
                        SerializedProperty rotateToWall = parent.Get("rotateToWall");

                        if (FoldOut.Bar(rotateToWall, FoldOut.boxColorLight).Label("Rotate To Wall", FoldOut.titleColor, false).BRE("enable").FoldOut())
                        {
                                int climbType = rotateToWall.Enum("climbType");
                                int height = climbType == 0 ? 1 : 0;
                                GUI.enabled = rotateToWall.Bool("enable");
                                FoldOut.Box(4 + height, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);
                                {
                                        rotateToWall.Field("Buttons", "climbType");
                                        rotateToWall.DropDownDoubleList(inputList, "Up, Down", "climbUp", "climbDown", execute: climbType == 0);
                                        rotateToWall.FieldAndDropDownList(inputList, "Press To Climb", "pressRate", "pressToClimb");
                                        Labels.FieldText("Rate", rightSpacing: Layout.contentWidth * 0.5f);// Press Rate
                                        rotateToWall.Field("Climb Speed", "speed");
                                        rotateToWall.FieldAndEnable("Jump Away", "jumpAway", "canJump");
                                }

                                if (FoldOut.FoldOutButton(rotateToWall.Get("eventsFoldOut")))
                                {
                                        Fields.EventFoldOutEffectAndRate(rotateToWall.Get("onClimb"), rotateToWall.Get("climbWE"), rotateToWall.Get("climbRate"), rotateToWall.Get("climbFoldOut"), "On Climb", color: FoldOut.boxColorLight);
                                }

                                GUI.enabled = true;
                        }
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
