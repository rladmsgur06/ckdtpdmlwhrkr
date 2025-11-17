using System.Collections.Generic;
using TwoBitMachines.FlareEngine.Interactables;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class WorldCollision
        {
                [SerializeField] public bool climbSlopes;
                [SerializeField] public bool rotateToSlope;
                [SerializeField] public bool rectifyInAir;
                [SerializeField] public bool collideWorldOnly;
                [SerializeField] public bool horizontalCorners;
                [SerializeField] public bool useMovingPlatform;
                [SerializeField] public bool useMovingPlatformMomentum = true;
                [SerializeField] public bool rotateToWall;
                [SerializeField] public bool useBridges = true;
                [SerializeField] public bool jumpThroughEdge = true;
                [SerializeField] public float maxSlopeAngle = 45f;
                [SerializeField] public float rotateRate = 0.5f;
                [SerializeField] public string crushedWE;
                [SerializeField] public Edge2DSkip skipDownEdge;

                [SerializeField] public UnityEventEffect onCrushed;
                [SerializeField] public BoxInfo box = new BoxInfo();
                [SerializeField] public RotateToType rotateTo;

                [System.NonSerialized] public Transform transform;
                [System.NonSerialized] public LayerMask collisionLayer;
                [System.NonSerialized] public BoxCollider2D boxCollider;
                [System.NonSerialized] public Health health;
                [System.NonSerialized] public Vector2 oldPosition;
                [System.NonSerialized] public Vector2 oldVelocity;
                [System.NonSerialized] public int originLayer;

                [System.NonSerialized] public Vector2 groundNormal;
                [System.NonSerialized] public bool wasOnSlope;
                [System.NonSerialized] public bool wasOnSlopeUp;
                [System.NonSerialized] public bool climbingSlopeUp;
                [System.NonSerialized] public bool climbingSlopeDown;

                [System.NonSerialized] public Vector2 wallNormal;
                [System.NonSerialized] public Transform wallTransform;
                [System.NonSerialized] public Transform verticalTransform;
                [System.NonSerialized] public Collider2D wallCollider;
                [System.NonSerialized] public Collider2D verticalCollider;
                [System.NonSerialized] public bool leftWall;
                [System.NonSerialized] public bool rightWall;

                [System.NonSerialized] public bool pressedDown;
                [System.NonSerialized] public bool holdingDown;
                [System.NonSerialized] public bool holdingJump;
                [System.NonSerialized] public bool onSpikeY;
                [System.NonSerialized] public bool onSpikeX;
                [System.NonSerialized] public bool onBridge;
                [System.NonSerialized] public bool onGround;
                [System.NonSerialized] public bool onCeiling;
                [System.NonSerialized] public bool wasHiding;
                [System.NonSerialized] public bool wasOnPeak;
                [System.NonSerialized] public bool wasOnBridge;
                [System.NonSerialized] public bool wasOnGround;
                [System.NonSerialized] public bool wasOnCeiling;
                [System.NonSerialized] public bool isHiding;
                [System.NonSerialized] public bool isReversed;
                [System.NonSerialized] public bool hitInteractable;
                [System.NonSerialized] public bool missedAVertical;
                [System.NonSerialized] public bool missedAHorizontal;
                [System.NonSerialized] public bool interactableWasHit;
                [System.NonSerialized] public bool isHidingExternal;
                [System.NonSerialized] public bool healthInitialized;
                [System.NonSerialized] public bool hasJumped;
                [System.NonSerialized] public bool ignoreYDown;

                [System.NonSerialized] private Vector2 jumpPoint;
                [System.NonSerialized] public MovingPlatformInfo mp = new MovingPlatformInfo();
                [System.NonSerialized] public Dictionary<int, PlankState> bridge = new Dictionary<int, PlankState>();
                public static List<Collider2D> colliderResults = new List<Collider2D>();

                public Vector2 position => transform.position;
                public int wallDirection => leftWall ? -1 : 1;
                public bool onWall => leftWall || rightWall;
                public bool onWallStop => onWall || onSpikeX;
                public bool onJumpingSurface => onGround || onWall;
                public bool touchingASurface => onGround || onCeiling || onWall;
                private bool canTransition => climbingSlopeDown || onSpikeY || wasOnPeak || (!onGround && wasOnGround);
                public enum Edge2DSkip { OnDownHold, OnDownAndJump }

                public void Initialize (Transform transformRef)
                {
                        transform = transformRef;
                        originLayer = transform.gameObject.layer;
                        boxCollider = transformRef.gameObject.GetComponent<BoxCollider2D>();
                        collisionLayer = collideWorldOnly ? WorldManager.worldMask : WorldManager.collisionMask;
                        box.Initialize(this, boxCollider);
                        mp.Initialize(this);
                }

                public void Move (ref Vector2 velocity, ref float velY, bool hasJumped, float deltaTime, ref bool onSurface)
                {
                        if (transform == null || deltaTime == 0)
                                return;

                        HideLayer();

                        IgnoreBlockCollision();

                        JumpFromMovingPlatform(ref velocity, ref velY);

                        velocity *= deltaTime;

                        ResetCollisionInfo();

                        Bridges(hasJumped, ref velocity, ref onSurface);

                        FindSlopeToClimbDown(ref velocity);

                        HorizontalCollision(ref velocity, climbSlopes);

                        HorizontalSpikeCheck(ref velocity);

                        VerticalCollision(ref velocity, climbingSlopeUp);

                        VerticalSpikeCheck(ref velocity);

                        ApplyVelocity(velocity);

                        SlopeTransition(velocity);

                        RotateToSlope(velocity, deltaTime);

                        if (onGround)
                                onSurface = true; // if on ground, onSurface is true
                        if (onSurface)
                                onGround = true; // if on bridge (OnSurface), onGround is true
                        velocity = oldVelocity; //        box.velocity was the total velocity applied to the character

                        LatchToMovingPlatform();

                }

                public void MoveBasic (ref Vector2 velocity, float deltaTime)
                {
                        if (transform == null || deltaTime == 0)
                                return;

                        oldVelocity = Vector2.zero;
                        velocity *= deltaTime;
                        HorizontalCollision(ref velocity, false);
                        VerticalCollision(ref velocity, false);
                        ApplyVelocity(velocity);
                }

                #region Normal
                private void HorizontalCollision (ref Vector2 velocity, bool climbSlopes, bool stopSlopeMovement = true)
                {
                        if (velocity.x == 0)
                                return;

                        float signX = Mathf.Sign(velocity.x);
                        float magnitude = Mathf.Abs(velocity.x) + box.skin.x * 2f;
                        Vector2 corner = signX > 0 ? box.bottomRight - box.skinX : box.bottomLeft + box.skinX; // x ray starts from inside collider, but skin stops outside of collider to allow y ray to start on collider edge,  to use perfect corners

                        for (int i = 0; i < box.rays.x; i++)
                        {
                                Vector2 origin = corner + box.up * box.spacing.y * i;
                                RaycastHit2D hit = Physics2D.Raycast(origin, box.right * signX, magnitude, collisionLayer);

                                #region Debug
#if UNITY_EDITOR
                                if (WorldManager.viewDebugger)
                                {
                                        Debug.DrawRay(origin, box.right * signX * magnitude, Color.magenta);
                                }
#endif
                                #endregion

                                if (!hit)
                                {
                                        missedAHorizontal = true;
                                        continue;
                                }
                                if (i == 0 && climbSlopes && (FindSlopeToClimbUp(ref velocity, hit, out bool isJumping) || isJumping))
                                {
                                        if (isJumping)
                                        {
                                                continue; // need to ignore first x collision on jump or it will get stuck on first jump frame
                                        }
                                        return;
                                }
                                if (stopSlopeMovement && Vector2.Angle(hit.normal, box.up) <= 89.95f) // if on a sloped edge, stop x movement, since player can still move the corner into a slope
                                {
                                        velocity.x = 0;
                                        SetWall(hit, signX);
                                        return;
                                }

                                SetWall(hit, signX);
                                magnitude = Mathf.Max(hit.distance, box.skin.x * 2f);
                                velocity.x = (magnitude - box.skin.x * 2f) * signX;
                                magnitude += 0.0001f; //                                                  add 0.0001f for next ray, or it will think it missed a horizontal
                        }
                }

                private void VerticalCollision (ref Vector2 velocity, bool skip = false)
                {
                        if (skip || velocity.y == 0)
                                return;

                        float sign = Mathf.Sign(velocity.y);
                        float magnitude = Mathf.Abs(velocity.y) + box.skin.y;
                        float clampMag = magnitude;
                        Vector2 corner = sign > 0 ? box.topLeft : box.bottomLeft;
                        for (int i = 0; i < box.rays.y; i++)
                        {
                                Vector2 origin = corner + box.right * (box.spacing.x * i + velocity.x);
                                RaycastHit2D hit = Physics2D.Raycast(origin, box.up * sign, magnitude, collisionLayer);

                                #region Debug
#if UNITY_EDITOR
                                if (WorldManager.viewDebugger)
                                {
                                        Debug.DrawRay(origin, box.up * sign * magnitude, Color.yellow);
                                }
#endif
                                #endregion

                                if (!hit || (hit.collider is EdgeCollider2D && IgnoreEdge2D(hit.transform, sign)))
                                {
                                        missedAVertical = true;
                                        continue;
                                }
                                if (useMovingPlatform)
                                {
                                        if (mp.latched && hit.distance < box.skin.y) //         don't push out from inside a moving platform
                                        {
                                                hit.distance = box.skin.y;
                                        }
                                        else if (hit.distance == 0 && hit.transform.gameObject.layer == WorldManager.platformLayer)
                                        {
                                                if (velocity.y > 0)
                                                        continue; //                jump out from a moving platform if inside it
                                                hit.distance = box.skin.y;
                                        }
                                }
                                if (ignoreYDown && sign < 0 && hit.distance == 0)
                                {
                                        continue;
                                }

                                hit.distance = Mathf.Min(hit.distance, clampMag); //           fix missed a vertical issue, we only apply the smallest hit distance
                                clampMag = hit.distance;
                                magnitude = Mathf.Max(hit.distance, box.skin.y + 0.0001f); //  add 0.0001f for next ray, or it will think it missed a vertical
                                velocity.y = (hit.distance - box.skin.y) * sign;
                                onGround = sign <= 0 || onGround; //                            since slope climb Up can set onGround true, keep true
                                onCeiling = !onGround && sign > 0;
                                groundNormal = hit.normal;
                                verticalTransform = hit.transform;
                                verticalCollider = hit.collider;
                        }
                }

                private void ApplyVelocity (Vector2 velocity)
                {
                        transform.Translate(velocity);
                        oldVelocity += velocity;
                        box.UpdateCornersManually(velocity);
                }

                public bool IgnoreEdge2D (Transform edge, float sign)
                {
                        if (sign > 0 && (holdingJump || jumpThroughEdge))
                        {
                                return !edge.CompareTag("Edge2DDownOnly");
                        }
                        if (sign < 0 && ((skipDownEdge == Edge2DSkip.OnDownHold && holdingDown) || (skipDownEdge == Edge2DSkip.OnDownAndJump && pressedDown)))
                        {
                                return !edge.CompareTag("Edge2DUpOnly") && !edge.CompareTag("Beam");
                        }
                        return false;
                }

                public void SetWall (RaycastHit2D hit, float sign)
                {
                        wallCollider = hit.collider;
                        wallTransform = hit.transform;
                        wallNormal = hit.normal;
                        leftWall = sign < 0;
                        rightWall = sign > 0;
                }
                #endregion

                #region Slopes
                private bool FindSlopeToClimbUp (ref Vector2 velocity, RaycastHit2D slope, out bool isJumping)
                {
                        isJumping = false;
                        if (slope.distance <= 0 || !Compute.Between(Compute.Angle(slope.normal, box.up, out float slopeAngle), 0, maxSlopeAngle))
                        {
                                return false;
                        }
                        if (!ValidateSlope(ref velocity, velocity.x.Abs(), slopeAngle, slope, ref isJumping))
                        {
                                return false;
                        }
                        else
                        {
                                DiagonalClimbUp(ref velocity, box.BottomExactCorner(velocity.x));
                                return true;
                        }
                }

                private bool ValidateSlope (ref Vector2 velocity, float magnitude, float slopeAngle, RaycastHit2D slope, ref bool isJumping)
                {
                        Vector2 climbVelocity = Vector2.zero;
                        Vector2 corner = box.BottomExactCorner(velocity.x) + box.up * 0.001f;

                        for (int i = 0; i < 25; i++) // loop for organic slopes, most checks between 1-3
                        {
                                Vector2 slopeDirection = slope.normal.Rotate(90f * -Mathf.Sign(velocity.x));
                                Vector2 newVelocity = Compute.LineCircleIntersect(corner, magnitude, slope.point, slopeDirection) - corner;
                                climbVelocity = transform.InverseTransformVector(newVelocity);
                                RaycastHit2D hit = Physics2D.Raycast(corner, newVelocity.normalized, magnitude + 0.001f, collisionLayer);
                                //Debug.DrawRay (corner, newVelocity.normalized * (magnitude + 0.001f), Color.red);

                                if (hit && hit.distance > 0 && Compute.Angle(hit.normal, box.up, out float newAngle) > slopeAngle && newAngle <= maxSlopeAngle)
                                {
                                        slopeAngle = newAngle;
                                        slope = hit;
                                        continue;
                                }
                                break;
                        }
                        if (climbVelocity.y < velocity.y || climbVelocity.x == 0)
                        {
                                isJumping = climbVelocity.y < velocity.y;
                                return false;
                        }

                        verticalTransform = slope.transform;
                        groundNormal = slope.normal;
                        velocity = climbVelocity;
                        climbingSlopeUp = true;
                        onGround = true;
                        return true;
                }

                private void DiagonalClimbUp (ref Vector2 velocity, Vector2 corner)
                {
                        float sign = Mathf.Sign(velocity.x);
                        float magnitude = velocity.magnitude;
                        Vector2 normal = velocity / magnitude;
                        Vector2 direction = transform.InverseTransformVector(normal);
                        magnitude += box.skin.x;

                        for (int i = 1; i < box.rays.x; i++) // skip first since  already tested 
                        {
                                Vector2 origin = corner + box.up * (box.spacing.y * i + box.skin.y * 2f);
                                RaycastHit2D hit = Physics2D.Raycast(origin, direction, magnitude, collisionLayer);
                                #region Debug
#if UNITY_EDITOR
                                if (WorldManager.viewDebugger)
                                {
                                        Debug.DrawRay(origin, direction * magnitude, Color.black);
                                }
#endif
                                #endregion
                                if (!hit)
                                        missedAHorizontal = true;
                                if (!hit)
                                        continue;
                                magnitude = Mathf.Max(hit.distance, box.skin.x);
                                velocity = (magnitude - box.skin.x) * normal;
                                SetWall(hit, sign);
                        }
                }

                private void FindSlopeToClimbDown (ref Vector2 velocity)
                {
                        if (!climbSlopes || !wasOnGround || velocity.x == 0 || velocity.y >= 0)
                        {
                                return;
                        }

                        Vector2 corner = box.BottomCorner(-velocity.x);
                        float magnitude = Mathf.Abs(velocity.x) + box.skin.y;
                        RaycastHit2D slope = Physics2D.Raycast(corner, box.down, magnitude, collisionLayer);

                        if (slope && slope.distance > 0 && Compute.Between(Compute.Angle(box.up, slope.normal, out float slopeAngle), 0, maxSlopeAngle))
                        {
                                if (Compute.Dot(box.right * Mathf.Sign(velocity.x), slope.normal) > 0) //      only  climb if pointing in the same direction
                                {
                                        velocity.y -= Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * velocity.x;
                                        climbingSlopeDown = onGround = true;
                                        verticalTransform = slope.transform;
                                        groundNormal = slope.normal;
                                }
                        }
                }
                #endregion

                #region Transitions
                private void SlopeTransition (Vector2 velocity)
                {
                        if (!climbSlopes || velocity.x == 0 || onCeiling)
                        {
                                return;
                        }
                        if (climbingSlopeUp)
                        {
                                Transition(velocity.magnitude, Mathf.Sign(velocity.x));
                        }
                        else if (canTransition && velocity.y <= 0)
                        {
                                FindWall(box.BottomExactCorner(Mathf.Sign(velocity.x)), velocity.magnitude, Mathf.Sign(velocity.x));
                        }
                }

                private void FindWall (Vector2 origin, float magnitude, float signX)
                {
                        for (int i = 0; i < 3; i++)
                        {
                                Vector2 direction = (box.right * -signX).Rotate(10f * i * signX);
                                RaycastHit2D slope = Physics2D.Raycast(origin, direction, magnitude + 0.1f + 0.1f * i + box.sizeX, collisionLayer); //    Debug.DrawRay (origin, direction * (magnitude + 0.1f + 0.1f * i + box.sizeX), Color.black);

                                if (!slope)
                                {
                                        continue;
                                }
                                if (slope.distance > 0 && Vector2.Angle(slope.normal, box.up) <= maxSlopeAngle && slope.normal != groundNormal)
                                {
                                        Transition(magnitude, signX);
                                }
                                return;
                        }
                }

                private void Transition (float velocity, float signX)
                {
                        if (ModeDown(-velocity))
                        {
                                return;
                        }
                        if (ModeToSide(velocity * -signX))
                        {
                                onGround = true;
                        }
                }

                private bool ModeDown (float velocity)
                {
                        Vector2 velY = new Vector2(0, velocity);
                        VerticalCollision(ref velY);
                        VerticalSpikeCheck(ref velY, true);
                        ApplyVelocity(velY);
                        return velY.y != velocity; // if true, surface hit
                }

                private bool ModeToSide (float value)
                {
                        Vector2 velX = new Vector2(value, 0);
                        HorizontalCollision(ref velX, false, false);
                        ApplyVelocity(velX);
                        return velX.x != value; // if true, surface hit
                }
                #endregion

                #region Rotate
                private void RotateToSlope (Vector2 velocity, float deltaTime)
                {
                        if (!climbSlopes || !rotateToSlope)
                                return;

                        if (onGround && (missedAVertical || climbingSlopeUp || box.angle != 0))
                        {
                                Vector2 pivot, normal;
                                RaycastHit2D left = Physics2D.Raycast(box.BottomCorner(-1) + box.skinY, box.down, box.size.x + box.skin.y * 2f, collisionLayer);
                                RaycastHit2D right = Physics2D.Raycast(box.BottomCorner(1) + box.skinY, box.down, box.size.x + box.skin.y * 2f, collisionLayer);

                                if (rotateToWall && left.Valid() && !right.Valid()) // detect spikes
                                {
                                        right = Physics2D.Raycast(box.BottomCorner(1) - box.up * box.size.x * 0.5f, -box.right, box.size.x, collisionLayer);
                                }
                                if (rotateToWall && right.Valid() && !left.Valid())
                                {
                                        left = Physics2D.Raycast(box.BottomCorner(-1) - box.up * box.size.x * 0.5f, box.right, box.size.x, collisionLayer);
                                }
                                if (left.Valid() && right.Valid())
                                {
                                        pivot = left.distance <= right.distance ? left.point : right.point;
                                        normal = (right.point - left.point).normalized.Rotate(90f);
                                }
                                else
                                {
                                        pivot = left.Valid() ? left.point : right.Valid() ? right.point : Vector2.zero;
                                        normal = left.Valid() ? left.normal : right.Valid() ? right.normal : Vector2.zero;
                                }
                                if ((left.Valid() || right.Valid()) && Vector2.Angle(box.up, normal) > 0f)
                                {
                                        RotatePlayer(pivot, normal, deltaTime);
                                }

                        }
                        else if (!onGround && rectifyInAir && velocity.y > 0)
                        {
                                Vector2 rotate = rotateTo == RotateToType.RotateUp ? Vector2.up : Util.GetNearest90Normal(box.up);
                                if (wasOnGround)
                                {
                                        float sign = -box.up.CrossSign(rotate);
                                        jumpPoint = box.BottomExactCorner(sign) - (Vector2) transform.InverseTransformVector(velocity);
                                }
                                RotatePlayer(jumpPoint, rotate, deltaTime);
                        }
                }

                private void RotatePlayer (Vector2 pivot, Vector2 slopeNormal, float deltaTime)
                {
                        float angle = Vector2.Angle(box.up, slopeNormal);
                        float maxAngle = Mathf.Clamp(angle * deltaTime * rotateRate * 10f, 0, angle);
                        maxAngle = angle > 0 && angle < 1.5f ? angle : maxAngle;
                        transform.RotateAround(pivot, Vector3.forward, maxAngle * box.up.CrossSign(slopeNormal));

                        box.UpdateLeftCorner();
                        Vector2 size = new Vector2(box.size.x, box.sizeY * 0.8f);
                        Vector2 origin = box.center + box.up * box.sizeY * 0.10f;
                        if (Physics2D.OverlapBox(origin, size, box.angle, collisionLayer) != null) // don't rotate if wall hit
                        {
                                transform.RotateAround(pivot, Vector3.forward, -maxAngle * box.up.CrossSign(slopeNormal));
                        }
                }
                #endregion

                #region Spike Check
                private void HorizontalSpikeCheck (ref Vector2 velocity)
                {
                        if (horizontalCorners && velocity.x != 0)
                        {
                                float skin = box.size.x * 0.25f;
                                float sign = Mathf.Sign(velocity.x);
                                float magnitude = Mathf.Abs(velocity.x) + skin;
                                Vector2 direction = box.right * sign;
                                Vector2 size = new Vector2(box.size.x * 0.5f, box.size.y);
                                Vector2 origin = box.center + (climbingSlopeUp ? box.up * velocity.y : Vector2.zero);
                                RaycastHit2D hit = Physics2D.BoxCast(origin, size, box.angle, direction, magnitude, collisionLayer);

                                if (hit && hit.distance > 0 && !(hit.collider is EdgeCollider2D))
                                {
                                        float oldVelX = velocity.x;
                                        velocity.x = (hit.distance - skin) * sign;
                                        velocity.y *= climbingSlopeUp ? Mathf.Abs(velocity.x / oldVelX) : 1f;
                                        climbingSlopeUp = false;
                                        onSpikeX = true;
                                }
                        }
                }

                private void VerticalSpikeCheck (ref Vector2 velocity, bool force = false)
                {
                        if (force || (climbSlopes && velocity.y < 0 && (!onGround || Vector2.Angle(groundNormal, box.up) >= 0.01f)))
                        {
                                float skin = box.sizeY * 0.25f;
                                float magnitude = Mathf.Abs(velocity.y) + skin;
                                Vector2 size = new Vector2(box.size.x - 0.025f, box.sizeY * 0.5f); // -0.02 minus contact offset
                                Vector2 origin = box.center + box.right * velocity.x;
                                RaycastHit2D hit = Physics2D.BoxCast(origin, size, box.angle, box.down, magnitude, collisionLayer);
                                //  Draw.SquareCenter(origin + box.down * magnitude, size, Color.red);

                                if (hit && hit.distance > 0 && !(hit.collider is EdgeCollider2D))
                                {
                                        velocity.y = (hit.distance - skin) * -1f;
                                        onSpikeY = true;
                                        onGround = true;
                                }
                        }
                }
                #endregion

                #region Moving Platform
                private void IgnoreBlockCollision ()
                {
                        if (!mp.ignoreBlockCollision)
                                return;

                        if (mp.detected)
                        {
                                collisionLayer = WorldManager.worldMask;
                        }
                        else
                        {
                                mp.ignoreBlockCollision = false;
                                collisionLayer = collideWorldOnly ? WorldManager.worldMask : WorldManager.collisionMask;
                        }
                }

                public void JumpFromMovingPlatform (ref Vector2 velocity, ref float velY)
                {
                        if (useMovingPlatform && useMovingPlatformMomentum && onGround && velocity.y > 0)
                        {
                                if (mp.mp != null && mp.mp.canBoostLaunch)
                                {
                                        mp.LaunchBoost(ref velocity, ref velY);
                                }
                                else
                                {
                                        mp.Launch(ref velocity, ref velY);
                                }
                        }
                }

                public void LatchToMovingPlatform ()
                {
                        if (useMovingPlatform && onGround)
                        {
                                MovingPlatform.LatchToPlatform(this, verticalTransform, LatchMPType.Standing);
                        }
                }
                #endregion

                #region Bridges
                public void Bridges (bool hasJumped, ref Vector2 velocity, ref bool onSurface)
                {
                        if (useBridges)
                        {
                                Bridge.Find(this, box.center, hasJumped, ref velocity);
                                onSurface = (onBridge || onSurface) && !hasJumped;
                        }
                }
                #endregion

                #region Reset
                public void Reset ()
                {
                        ResetCollisionInfo();
                        mp.ClearLaunch();
                        box.EnableCollider();
                        bridge.Clear();

                        wasHiding = false;
                        wasOnPeak = false;
                        wasOnSlope = false;
                        wasOnBridge = false;
                        wasOnGround = false;
                        wasOnCeiling = false;
                        wasOnSlopeUp = false;
                        isHidingExternal = false;
                        interactableWasHit = false;
                        if (transform != null)
                                transform.eulerAngles = Vector3.zero;
                }

                public void ResetCollisionInfo ()
                {
                        mp.Clear();
                        groundNormal = Vector2.zero;
                        oldVelocity = Vector2.zero;
                        oldPosition = box.bottomCenter;

                        wasHiding = isHiding;
                        wasOnPeak = onSpikeY;
                        wasOnBridge = onBridge;
                        wasOnGround = onGround;
                        wasOnCeiling = onCeiling;
                        wasOnSlopeUp = climbingSlopeUp;
                        interactableWasHit = hitInteractable;
                        wasOnSlope = climbingSlopeUp || climbingSlopeDown;

                        wallTransform = null;
                        verticalTransform = null;
                        verticalCollider = null;

                        climbingSlopeDown = false;
                        missedAHorizontal = false;
                        hitInteractable = false;
                        climbingSlopeUp = false;
                        missedAVertical = false;
                        rightWall = false;
                        onCeiling = false;
                        onBridge = false;
                        isHiding = false;
                        onGround = false;
                        leftWall = false;
                        onSpikeY = false;
                        onSpikeX = false;
                }
                #endregion

                #region Hide Layer
                public void HideLayer ()
                {
                        if (isHiding && health == null && !healthInitialized)
                        {
                                health = transform.GetComponent<Health>();
                                healthInitialized = true;
                        }
                        if (healthInitialized)
                        {
                                if (health != null)
                                {
                                        health.HideLayer(isHiding);
                                }
                                else if (isHiding && transform.gameObject.layer != WorldManager.hideLayer)
                                {
                                        transform.gameObject.layer = WorldManager.hideLayer;
                                }
                                else if (!isHiding && transform.gameObject.layer != originLayer)
                                {
                                        transform.gameObject.layer = originLayer;
                                }
                        }
                }

                #endregion

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool foldOut;
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private bool crushedFoldOut;
#pragma warning restore 0414
#endif
                #endregion
        }
}
