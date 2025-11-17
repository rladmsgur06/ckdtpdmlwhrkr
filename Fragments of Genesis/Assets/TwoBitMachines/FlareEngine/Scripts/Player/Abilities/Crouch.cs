#region
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("")]
        public class Crouch : Ability
        {
                [SerializeField] public string button = "Down";
                [SerializeField] public float height = 1;
                [SerializeField] public float offset = 0;

                [SerializeField] public bool crawl;
                [SerializeField] public float crawlFriction = 1;
                [SerializeField] public float maxCrawlSpeed = 8f;

                [SerializeField] public bool crouchJump;
                [SerializeField] public float jumpScale = 0.5f;
                [SerializeField] public float jumpBoost = 1;

                [SerializeField] public bool canJumpBoost;
                [SerializeField] public float chargeTime = 0;
                [SerializeField] public UnityEventFloat onHighJump;

                [SerializeField] public string enterWE;
                [SerializeField] public string exitWE;
                [SerializeField] public UnityEventEffect onEnter;
                [SerializeField] public UnityEventEffect onExit;

                [System.NonSerialized] private float chargeCounter = 0;
                [System.NonSerialized] private bool hasCrouchJumped;
                [System.NonSerialized] private bool hasCrouched;

                public override void Reset (AbilityManager player)
                {
                        chargeCounter = 0;
                        hasCrouchJumped = hasCrouched = false;
                        if (player.world.boxCollider.size.y != player.world.box.boxSize.y)
                        {
                                player.world.box.ColliderReset();
                        }
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        if (player.world.boxCollider.size.y == player.world.box.boxSize.y || !hasCrouched)
                        {
                                chargeCounter = 0;
                                hasCrouchJumped = hasCrouched = false;
                                return true;
                        }
                        else if (SafelyStandUp(player, player.world.box))
                        {
                                if (hasCrouched)
                                {
                                        player.world.box.ColliderReset();
                                }
                                chargeCounter = 0;
                                hasCrouchJumped = hasCrouched = false;
                                return true;
                        }
                        return false;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                                return false;

                        if (player.inputs.Holding(button) && player.ground && !IsEdge2D(player.world))
                        {
                                return true;
                        }
                        if (hasCrouched || hasCrouchJumped)
                        {
                                return true;
                        }
                        return false;
                }

                public static bool IsEdge2D (WorldCollision world)
                {
                        Collider2D edge = world.verticalCollider;
                        if (edge == null || !(edge is EdgeCollider2D))
                        {
                                return false;
                        }
                        if ((world.skipDownEdge == WorldCollision.Edge2DSkip.OnDownHold && world.holdingDown) || (world.skipDownEdge == WorldCollision.Edge2DSkip.OnDownAndJump && world.pressedDown))
                        {
                                return !edge.CompareTag("Edge2DUpOnly") && !edge.CompareTag("Beam");
                        }
                        return false;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        if (player.inputs.Holding(button) && (player.ground || velocity.y < 0 || hasCrouchJumped))
                        {
                                if (player.world.boxCollider.size.y != height) // CROUCH
                                {
                                        hasCrouched = true;
                                        hasCrouchJumped = false;
                                        player.world.box.ChangeColliderHeight(height, -offset);
                                        chargeCounter = 0;
                                        onEnter.Invoke(ImpactPacket.impact.Set(enterWE, transform, player.world.boxCollider, player.world.position, null, player.world.box.down, player.playerDirection, 0));
                                }
                                Crawl(player, ref velocity);
                                CanJump(player, ref velocity);
                        }
                        else if (player.world.boxCollider.size.y != player.world.box.boxSize.y)
                        {
                                if (SafelyStandUp(player, player.world.box))
                                {
                                        hasCrouchJumped = hasCrouched = false;
                                        player.world.box.ColliderReset();
                                }
                                else
                                {
                                        Crawl(player, ref velocity); // if not safe to stand, then  player can still crawl
                                }
                        }
                        else
                        {
                                hasCrouched = false;
                        }
                }

                private void Crawl (AbilityManager player, ref Vector2 velocity)
                {
                        if (player.lockVelX)
                        {
                                return;
                        }
                        player.signals.Set("crouch");
                        if (crawl && velocity.x != 0)
                        {
                                velocity.x *= crawlFriction;
                                velocity.x = Mathf.Clamp(velocity.x, -maxCrawlSpeed, maxCrawlSpeed);
                                player.signals.Set("crouchWalk");
                        }
                        else
                        {
                                velocity.x = 0;
                        }
                        if (velocity.x != 0)
                        {
                                player.UpdateVelocityGround();
                        }
                }

                private void CanJump (AbilityManager player, ref Vector2 velocity)
                {
                        //high jump
                        if (canJumpBoost && player.ground && velocity.x == 0)
                        {
                                if (Clock.TimerExpired(ref chargeCounter, chargeTime) && player.jumpButtonActive && SafelyStandUp(player, player.world.box))
                                {
                                        hasCrouchJumped = false;
                                        player.hasJumped = true;
                                        velocity = new Vector2(0, player.maxJumpVel * jumpBoost);
                                        return;
                                }
                                if (chargeTime > 0)
                                {
                                        onHighJump.Invoke(chargeCounter / chargeTime);
                                }
                        }
                        else
                        {
                                chargeCounter = 0;
                        }
                        //crouch jump
                        if (crouchJump && player.ground && player.jumpButtonActive && SafelyStandUp(player, player.world.box))
                        {
                                hasCrouchJumped = true;
                                player.hasJumped = true;
                                velocity = new Vector2(0, player.maxJumpVel * jumpScale);
                        }
                }

                public bool SafelyStandUp (AbilityManager player, BoxInfo box)
                {
                        float length = Mathf.Abs(box.boxSize.y - box.collider.size.y) * box.collider.transform.localScale.y;
                        for (int i = 0; i < box.rays.y; i++)
                        {
                                Vector2 origin = box.cornerTopLeft + box.right * (box.spacing.x * i);
                                #region Debug
#if UNITY_EDITOR
                                if (WorldManager.viewDebugger)
                                {
                                        Debug.DrawRay(origin, box.up * length, Color.white);
                                }
#endif
                                #endregion
                                RaycastHit2D hit = Physics2D.Raycast(origin, box.up, length, WorldManager.collisionMask);
                                if (hit && hit.distance == 0 && hit.transform.gameObject.layer == WorldManager.platformLayer)
                                {
                                        continue;
                                }
                                if (hit)
                                {
                                        return false;
                                }
                        }
                        onExit.Invoke(ImpactPacket.impact.Set(exitWE, transform, box.world.boxCollider, box.world.position, null, box.world.box.down, player.playerDirection, 0));
                        return true;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private bool crawlFoldOut;
                [SerializeField, HideInInspector] private bool jumpFoldOut;
                [SerializeField, HideInInspector] private bool eventFoldOut;
                [SerializeField, HideInInspector] private bool crouchFoldOut;
                [SerializeField, HideInInspector] private bool enterFoldOut;
                [SerializeField, HideInInspector] private bool exitFoldOut;
                [SerializeField, HideInInspector] private bool entryFoldOut;
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Crouch", barColor, labelColor))
                        {
                                FoldOut.Box(2, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);
                                {
                                        parent.DropDownList(inputList, "Button", "button");
                                        parent.FieldDouble("Crouch Height", "height", "offset");
                                        Labels.FieldText("Offset");
                                }
                                if (FoldOut.FoldOutButton(parent.Get("entryFoldOut")))
                                {
                                        Fields.EventFoldOutEffect(parent.Get("onEnter"), parent.Get("enterWE"), parent.Get("enterFoldOut"), "On Crouch", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOutEffect(parent.Get("onExit"), parent.Get("exitWE"), parent.Get("exitFoldOut"), "On Exit", color: FoldOut.boxColorLight);
                                }

                                if (FoldOut.Bar(parent, FoldOut.boxColorLight).Label("Crawl", FoldOut.titleColor, false).BRE("crawl").FoldOut("crawlFoldOut"))
                                {
                                        GUI.enabled = parent.Bool("crawl");
                                        FoldOut.Box(2, FoldOut.boxColorLight, offsetY: -2);
                                        parent.Field("Speed Scale", "crawlFriction");
                                        parent.Field("Speed Max", "maxCrawlSpeed");
                                        GUI.enabled = true;
                                        Layout.VerticalSpacing(3);
                                }

                                if (FoldOut.Bar(parent, FoldOut.boxColorLight).Label("Jump", FoldOut.titleColor, false).BRE("crouchJump").FoldOut("crouchFoldOut"))
                                {
                                        GUI.enabled = parent.Bool("crouchJump");
                                        FoldOut.Box(1, FoldOut.boxColorLight, offsetY: -2);
                                        parent.Slider("Jump Scale", "jumpScale", 0.1f, 1f);
                                        GUI.enabled = true;
                                        Layout.VerticalSpacing(3);
                                }
                                if (FoldOut.Bar(parent, FoldOut.boxColorLight).Label("High Jump", FoldOut.titleColor, false).BRE("canJumpBoost").FoldOut("jumpFoldOut"))
                                {
                                        GUI.enabled = parent.Bool("canJumpBoost");
                                        FoldOut.Box(2, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);
                                        parent.Field("Jump Boost", "jumpBoost");
                                        parent.Slider("Charge Time", "chargeTime", 0.5f, 5f);

                                        bool eventOpen = FoldOut.FoldOutButton(parent.Get("eventsFoldOut"));
                                        Fields.EventFoldOut(parent.Get("onHighJump"), parent.Get("eventFoldOut"), "On High Jump", execute: eventOpen, color: FoldOut.boxColorLight);
                                        GUI.enabled = true;
                                        Layout.VerticalSpacing(3);
                                }
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
