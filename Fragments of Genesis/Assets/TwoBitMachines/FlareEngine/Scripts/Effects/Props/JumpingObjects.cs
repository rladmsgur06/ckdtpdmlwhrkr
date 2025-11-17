using System.Collections.Generic;
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class JumpingObjects : MonoBehaviour
        {
                [SerializeField] public float jumpForceXMin;
                [SerializeField] public float jumpForceXMax;
                [SerializeField] public float jumpForceYMin;
                [SerializeField] public float jumpForceYMax;
                [SerializeField] public Vector2 bounceFriction = new Vector2(0.3f, 0.3f);
                [SerializeField] public float gravity = 40f;
                [SerializeField] public float objectRadius = 0.5f;
                [SerializeField] public float timeOut = 0f;
                [SerializeField] public float fadeOut = 0f;

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

                [SerializeField] public int randomMin;
                [SerializeField] public int randomMax;
                [SerializeField] public bool isRandom;

                [SerializeField] public List<JumpingObjectItem> item = new List<JumpingObjectItem>();

                private void Awake ()
                {
                        for (int i = 0; i < this.transform.childCount; i++) // reclaim already existing objects if any
                        {
                                SpriteRenderer temp = this.transform.GetChild(i).GetComponent<SpriteRenderer>();
                                if (temp != null && i < item.Count)
                                {
                                        item[i].gameObject = temp.gameObject;
                                        item[i].renderer = temp;
                                }
                        }

                }
                private void OnEnable ()
                {
                        if (isRandom)
                        {
                                RandomizeList();
                                int random = Mathf.Clamp(Random.Range(randomMin, randomMax + 1), 0, item.Count);
                                for (int i = 0; i < random; i++)
                                {
                                        item[i].Reset(this);
                                }
                        }
                        else
                        {
                                for (int i = 0; i < item.Count; i++)
                                {
                                        item[i].Reset(this);
                                }
                        }
                }

                private void RandomizeList ()
                {
                        for (int i = 0; i < item.Count; i++)
                        {
                                JumpingObjectItem temp = item[i];
                                int randomIndex = Random.Range(i, item.Count);
                                item[i] = item[randomIndex];
                                item[randomIndex] = temp;
                        }
                }

                public void Update ()
                {
                        bool complete = true;
                        for (int i = 0; i < item.Count; i++)
                        {
                                item[i].Update(this, i);
                                if (item[i].active)
                                        complete = false;
                        }
                        if (complete)
                                this.gameObject.SetActive(false);
                }
        }

        [System.Serializable]
        public class JumpingObjectItem
        {
                [SerializeField] public Sprite sprite;
                [System.NonSerialized] private CubicBezier path = new CubicBezier();
                [System.NonSerialized] private Vector2 tempForce;
                [System.NonSerialized] private Vector2 velocity;
                [System.NonSerialized] private Vector2 dissipate;
                [System.NonSerialized] private float counter = 0;
                [System.NonSerialized] private float counterX = 0;
                [System.NonSerialized] private float pathCounter = 0;
                [System.NonSerialized] private float waitCounter = 0;
                [System.NonSerialized] private float fadeCounter = 0;
                [System.NonSerialized] private float fadeVariance;
                [System.NonSerialized] private float flyVariance;
                [System.NonSerialized] private float curveVariance;
                [System.NonSerialized] private bool insideRadius;
                [System.NonSerialized] private Vector2 startPoint;
                [System.NonSerialized] public GameObject gameObject;
                [System.NonSerialized] public SpriteRenderer renderer;
                [System.NonSerialized] private bool renderCheck;

                public bool active => gameObject != null && gameObject.activeInHierarchy;

                public void Reset (JumpingObjects jumpingRef)
                {
                        if (gameObject == null)
                        {
                                gameObject = new GameObject();
                                gameObject.transform.parent = jumpingRef.transform;
                        }
                        if (renderer == null)
                        {
                                renderer = gameObject.AddComponent<SpriteRenderer>();
                                renderer.sprite = sprite;
                        }
                        if (jumpingRef.useRandomRotation)
                        {
                                gameObject.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, jumpingRef.randomRotation));
                        }
                        if (renderer != null)
                        {
                                renderer.enabled = true;
                                Color spriteColor = renderer.color;
                                spriteColor.a = 1f;
                                renderer.color = spriteColor;
                        }
                        gameObject.SetActive(true);

                        gameObject.transform.position = jumpingRef.transform.position;
                        counter = 0;
                        counterX = 0;
                        pathCounter = 0;
                        waitCounter = 0;
                        fadeCounter = 0;
                        renderCheck = false;
                        insideRadius = false;
                        dissipate = Vector2.one;
                        startPoint = gameObject.transform.position;

                        if (jumpingRef.useDamageDirection)
                        {
                                Vector2 forceDirection = ImpactPacket.impact.direction;
                                tempForce.x = Mathf.Sign(forceDirection.x) * Random.Range(jumpingRef.jumpForceXMin, jumpingRef.jumpForceXMax);
                                tempForce.y = Mathf.Sign(forceDirection.y) * Random.Range(jumpingRef.jumpForceYMin, jumpingRef.jumpForceYMax);
                        }
                        else
                        {
                                tempForce.x = Random.Range(jumpingRef.jumpForceXMin, jumpingRef.jumpForceXMax);
                                tempForce.y = Random.Range(jumpingRef.jumpForceYMin, jumpingRef.jumpForceYMax);
                        }

                        velocity.x = tempForce.x;
                        velocity.y = tempForce.y;
                        flyVariance = Random.Range(0f, 0.5f);
                        fadeVariance = Random.Range(-jumpingRef.timeOut * 0.5f, 1f);
                        curveVariance = jumpingRef.curve > 0 ? Random.Range(0f, 1f) : 0;
                }

                public void Update (JumpingObjects jumpingRef, int index)
                {
                        if (!active)
                                return;

                        if (jumpingRef.timeOut > 0 && Clock.TimerExpired(ref counter, jumpingRef.timeOut + index * 0.5f + fadeVariance))
                        {
                                if (jumpingRef.fadeOut <= 0 || renderer == null)
                                {
                                        gameObject.SetActive(false);
                                        return;
                                }
                                else
                                {
                                        Color spriteColor = renderer.color;
                                        spriteColor.a = Mathf.Lerp(1f, 0, fadeCounter / jumpingRef.fadeOut);
                                        renderer.color = spriteColor;
                                        if (Clock.Timer(ref fadeCounter, jumpingRef.fadeOut))
                                        {
                                                gameObject.SetActive(false);
                                                return;
                                        }
                                }
                        }
                        if (jumpingRef.jumpForceXMin != 0 || jumpingRef.jumpForceXMax != 0)
                        {
                                velocity.x = Compute.Lerp(tempForce.x, 0f, 2f, ref counterX);
                                Collide(ref velocity.x, ref dissipate.x, ref tempForce.x, jumpingRef.bounceFriction.x, 0, jumpingRef.objectRadius, Vector3.right);
                        }
                        if (jumpingRef.jumpForceYMin != 0 || jumpingRef.jumpForceYMax != 0)
                        {
                                velocity.y += -jumpingRef.gravity * Time.deltaTime;
                                Collide(ref velocity.y, ref dissipate.y, ref tempForce.y, jumpingRef.bounceFriction.y, velocity.x * Time.deltaTime, jumpingRef.objectRadius, Vector3.up);
                        }

                        if (jumpingRef.flyToTarget && jumpingRef.target != null && jumpingRef.flyTime > 0)
                        {
                                if (Clock.TimerExpired(ref waitCounter, jumpingRef.waitTime))
                                {
                                        if (!jumpingRef.useRadius || insideRadius || (jumpingRef.target.position - gameObject.transform.position).sqrMagnitude < jumpingRef.flyToRadius * jumpingRef.flyToRadius)
                                        {
                                                insideRadius = true;
                                                path.start = startPoint;
                                                path.end = jumpingRef.target.position + Vector3.up;
                                                path.control1 = path.start + Vector2.up * (jumpingRef.curve + curveVariance);
                                                path.control2 = path.end + Vector2.up * (jumpingRef.curve + curveVariance);
                                                pathCounter = Mathf.Clamp(pathCounter + Time.deltaTime, 0, jumpingRef.flyTime + flyVariance);
                                                float percent = pathCounter / (jumpingRef.flyTime + flyVariance);
                                                gameObject.transform.position = path.Follow(percent);

                                                if (percent >= 0.99f && !renderCheck)
                                                {
                                                        renderCheck = true;
                                                        if (renderer != null)
                                                        {
                                                                renderer.enabled = false;
                                                        }
                                                }
                                                return;
                                        }
                                }
                                else
                                {
                                        startPoint = gameObject.transform.position + (Vector3) velocity * Time.deltaTime;
                                }

                        }
                        gameObject.transform.position += (Vector3) velocity * Time.deltaTime;
                }

                public void Collide (ref float velocity, ref float dissipate, ref float force, float friction, float offset, float objectRadius, Vector2 direction)
                {
                        float sign = Mathf.Sign(velocity);
                        float magnitude = Mathf.Abs(velocity) * Time.deltaTime;
                        Vector3 offsetDirection = Vector3.right * offset;
                        RaycastHit2D ray = Physics2D.Raycast(gameObject.transform.position + offsetDirection, direction * sign, objectRadius + magnitude, WorldManager.collisionMask);

                        if (ray)
                        {
                                dissipate = dissipate < 0.2f ? 0f : dissipate * (1f - friction);
                                velocity = force * -sign * dissipate;
                                force = Mathf.Abs(force) * -sign;
                        }
                }
        }

}
