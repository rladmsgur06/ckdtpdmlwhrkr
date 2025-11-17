using TwoBitMachines.FlareEngine.ThePlayer;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("")]
        public class ProjectileGrapplingHook : ProjectileBase
        {
                [SerializeField] public LayerMask layer;
                [SerializeField] public IgnoreEdge ignoreEdges;
                [SerializeField] public GameObject hook;
                [SerializeField] public bool setToFirePoint;

                [SerializeField] public UnityEvent onMissed;
                [SerializeField] public UnityEvent offOnContact;
                [SerializeField] public UnityEvent onRetractComplete;
                [SerializeField] public UnityEvent onEmergencyExit;
                [SerializeField] public UnityEventEffect onHook;
                [SerializeField] public UnityEventEffect onItemGrabComplete;
                [SerializeField] public UnityEvent onItemGrabFound;
                [SerializeField] public UnityEvent onItemGrabFailed;
                [SerializeField] public UnityEvent onEnable;
                [SerializeField] public string hookWE = "";
                [SerializeField] public string grabCompleteWE = "";

                [SerializeField] public float swingForce = 0.01f;
                [SerializeField] public float maxLength = 25f;
                [SerializeField] public float gravity = 0.05f;
                [SerializeField] public float jumpAway = 15f;
                [SerializeField] public float hookOffset = 0;
                [SerializeField] public bool disableOnContact;

                [SerializeField] public bool retract = true;
                [SerializeField] public float rate = 5f;
                [SerializeField] public float minLength = 4f;
                [SerializeField] public float retractDelay = 0;
                [SerializeField] public RetractType retractType;
                [SerializeField] public InputButtonSO retractButton;
                [SerializeField] public bool disableOnRetractComplete;
                [SerializeField] public Vector2 retractFriction = new Vector2(0.2f, 0.55f);

                [SerializeField] public LineRenderer lineRenderer;
                [SerializeField] public AnimationCurve shootCurve;
                [SerializeField] public int pointsInLine = 30;
                [SerializeField] public float amplitude = 1.5f;
                [SerializeField] public float speed = 1f;

                [SerializeField] public bool canGrabItem;
                [SerializeField] public string itemTag;
                [SerializeField] public string itemTagID;
                [SerializeField] public float grabRetractSpeed = 25f;
                [SerializeField] public float releaseDistance = 4f;

                [System.NonSerialized] private Particle handle = new Particle();
                [System.NonSerialized] private Particle anchor = new Particle();
                [System.NonSerialized] private Stick stick = new Stick();
                [System.NonSerialized] private Transform itemTransform;
                [System.NonSerialized] private float retreatTime;
                [System.NonSerialized] private float extendTime;
                [System.NonSerialized] private bool retractComplete;
                [System.NonSerialized] private bool anchorSet;
                [System.NonSerialized] private bool retreat;
                [System.NonSerialized] private bool grabbingItem;
                [System.NonSerialized] private int inZone;
                [System.NonSerialized] private float delayCounter;
                [System.NonSerialized] private float oldHandleVel;
                [System.NonSerialized] private float grabWait;
                [System.NonSerialized] private bool itemReleased;
                [System.NonSerialized] private Vector2 hookPosition;
                [System.NonSerialized] private Vector2 retreatOffset;
                [System.NonSerialized] private GrabItemState grabItemState;

                [System.NonSerialized] private Vector2 previousHandlePosition;
                [System.NonSerialized] private Vector2 currentHandlePosition;
                [System.NonSerialized] private float timeAccumulator = 0f;

                public enum GrabItemState { Extending, Wait, Retract }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] public bool hookFoldOut;
                [SerializeField] public bool onEnableFoldOut;
                [SerializeField] public bool missedFoldOut;
                [SerializeField] public bool retractFoldOut;
                [SerializeField] public bool emergencyFoldOut;
                [SerializeField] public bool offOnContactFoldOut;
                [SerializeField] public bool retractBarFoldOut;
                [SerializeField] public bool grabItemFoldOut;
                [SerializeField] public bool grabItemCompleteFoldOut;
                [SerializeField] public bool grabItemFoundFoldOut;
                [SerializeField] public bool grabItemFailedFoldOut;
                [SerializeField] public bool grabMainFoldOut;
                [SerializeField] public bool retractMainFoldOut;
#pragma warning restore 0414
#endif
                #endregion

                #region Reset, Exit

                public void Start ()
                {
                        ammunition.RestoreValue();
                }

                public override void OnEnableNow ()
                {
                        onEnable.Invoke();
                }

                public override void OnDisableNow ()
                {
                        anchorSet = false;
                        retreat = false;
                        grabbingItem = false;
                        lineRenderer.enabled = false;
                        grabItemState = GrabItemState.Extending;
                        if (hook != null)
                                hook.SetActive(false);
                }

                public override void ResetAll ()
                {
                        OnDisableNow();
                        gameObject.SetActive(false);
                }

                private void CheckForExit (AbilityManager player, ref Vector2 velocity)
                {
                        if (player.world.onWall || player.world.onCeiling)
                        {
                                handle.SetPosition(transform.position);
                                if (disableOnContact)
                                {
                                        anchorSet = false;
                                        retreat = true;
                                        offOnContact.Invoke();
                                }
                        }
                        if (retractComplete)
                        {
                                anchorSet = false;
                                retreat = true;
                        }
                        if (player.world.onGround && player.world.box.center.y >= anchor.position.y)
                        {
                                anchorSet = false;
                                retreat = true;
                        }
                        if (player.jumpButtonPressed)
                        {
                                anchorSet = false;
                                retreat = true;
                                velocity = new Vector2(velocity.x, jumpAway);
                                player.CheckForAirJumps();
                        }
                        if (retreat && !gameObject.activeInHierarchy)
                        {
                                OnDisableNow();
                        }
                }

                public override void TurnOff (Firearm firearm, AbilityManager player)
                {
                        EmergencyExit(); // ability, firearm, has been turned off
                }

                public void EmergencyExit ()
                {
                        if (anchorSet)
                        {
                                anchorSet = false;
                                retreat = true;
                                onEmergencyExit.Invoke();
                        }
                }
                #endregion

                #region Shoot, Set
                public override bool FireProjectile (Transform transform, Quaternion rotation, Vector2 playerVelocity)
                {
                        if (anchorSet || !ammunition.Consume(1, inventory))
                        {
                                return false;
                        }
                        if (ignoreEdges == IgnoreEdge.NeverIgnore)
                        {
                                SingleRay(rotation * Vector3.right);
                        }
                        else
                        {
                                MultipleRays(rotation * Vector3.right);
                        }
                        if (canGrabItem && anchorSet && itemTransform != null)
                        {
                                FlareTag tag = itemTransform.GetComponent<FlareTag>();
                                Vector2 distance = itemTransform.position - transform.position; // don't use z position
                                if ((itemTag != "" && itemTransform.gameObject.CompareTag(itemTag)) || (tag != null && tag.Contains(itemTagID)))
                                {
                                        if (distance.magnitude > releaseDistance)
                                        {
                                                onItemGrabFound.Invoke();
                                                return grabbingItem = true;
                                        }
                                        else
                                        {
                                                OnDisableNow();
                                                onItemGrabFailed.Invoke();
                                                return false;
                                        }
                                }
                        }
                        return true;
                }

                public void SingleRay (Vector2 direction)
                {
                        RaycastHit2D ray = Physics2D.Raycast(transform.position, direction, maxLength, layer);
                        if (ray && ray.distance > 0)
                        {
                                SetActive(ray, ray.distance);
                        }
                        else
                        {
                                onMissed.Invoke();
                        }
                }

                public void MultipleRays (Vector2 direction)
                {
                        Vector2 origin = transform.position;
                        float actualDistance = 0;
                        for (int i = 0; i < 25; i++) // Will ignore up to twentyfive (arbitrary number), most of the time it will only execute once
                        {
                                RaycastHit2D ray = Physics2D.Raycast(origin, direction, maxLength, layer);
                                // Debug.DrawRay (origin, direction * maxLength, Color.red, 3f);
                                bool hitTarget = ray && ray.distance > 0;
                                if (ray)
                                {
                                        if (ray.collider is EdgeCollider2D && (ignoreEdges == IgnoreEdge.IgnoreAlways || direction.y > 0))
                                        {
                                                origin = ray.point + direction * 0.001f;
                                                actualDistance += ray.distance > 0 ? ray.distance : 0.001f;
                                                continue;
                                        }
                                        else if (ray.distance <= 0)
                                        {
                                                onMissed.Invoke();
                                                return;
                                        }
                                }
                                if (!ray)
                                {
                                        onMissed.Invoke();
                                        return;
                                }
                                if (hitTarget)
                                {
                                        actualDistance += ray.distance;
                                        SetActive(ray, actualDistance);
                                        return;
                                }
                        }
                }

                private void SetActive (RaycastHit2D hit, float distance)
                {
                        anchorSet = true;
                        retreat = false;
                        grabbingItem = false;
                        itemReleased = false;
                        retractComplete = false;
                        grabWait = 0;
                        extendTime = 0;
                        delayCounter = 0;
                        timeAccumulator = 0f;

                        stick.SetLength(distance);
                        anchor.Set(hit.point, 0, true);
                        handle.Set(transform.position, -gravity, false);
                        oldHandleVel = inZone = 0;
                        itemTransform = hit.transform;
                        grabItemState = GrabItemState.Extending;
                        onHook.Invoke(ImpactPacket.impact.Set(hookWE, hit.point, hit.normal));

                        previousHandlePosition = currentHandlePosition = handle.position;

                        lineRenderer.positionCount = pointsInLine;
                        lineRenderer.enabled = true;
                        for (int i = 0; i < pointsInLine; i++)
                        {
                                lineRenderer.SetPosition(i, hit.transform.position);
                        }
                }
                #endregion

                public void FixedUpdate ()//RopePhysics () //
                {
                        if (!anchorSet)
                                return;

                        // Store previous position before updating
                        previousHandlePosition = currentHandlePosition;
                        float hasFriction = retract && stick.length > minLength ? 1f : 0f;
                        handle.FixedUpdate(1f - retractFriction.x * hasFriction, 1f - retractFriction.y * hasFriction);

                        for (int i = 0; i < 10; i++)
                        {
                                stick.FixedUpdate(anchor, handle);
                        }
                        currentHandlePosition = handle.position;
                        timeAccumulator = 0;
                }

                public override void LateExecute (Firearm firearm, AbilityManager player, ref Vector2 velocity)
                {
                        ////
                        if (setToFirePoint && firearm.firePoint != null)
                        {
                                transform.position = firearm.firePoint.position;
                        }
                        if (grabbingItem)
                        {
                                GrabbingItem(firearm);
                                return;
                        }
                        if (retreat)
                        {
                                handle.SetPosition(transform.position);
                                ExtendRope(speed, -1f, 1.5f, 0.4f);
                        }
                        if (anchorSet)
                        {
                                timeAccumulator += Time.deltaTime / Time.fixedDeltaTime;
                                timeAccumulator = Mathf.Clamp(timeAccumulator, 0, 1f);

                                Retract(player);
                                Swing(player, ref velocity); // this moves the player to the grappling handle position
                                ExtendRope(speed);
                                CheckForExit(player, ref velocity);
                                Signals(player);
                                if (anchorSet && !gameObject.activeInHierarchy) // enable here incase hook is being shot while player is on ground
                                {
                                        gameObject.SetActive(true);
                                }
                        }
                        else
                        {
                                SetHookPosition(firearm.firePoint.position, firearm.transform.right);
                        }
                }

                private void Swing (AbilityManager player, ref Vector2 velocity)
                {
                        Vector2 handlePosition = Vector2.Lerp(previousHandlePosition, currentHandlePosition, timeAccumulator); //Vector2.Lerp(previousHandlePos, currentHandlePos, lerpFactor);
                        handle.ApplyAcceleration(Vector2.right * player.inputX * swingForce); //     swing
                        //velocity = (handle.position - (Vector2) transform.position) / Time.deltaTime;
                        velocity = (handlePosition - (Vector2) transform.position) / Time.deltaTime;
                }

                private void ExtendRope (float speed, float sign = 1f, float speedBoost = 1f, float scale = 1f)
                {
                        Vector2 handlePosition = sign < 0 ? handle.position : Vector2.Lerp(previousHandlePosition, currentHandlePosition, timeAccumulator);

                        Vector2 direction = anchor.position - handlePosition;
                        Vector2 normal = direction.normalized;
                        Vector2 waveDirection = normal.Rotate(90f);
                        extendTime = Mathf.Clamp01(extendTime + Time.deltaTime * speed * speedBoost * sign);
                        retreatTime = Mathf.Clamp01(extendTime >= 1f ? retreatTime + Time.deltaTime * speed : 0);

                        for (int i = 0; i < pointsInLine; i++)
                        {
                                float percentPosition = (float) i / (pointsInLine - 1); // Calculate the lerp amount for each point based on its position in the line
                                Vector2 oscillate = waveDirection * shootCurve.Evaluate(percentPosition) * amplitude * scale * (1f - retreatTime);
                                Vector2 pointPosition = hookPosition = Vector2.Lerp(handlePosition, anchor.position, percentPosition * extendTime) + oscillate;
                                lineRenderer.SetPosition(i, pointPosition);
                        }

                        SetHookPosition(hookPosition - normal * hookOffset, normal);

                        if (sign < 0 && extendTime <= 0)
                        {
                                ResetAll();
                        }
                }

                private void SetHookPosition (Vector2 position, Vector2 direction)
                {
                        if (hook != null && gameObject.activeInHierarchy)
                        {
                                hook.SetActive(true);
                                hook.transform.position = position;
                                float radians = Mathf.Atan2(direction.y, direction.x);
                                float angle = (radians * Mathf.Rad2Deg + 360f) % 360f;
                                hook.transform.eulerAngles = new Vector3(0f, 0f, angle);
                        }
                }

                private void Retract (AbilityManager player)
                {
                        if (retract && (retractType == RetractType.Automatic || (retractButton != null && retractButton.Holding())) && Clock.TimerExpired(ref delayCounter, retractDelay))
                        {
                                float rateBoost = player.world.box.center.y >= anchor.position.y ? 1.5f : 1f;
                                float realMagnitude = (anchor.position - (Vector2) transform.position).magnitude;
                                float realLength = realMagnitude < minLength ? realMagnitude : minLength;
                                stick.length = Mathf.MoveTowards(stick.length, realLength, Time.deltaTime * rate * rateBoost);
                                if (stick.length <= realLength)
                                {
                                        retractComplete = disableOnRetractComplete;
                                        onRetractComplete.Invoke();
                                }
                        }
                }

                private void Signals (AbilityManager player)
                {
                        if (anchorSet && (!retract || delayCounter >= retractDelay))
                        {
                                player.signals.Set("grapplingHook", true);
                                player.signals.Set("retractingHook", retract && stick.length > minLength);
                                player.signals.Set("swingingHook", (player.inputX * swingForce) != 0);

                                float limit = 1.25f;
                                float handleVel = handle.velocity.x;
                                if (Mathf.Abs(handle.x - anchor.x) > limit)
                                {
                                        inZone = 0;
                                }
                                if (Mathf.Sign(handleVel) != Mathf.Sign(oldHandleVel))
                                {
                                        inZone++;
                                }
                                oldHandleVel = handleVel;

                                if (Mathf.Abs(handle.x - anchor.x) < limit && inZone >= 3) //Mathf.Abs (previousLeft - previousRight) < limit)
                                {
                                        player.signals.ForceDirection(1);
                                        player.signals.SetDirection(1);
                                }
                                if ((player.inputX * swingForce) != 0)
                                {
                                        player.signals.ForceDirection((int) player.inputX);
                                        player.signals.SetDirection((int) player.inputX);
                                }
                        }
                }

                public void GrabbingItem (Firearm firearm)
                {
                        if (grabItemState == GrabItemState.Extending)
                        {
                                ExtendRope(speed);
                                retreatOffset = (Vector2) itemTransform.position - hookPosition;
                                grabItemState = extendTime >= 1 && retreatTime >= 1 ? GrabItemState.Retract : grabItemState;
                        }
                        if (grabItemState == GrabItemState.Wait)
                        {
                                if (Clock.Timer(ref grabWait, 0.15f))
                                {
                                        grabItemState = GrabItemState.Retract;
                                }
                        }
                        if (grabItemState == GrabItemState.Retract)
                        {
                                handle.SetPosition(transform.position);
                                ExtendRope(grabRetractSpeed, -1f, 1f, 0.5f);
                                float distance = (transform.position - itemTransform.position).magnitude;
                                if (itemTransform != null && distance > releaseDistance)
                                {
                                        itemTransform.position = hookPosition + retreatOffset;
                                }
                                if (!itemReleased && (distance <= releaseDistance || extendTime <= 0))
                                {
                                        itemReleased = true;
                                        int directionX = (int) Mathf.Sign(firearm.transform.right.x);
                                        ImpactPacket packet = ImpactPacket.impact.Set(grabCompleteWE, firearm.transform, null, firearm.firePoint.position, firearm.transform, -firearm.transform.right, directionX, 0);
                                        onItemGrabComplete.Invoke(packet);
                                        if (itemTransform != null)
                                        {
                                                Impact impact = itemTransform.GetComponent<Impact>();
                                                impact?.Activate(packet);
                                        }
                                }

                        }
                }

        }

        public enum RetractType
        {
                Automatic,
                Button
        }
}
