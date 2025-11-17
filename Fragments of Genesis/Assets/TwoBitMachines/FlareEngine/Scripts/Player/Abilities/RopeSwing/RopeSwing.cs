#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.Interactables;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("")]
        public class RopeSwing : Ability //animation signal: rope, ropeClimbing, ropeHanging, ropeHolding, ropeSwinging
        {
                [SerializeField] public float swingStrength = 0.10f;
                [SerializeField] public float grabOffset = 0.5f;
                [SerializeField] public float climbSpeed = 2f;
                [SerializeField] public float jumpAway = 10f;
                [SerializeField] public float rotateRate = 0.5f;
                [SerializeField] public bool canRotate;

                [SerializeField] public string enterWE;
                [SerializeField] public string exitWE;
                [SerializeField] public UnityEventEffect onEnter;
                [SerializeField] public UnityEventEffect onExit;

                [System.NonSerialized] private float exitCounter;
                [System.NonSerialized] private float latchCounter;
                [System.NonSerialized] private bool ropeJumpExit;
                [System.NonSerialized] private bool unrotate;
                [System.NonSerialized] private bool hasJumped;
                [System.NonSerialized] private bool onRope;

                [System.NonSerialized] public Rope rope;
                [System.NonSerialized] public Rope oldRope;
                [System.NonSerialized] public int particle1;
                [System.NonSerialized] public int particle2;
                [System.NonSerialized] public float grabDistance;

                [System.NonSerialized] private RopeInteraction ropeUtil = new RopeInteraction();

                private float playerAngle => this.transform.eulerAngles.z;
                private bool onLastPoint => rope != null && (particle1 == rope.last || particle2 == rope.last);

                public override void Reset (AbilityManager player)
                {
                        if (onRope)
                        {
                                onExit.Invoke(ImpactPacket.impact.Set(exitWE, transform, player.world.boxCollider, player.world.position, null, player.world.box.down, player.playerDirection, 0));
                        }
                        exitCounter = 0;
                        latchCounter = 0;

                        ropeJumpExit = false;
                        unrotate = false;
                        hasJumped = false;
                        onRope = false;

                        oldRope = rope;
                        rope = null;
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        return true;
                }

                public override void EarlyExecute (AbilityManager player, ref Vector2 velocity)
                {
                        if (player.world.onGround || (ropeJumpExit && Clock.Timer(ref exitCounter, 0.5f)))
                        {
                                ropeJumpExit = false;
                                oldRope = null;
                        }
                        if (!onRope && unrotate && canRotate && playerAngle != 0)
                        {
                                ropeUtil.RotatePlayerToRope(player.world, player.world.box.center, Vector2.up, 3.5f); // will unrotate player

                                if (Mathf.Abs(playerAngle) < 1 || player.world.touchingASurface)
                                {
                                        this.transform.eulerAngles = Vector3.zero;
                                        unrotate = false;
                                }
                        }
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                                return false;

                        if (hasJumped && velocity.y > 0)
                        {
                                return false;
                        }
                        hasJumped = false;

                        if (onRope)
                        {
                                return true;
                        }
                        return Rope.Find(PlayerCenter(player), velocity - Vector2.up * player.gravityEffect, this);
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {

                        if (rope == null)
                        {
                                Reset(player);
                                return;
                        }
                        if (!onRope)
                        {
                                onEnter.Invoke(ImpactPacket.impact.Set(enterWE, transform, player.world.boxCollider, player.world.position, null, player.world.box.down, player.playerDirection, 0));
                        }

                        onRope = true;
                        player.signals.Set("rope");
                        ropeUtil.Set(this, rope);
                        player.world.hitInteractable = true;

                        HoldRopeHandle(player, ref velocity);
                        Jump(player, ref velocity);
                }

                private void HoldRopeHandle (AbilityManager player, ref Vector2 velocity)
                {
                        Vector2 center = PlayerCenter(player);
                        float latchRate = Clock.TimerExpired(ref latchCounter, 0.25f) ? 1f : 0.5f;
                        float scaleStrength = rope.isClimbable ? (float) particle1 / (float) rope.particle.Length : 1f;
                        rope.ApplyImpactAtEnd(player.inputX, swingStrength * scaleStrength);

                        if (rope.isClimbable)
                        {
                                bool up = player.inputs.Holding("Up");
                                bool down = player.inputs.Holding("Down");
                                Vector2 climb = up ? Vector2.up : down ? Vector2.down : Vector2.zero;
                                Vector2 newVel = ropeUtil.RopeHoldPoint(player.world, center, climb * climbSpeed * Time.deltaTime, out bool climbing) - center;
                                velocity = Time.deltaTime <= 0 ? Vector2.zero : (newVel / Time.deltaTime) * latchRate;

                                player.signals.Set("ropeClimbing", climbing);
                                player.signals.Set("ropeHanging", onLastPoint);
                                player.signals.Set("ropeHolding", !onLastPoint);
                                player.signals.Set("ropeSwinging", player.inputX != 0);
                        }
                        else // can just hang and swing
                        {
                                player.signals.Set("ropeHanging", player.inputX == 0);
                                player.signals.Set("ropeSwinging", player.inputX != 0);
                                Vector2 newVel = rope.ropeHandle.position - center;
                                velocity = Time.deltaTime <= 0 ? Vector2.zero : (newVel / Time.deltaTime) * latchRate;
                        }

                        if (canRotate)
                        {
                                ropeUtil.RotatePlayerToRope(player.world, center, ropeUtil.RopeDirection(center), rotateRate);
                                velocity = this.transform.InverseTransformDirection(velocity);
                        }

                }

                private Vector2 PlayerCenter (AbilityManager player)
                {
                        return player.world.box.center + player.world.box.up * grabOffset;
                }

                private void Jump (AbilityManager player, ref Vector2 velocity)
                {
                        if (player.jumpButtonPressed)
                        {
                                if (rope.isClimbable && player.inputX == 0)
                                {
                                        return;
                                }

                                velocity = new Vector2(velocity.x, jumpAway) + rope.ropeHandle.velocity * 10f;
                                player.CheckForAirJumps();
                                player.signals.Set("rope", false);

                                Reset(player);
                                hasJumped = true;
                                unrotate = canRotate;
                                ropeJumpExit = oldRope.isClimbable;
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool enterFoldOut;
                [SerializeField, HideInInspector] private bool exitFoldOut;
                [SerializeField, HideInInspector] private bool entryFoldOut;
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Rope Swing", barColor, labelColor))
                        {
                                FoldOut.Box(5, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);
                                {
                                        parent.Slider("Swing Strength", "swingStrength", 0.001f, 0.1f);
                                        parent.Field("Jump Force", "jumpAway");
                                        parent.Field("Grab Offset", "grabOffset");
                                        parent.Field("Climb Speed", "climbSpeed");
                                        parent.FieldAndEnable("Can Rotate", "rotateRate", "canRotate");
                                        Labels.FieldText("Rate", rightSpacing: 18);
                                }
                                if (FoldOut.FoldOutButton(parent.Get("entryFoldOut")))
                                {
                                        Fields.EventFoldOutEffect(parent.Get("onEnter"), parent.Get("enterWE"), parent.Get("enterFoldOut"), "On Enter", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOutEffect(parent.Get("onExit"), parent.Get("exitWE"), parent.Get("exitFoldOut"), "On Exit", color: FoldOut.boxColorLight);
                                }

                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
