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
        public class RailSlide : Ability
        {
                [SerializeField] public float speedBoost = 2f;
                [SerializeField] public float crouchBoost = 1.5f;
                [SerializeField] public bool crouchSpeedBoost = false;
                [SerializeField] public bool forceDirection = false;

                [SerializeField] public string enterWE;
                [SerializeField] public string exitWE;
                [SerializeField] public string slideWE;
                [SerializeField] public float onSlideRate = 0f;
                [SerializeField] public UnityEventEffect onEnter;
                [SerializeField] public UnityEventEffect onExit;
                [SerializeField] public UnityEventEffect onSlide;

                [System.NonSerialized] private float slideRateCounter;
                [System.NonSerialized] private float directionX = 1f;
                [System.NonSerialized] private float counter;
                [System.NonSerialized] private bool exitTime;
                [System.NonSerialized] private bool crouching;
                [System.NonSerialized] private bool isSliding;

                public override void Reset (AbilityManager player)
                {
                        isSliding = false;
                        exitTime = false;
                        counter = 0;
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                        {
                                return false;
                        }
                        if (isSliding && player.world.onGround && !SlideOnLayer(player))
                        {
                                exitTime = true;
                        }
                        if (isSliding)
                        {
                                return true;
                        }
                        if (SlideOnLayer(player))
                        {
                                Reset(player);
                                OnEnter(player);
                                return isSliding = true;
                        }
                        return false;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        bool onRail = SlideOnLayer(player);
                        float totalBoost = speedBoost;
                        float speed = player.walk.speed;

                        if (crouchSpeedBoost)
                        {
                                if (player.world.boxCollider.size.y != player.world.box.boxSize.y || crouching)
                                {
                                        crouching = true;
                                        totalBoost += crouchBoost;
                                }
                                if (crouching && player.world.onGround && (player.world.boxCollider.size.y == player.world.box.boxSize.y))
                                {
                                        crouching = false;
                                }
                        }

                        if (exitTime && onRail)
                        {
                                exitTime = false;
                                counter = 0;
                        }
                        if (exitTime)
                        {
                                counter += Time.deltaTime;
                                float percent = (counter / 0.5f);
                                velocity.x = Mathf.Lerp(directionX * totalBoost * speed, directionX * speed, percent);
                                if (percent >= 1)
                                {
                                        OnExit(player);
                                        Reset(player);
                                }
                        }
                        else
                        {
                                velocity.x = directionX * totalBoost * speed;
                        }
                        if (velocity.x != 0)
                        {
                                player.UpdateVelocityGround();
                        }
                        if (player.world.onGround && onSlideRate > 0 && velocity.x != 0 && Clock.Timer(ref slideRateCounter, onSlideRate))
                        {
                                ImpactPacket impact = ImpactPacket.impact.Set(slideWE, player.world.transform, player.world.boxCollider, player.world.transform.position, null, -player.world.box.right * player.playerDirection, player.playerDirection, 0);
                                onSlide.Invoke(impact);
                        }
                        player.dashBoost = totalBoost;
                        player.signals.Set("onRail");
                        player.lockVelX = true;
                }

                public bool SlideOnLayer (AbilityManager player)
                {
                        if (player.world.verticalTransform != null)// force direction from left to right, or right to left
                        {
                                bool found = false;
                                if (player.world.verticalTransform.gameObject.CompareTag("RailRight"))
                                {
                                        directionX = 1f;
                                        found = true;
                                }
                                else if (player.world.verticalTransform.gameObject.CompareTag("RailLeft"))
                                {
                                        directionX = -1f;
                                        found = true;
                                }
                                if (found && !forceDirection)
                                {
                                        directionX = player.inputX != 0 ? Mathf.Sign(player.inputX) : player.playerDirection;
                                }
                                return found;
                        }
                        return false;
                }

                private void OnEnter (AbilityManager player)
                {
                        onEnter.Invoke(ImpactPacket.impact.Set(enterWE, transform, player.world.boxCollider, player.world.position, null, -player.world.box.right * player.playerDirection, player.playerDirection, 0));
                }

                private void OnExit (AbilityManager player)
                {
                        onExit.Invoke(ImpactPacket.impact.Set(exitWE, transform, player.world.boxCollider, player.world.position, null, -player.world.box.right * player.playerDirection, player.playerDirection, 0));
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private bool enterFoldOut;
                [SerializeField, HideInInspector] private bool exitFoldOut;
                [SerializeField, HideInInspector] private bool slideFoldOut;
                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Rail Slide", barColor, labelColor))
                        {
                                FoldOut.Box(3, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);
                                {
                                        parent.Field("Speed Boost", "speedBoost");
                                        parent.FieldAndEnable("Crouch Boost", "crouchBoost", "crouchSpeedBoost");
                                        parent.FieldToggleAndEnable("Force Direction", "forceDirection");
                                }
                                if (FoldOut.FoldOutButton(parent.Get("eventsFoldOut")))
                                {
                                        Fields.EventFoldOutEffect(parent.Get("onEnter"), parent.Get("enterWE"), parent.Get("enterFoldOut"), "On Start", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOutEffect(parent.Get("onExit"), parent.Get("exitWE"), parent.Get("exitFoldOut"), "On Exit", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOutEffectAndRate(parent.Get("onSlide"), parent.Get("slideWE"), parent.Get("onSlideRate"), parent.Get("slideFoldOut"), "On Sliding", color: FoldOut.boxColor);
                                }
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
