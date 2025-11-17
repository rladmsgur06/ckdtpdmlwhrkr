using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class JumpingObject : MonoBehaviour
        {
                [SerializeField] public float jumpForceXMin;
                [SerializeField] public float jumpForceXMax;
                [SerializeField] public float jumpForceYMin;
                [SerializeField] public float jumpForceYMax;
                [SerializeField] public Vector2 bounceFriction = new Vector2(0.3f, 0.3f);
                [SerializeField] public float gravity = 40f;
                [SerializeField] public float objectRadius = 0.5f;
                [SerializeField] public float timeOut = 0f;

                [SerializeField] public Transform target;
                [SerializeField] public bool flyToTarget;
                [SerializeField] public float waitTime = 2.5f;
                [SerializeField] public float flyTime = 0.5f;
                [SerializeField] public float curve = 5f;
                [SerializeField] public float flyToRadius = 10f;
                [SerializeField] public float randomRotation;
                [SerializeField] public bool useRadius;
                [SerializeField] public bool useRandomRotation;
                [SerializeField] public bool useDamageDirection;

                [System.NonSerialized] private CubicBezier path = new CubicBezier();
                [System.NonSerialized] private Vector2 tempForce;
                [System.NonSerialized] private Vector2 velocity;
                [System.NonSerialized] private Vector2 dissipate;
                [System.NonSerialized] private float counter = 0;
                [System.NonSerialized] private float counterX = 0;
                [System.NonSerialized] private float pathCounter = 0;
                [System.NonSerialized] private float waitCounter = 0;
                [System.NonSerialized] private float flyVariance;
                [System.NonSerialized] private float curveVariance;
                [System.NonSerialized] private float waitVariance;
                [System.NonSerialized] private bool insideRadius;
                [System.NonSerialized] private Vector2 startPoint;
                [System.NonSerialized] private SpriteRenderer sprite;
                [System.NonSerialized] private bool renderCheck;

                public void OnEnable ()
                {
                        counter = 0;
                        counterX = 0;
                        pathCounter = 0;
                        waitCounter = 0;
                        renderCheck = false;
                        insideRadius = false;
                        dissipate = Vector2.one;
                        gameObject.SetActive(true);

                        if (sprite != null)
                        {
                                sprite.enabled = true;
                        }
                        if (useRandomRotation)
                        {
                                gameObject.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, randomRotation));
                        }
                        if (useDamageDirection)
                        {
                                Vector2 forceDirection = ImpactPacket.impact.direction;
                                tempForce.x = Mathf.Sign(forceDirection.x) * Random.Range(jumpForceXMin, jumpForceXMax);
                                tempForce.y = Mathf.Sign(forceDirection.y) * Random.Range(jumpForceYMin, jumpForceYMax);
                        }
                        else
                        {
                                tempForce.x = Random.Range(jumpForceXMin, jumpForceXMax);
                                tempForce.y = Random.Range(jumpForceYMin, jumpForceYMax);
                        }

                        velocity.x = tempForce.x;
                        velocity.y = tempForce.y;
                        flyVariance = Random.Range(0, 0.2f);
                        waitVariance = waitTime == 0 ? 0 : Random.Range(0, 1f);
                        curveVariance = curve > 0 ? Random.Range(0, 1f) : 0;
                        startPoint = transform.position;
                }

                private void Update ()
                {
                        if (timeOut > 0 && Clock.Timer(ref counter, timeOut))
                        {
                                gameObject.SetActive(false);
                                return;
                        }
                        if (jumpForceXMin != 0 || jumpForceXMax != 0)
                        {
                                velocity.x = Compute.Lerp(tempForce.x, 0f, 2f, ref counterX);
                                Collide(ref velocity.x, ref dissipate.x, ref tempForce.x, bounceFriction.x, 0, Vector3.right);
                        }
                        if (jumpForceYMin != 0 || jumpForceYMax != 0)
                        {
                                velocity.y += -gravity * Time.deltaTime;
                                Collide(ref velocity.y, ref dissipate.y, ref tempForce.y, bounceFriction.y, velocity.x * Time.deltaTime, Vector3.up);
                        }

                        if (flyToTarget && target != null && flyTime > 0)
                        {
                                if (Clock.TimerExpired(ref waitCounter, waitTime + waitVariance))
                                {
                                        if (!useRadius || insideRadius || (target.position - transform.position).sqrMagnitude < flyToRadius * flyToRadius)
                                        {
                                                insideRadius = true;
                                                path.start = startPoint;
                                                path.end = target.position + Vector3.up;
                                                path.control1 = path.start + Vector2.up * (curve + curveVariance);
                                                path.control2 = path.end + Vector2.up * (curve + curveVariance);
                                                pathCounter = Mathf.Clamp(pathCounter + Time.deltaTime, 0, flyTime + flyVariance);
                                                float percent = pathCounter / (flyTime + flyVariance);
                                                transform.position = path.Follow(percent);

                                                if (percent >= 0.99f && !renderCheck)
                                                {
                                                        renderCheck = true;
                                                        if (sprite == null)
                                                        {
                                                                sprite = gameObject.GetComponent<SpriteRenderer>();
                                                        }
                                                        if (sprite != null)
                                                        {
                                                                sprite.enabled = false;
                                                        }
                                                }
                                                return;
                                        }
                                }
                                else
                                {
                                        startPoint = transform.position + (Vector3) velocity * Time.deltaTime;
                                }
                        }
                        transform.position += (Vector3) velocity * Time.deltaTime;
                }

                public void Collide (ref float velocity, ref float dissipate, ref float force, float friction, float offset, Vector2 direction)
                {
                        float sign = Mathf.Sign(velocity);
                        float magnitude = Mathf.Abs(velocity) * Time.deltaTime;
                        Vector3 offsetDirection = Vector3.right * offset;
                        RaycastHit2D ray = Physics2D.Raycast(transform.position + offsetDirection, direction * sign, objectRadius + magnitude, WorldManager.collisionMask);

                        if (ray)
                        {
                                dissipate = dissipate < 0.2f ? 0f : dissipate * (1f - friction);
                                velocity = force * -sign * dissipate;
                                force = Mathf.Abs(force) * -sign;
                        }
                }
        }
}
