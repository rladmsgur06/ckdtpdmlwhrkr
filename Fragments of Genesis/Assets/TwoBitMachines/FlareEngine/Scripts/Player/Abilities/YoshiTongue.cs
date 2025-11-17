#region
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using System.Collections.Generic;
using TwoBitMachines.FlareEngine.AI;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("")]
        public class YoshiTongue : Ability
        {
                [SerializeField] public LayerMask layer;
                [SerializeField] public string button;
                [SerializeField] public float speed = 20f;
                [SerializeField] public float tongueLength = 5f;
                [SerializeField] public float closingLength = 1f;
                [SerializeField] public float animationTimer = 0.20f;
                [SerializeField] public bool stopVelX;
                [SerializeField] public bool deactivateTarget = true;
                [SerializeField] public float targetOffset = 0.5f;
                [SerializeField] public List<Firearm> weapons = new List<Firearm>();

                [SerializeField] public Transform tongueTip;
                [SerializeField] public Transform tongue; // sprite renderer needs to be set to bottom left
                [SerializeField] public SpriteRenderer tongueSprite;

                [SerializeField] public UnityEvent onBegin;
                [SerializeField] public UnityEvent onEnd;
                [SerializeField] public UnityEvent onClose;
                [SerializeField] public UnityEvent onFail;
                [SerializeField] public UnityEvent onShoot;

                [System.NonSerialized] private SpriteRenderer targetSprite;
                [System.NonSerialized] private Collider2D targetCollider;
                [System.NonSerialized] private Transform targetTransform;
                [System.NonSerialized] private CapturedByYoshi yoshiTarget;
                [System.NonSerialized] private AIBase aiTarget;
                [System.NonSerialized] private State state;

                [System.NonSerialized] private float length = 0;
                [System.NonSerialized] private float counter = 0;
                [System.NonSerialized] private float spriteWidth = 1f;
                [System.NonSerialized] private bool closedTriggered;

                private enum State
                {
                        Wait,
                        Start,
                        Extract,
                        Retract,
                        End,
                        Shoot,
                        ShootRelease
                }

                private void Awake ()
                {
                        if (tongue != null)
                        {
                                tongueSprite = tongue.GetComponent<SpriteRenderer>();
                                spriteWidth = tongueSprite != null ? tongueSprite.bounds.size.x : 1f;
                        }
                }

                public override void Reset (AbilityManager player)
                {
                        counter = 0;
                        state = State.Wait;
                        targetTransform = null;
                        closedTriggered = false;
                        if (tongue != null)
                                tongue.gameObject.SetActive(false);
                        if (tongueTip != null)
                                tongueTip.gameObject.SetActive(false);
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                                return false;

                        if (state == State.ShootRelease)
                        {
                                player.signals.Set("yoshiShoot");
                                if (weapons.Count == 0 || Clock.Timer(ref counter, animationTimer))
                                {
                                        player.signals.Set("yoshiShoot", false);
                                        state = State.Wait;
                                }
                                else
                                        return false;
                        }
                        if (state == State.Shoot)
                        {
                                if (yoshiTarget == null || yoshiTarget.weapon < 0)
                                {
                                        state = State.Wait;
                                }
                                else
                                {
                                        player.signals.Set("yoshiFull");
                                        if (player.inputs.Pressed(button))
                                        {
                                                for (int i = 0; i < weapons.Count; i++)
                                                        if (i == yoshiTarget.weapon)
                                                        {
                                                                onShoot.Invoke();
                                                                weapons[i].Shoot();
                                                                break;
                                                        }
                                                counter = 0;
                                                yoshiTarget = null;
                                                state = State.ShootRelease;
                                                player.signals.Set("yoshiFull", false);
                                                player.signals.Set("yoshiShoot");
                                        }
                                        return false;
                                }
                        }
                        if (state == State.Start || state == State.Extract || state == State.Retract || state == State.End)
                        {
                                return true;
                        }
                        if (player.inputs.Pressed(button))
                        {
                                counter = 0;
                                state = State.Start;
                                return true;
                        }
                        return false;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        if (tongueTip == null || tongue == null)
                        {
                                state = State.Wait;
                                return;
                        }

                        switch (state)
                        {
                                case State.Start:
                                {
                                        if (stopVelX)
                                                velocity.x = 0;
                                        player.signals.Set("yoshiExtend");
                                        if (Clock.Timer(ref counter, animationTimer))
                                        {
                                                length = 0;
                                                counter = 0;
                                                targetTransform = null;
                                                closedTriggered = false;
                                                onBegin.Invoke();
                                                tongue.gameObject.SetActive(true);
                                                tongueTip.gameObject.SetActive(true);
                                                state = State.Extract;
                                        }
                                }
                                break;
                                case State.Extract:
                                {
                                        if (stopVelX)
                                                velocity.x = 0;
                                        player.signals.Set("yoshiExtend");
                                        Vector3 direction = Vector3.right * player.signals.characterDirection;
                                        length = Mathf.MoveTowards(length, tongueLength, Time.deltaTime * speed);

                                        float localLength = length / spriteWidth * direction.x * this.transform.localScale.x;
                                        tongue.localScale = new Vector3(localLength, tongue.localScale.y, 1f);

                                        Vector3 tongueLP = TonguePosition(direction.x);
                                        tongueTip.localPosition = tongueLP + direction * length;

                                        RaycastHit2D hit = Physics2D.Linecast(tongue.position, tongueTip.position, layer);
                                        //Debug.DrawLine (tongue.position, tongueTip.position, Color.blue);

                                        if (hit)
                                        {

                                                yoshiTarget = hit.transform.GetComponent<CapturedByYoshi>();
                                                if (yoshiTarget != null)
                                                {
                                                        targetTransform = hit.transform;
                                                        aiTarget = targetTransform.GetComponent<AIBase>();
                                                        targetSprite = targetTransform.GetComponent<SpriteRenderer>();
                                                        targetCollider = targetTransform.GetComponent<Collider2D>();
                                                        if (aiTarget != null)
                                                                aiTarget.ChangeState("Captured");
                                                        if (targetCollider != null)
                                                                targetCollider.enabled = false;
                                                }
                                                else
                                                {
                                                        onFail.Invoke();
                                                }
                                        }
                                        if (hit || length >= tongueLength)
                                        {
                                                state = State.Retract;
                                        }
                                }
                                break;
                                case State.Retract:
                                {
                                        if (stopVelX)
                                                velocity.x = 0;
                                        player.signals.Set("yoshiExtend");
                                        Vector3 direction = Vector3.right * player.signals.characterDirection;
                                        length = Mathf.MoveTowards(length, 0, Time.deltaTime * speed);

                                        float localLength = length / spriteWidth * direction.x * this.transform.localScale.x;
                                        tongue.localScale = new Vector3(localLength, tongue.localScale.y, 1f);

                                        tongueTip.localPosition = TonguePosition(direction.x) + direction * length;

                                        if (targetTransform != null)
                                        {
                                                targetTransform.position = tongueTip.position - Vector3.up * targetOffset;
                                                if (aiTarget != null)
                                                        aiTarget.enabled = false;

                                                if (length <= closingLength)
                                                {
                                                        if (aiTarget != null)
                                                                aiTarget.enabled = true;
                                                        if (aiTarget != null)
                                                                aiTarget.ChangeState("Killed");
                                                        if (deactivateTarget)
                                                                targetTransform.gameObject.SetActive(false);
                                                        if (!closedTriggered)
                                                        {
                                                                closedTriggered = true;
                                                                onClose.Invoke();
                                                        }
                                                }
                                        }
                                        if (length <= 0)
                                        {
                                                state = State.End;
                                                onEnd.Invoke();
                                                if (tongue != null)
                                                        tongue.gameObject.SetActive(false);
                                                if (tongueTip != null)
                                                        tongueTip.gameObject.SetActive(false);
                                        }
                                }
                                break;
                                case State.End:
                                {
                                        if (stopVelX)
                                                velocity.x = 0;
                                        player.signals.Set("yoshiRetract");
                                        if (Clock.Timer(ref counter, animationTimer))
                                        {
                                                Reset(player);
                                                if (weapons.Count > 0 && yoshiTarget != null && yoshiTarget.weapon >= 0)
                                                {
                                                        state = State.Shoot;
                                                }
                                        }
                                }
                                break;
                        }
                }

                private Vector3 TonguePosition (float directionX)
                {
                        Vector3 t = tongue.localPosition;
                        return tongue.transform.localPosition = directionX > 0 ? new Vector3(Mathf.Abs(t.x), t.y) : new Vector3(-Mathf.Abs(t.x), t.y);
                }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool onBeginFoldOut;
                [SerializeField, HideInInspector] public bool onEndFoldOut;
                [SerializeField, HideInInspector] public bool onCloseFoldOut;
                [SerializeField, HideInInspector] public bool onFailFoldOut;
                [SerializeField, HideInInspector] public bool onShootFoldOut;

                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Yoshi's Tongue", barColor, labelColor))
                        {
                                FoldOut.Box(4, FoldOut.boxColorLight, offsetY: -2);
                                parent.Field("Target Layer", "layer");
                                parent.DropDownList(inputList, "Button", "button");
                                parent.Field("Animation Timer", "animationTimer");
                                parent.FieldToggle("Stop Vel X", "stopVelX");
                                Layout.VerticalSpacing(3);

                                FoldOut.Box(2, FoldOut.boxColorLight);
                                parent.Field("Tongue Tip", "tongueTip");
                                parent.Field("Tongue", "tongue");
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(3, FoldOut.boxColorLight);
                                parent.Field("Tongue Speed", "speed");
                                parent.Field("Tongue Length", "tongueLength");
                                parent.Field("Closing Length", "closingLength");
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(2, FoldOut.boxColorLight);
                                parent.Field("Target Offset", "targetOffset");
                                parent.FieldToggle("Deactivate Target", "deactivateTarget");
                                Layout.VerticalSpacing(5);

                                Fields.Array(parent.Get("weapons"), "Add Weapon", "Weapon", FoldOut.boxColorLight);

                                Fields.EventFoldOut(parent.Get("onBegin"), parent.Get("onBeginFoldOut"), "On Begin", color: FoldOut.boxColorLight);
                                Fields.EventFoldOut(parent.Get("onEnd"), parent.Get("onEndFoldOut"), "On End", color: FoldOut.boxColorLight);
                                Fields.EventFoldOut(parent.Get("onClose"), parent.Get("onCloseFoldOut"), "On Close", color: FoldOut.boxColorLight);
                                Fields.EventFoldOut(parent.Get("onFail"), parent.Get("onFailFoldOut"), "On Fail", color: FoldOut.boxColorLight);
                                Fields.EventFoldOut(parent.Get("onShoot"), parent.Get("onShootFoldOut"), "On Shoot", color: FoldOut.boxColorLight);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
