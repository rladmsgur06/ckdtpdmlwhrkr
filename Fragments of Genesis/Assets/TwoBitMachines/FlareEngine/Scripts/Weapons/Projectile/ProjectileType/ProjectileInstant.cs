using UnityEngine;
using TwoBitMachines.TwoBitSprite;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("")]
        public class ProjectileInstant : ProjectileBase
        {
                [SerializeField] public LayerMask layer;
                [SerializeField] public IgnoreEdge ignoreEdges;
                [SerializeField] public float maxLength = 50f;
                [SerializeField] public float hitRate;
                [SerializeField] public GameObject impactObject;
                [SerializeField] public OnTriggerRelease release;
                [SerializeField] public UnityEventEffect onImpact;
                [SerializeField] public string impactEffect = "";
                [SerializeField] public UnityEventEffect onFire;

                [System.NonSerialized] public float hitCounter;
                [System.NonSerialized] public float deactivateCounter;
                [System.NonSerialized] public bool deactivateActive;
                [System.NonSerialized] public float aliveCounter;
                [System.NonSerialized] public bool activated = false;
                [System.NonSerialized] public bool wasActivated = false;
                [System.NonSerialized] public SpriteEngine spriteEngine;
                [System.NonSerialized] public Transform currentFirePoint;
                [System.NonSerialized] public Quaternion currentRotation;

                public void Start ()
                {
                        ammunition.RestoreValue();
                        gameObject.SetActive(false);
                        spriteEngine = gameObject.GetComponent<SpriteEngine>();
                }

                public override void Activate (bool value)
                {
                        gameObject.SetActive(value);
                }

                public override void ResetAll ()
                {
                        hitCounter = 0;
                        deactivateCounter = 0f;
                        activated = false;
                        wasActivated = false;
                        deactivateActive = false;
                        gameObject.SetActive(false);
                }

                public override void Execute ()
                {
                        if (deactivateActive)
                        {
                                if (spriteEngine != null)
                                {
                                        spriteEngine.SetSignal("deactivateTimerOn");
                                }
                                if (Clock.Timer(ref deactivateCounter, 1f))
                                {
                                        gameObject.SetActive(false);
                                        DisableImpactObject();
                                }
                                else
                                {
                                        transform.position = currentFirePoint.position;
                                        HitScan(currentRotation * Vector3.right);
                                        onFire.Invoke(ImpactPacket.impact.Set("", transform.position, currentRotation * Vector3.forward));
                                }
                        }
                        if (!activated && !wasActivated)
                        {
                                return;
                        }

                        if (wasActivated && !activated && this.gameObject.activeInHierarchy && release != OnTriggerRelease.LeaveAsIs)
                        {
                                if (release == OnTriggerRelease.DeactivateGameObject)
                                {
                                        gameObject.SetActive(false);
                                        DisableImpactObject();
                                }
                                else
                                {
                                        deactivateActive = true;
                                        deactivateCounter = 0f;
                                }
                        }
                        wasActivated = activated;
                        activated = false;
                }

                public override bool FireProjectile (Transform transform, Quaternion rotation, Vector2 playerVelocity)
                {
                        if (!ammunition.Consume(1, inventory))
                        {
                                return false;
                        }

                        currentFirePoint = transform;
                        currentRotation = rotation;

                        activated = true;
                        deactivateCounter = 0f;
                        deactivateActive = false;
                        this.transform.position = transform.position;
                        this.transform.rotation = rotation;
                        HitScan(rotation * Vector3.right);
                        gameObject.SetActive(true);
                        onFire.Invoke(ImpactPacket.impact.Set("", transform.position, rotation * Vector3.forward));
                        return true;
                }

                private void EnableImpactObject (Vector3 position)
                {
                        if (impactObject == null)
                                return;
                        impactObject.transform.position = position;
                        impactObject.SetActive(true);
                }

                private void DisableImpactObject ()
                {
                        if (impactObject == null)
                                return;
                        impactObject.SetActive(true);
                }

                public void HitScan (Vector2 direction)
                {
                        float distance = maxLength;
                        if (ignoreEdges == IgnoreEdge.NeverIgnore)
                        {
                                SingleRay(direction, ref distance);
                        }
                        else
                        {
                                MultipleRays(direction, ref distance);
                        }

                        Vector3 scale = transform.localScale;
                        scale.x = distance;
                        transform.localScale = scale;
                }

                public void SingleRay (Vector2 direction, ref float distance)
                {
                        RaycastHit2D ray = Physics2D.Raycast(transform.position, direction, maxLength, layer);

                        if (ray)
                        {
                                if (ray.distance > 0)
                                {
                                        distance = ray.distance;
                                        #region Debug
#if UNITY_EDITOR
                                        if (WorldManager.viewDebugger)
                                        {
                                                Draw.GLCircleInit(ray.point, 0.25f, Color.red);
                                        }
#endif
                                        #endregion
                                        TargetFound(ray.transform, ray.point, direction);
                                }
                                else
                                {
                                        distance = 0; // origin point same as collider 
                                        DisableImpactObject();
                                }
                        }
                }

                public void MultipleRays (Vector2 direction, ref float distance)
                {
                        Vector2 origin = transform.position;
                        float actualDistance = 0;
                        for (int i = 0; i < 25; i++) // Will ignore up to twentyfive (arbitrary number), most of the time it will only execute once
                        {
                                RaycastHit2D ray = Physics2D.Raycast(origin, direction, maxLength, layer);
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
                                                distance = 0;
                                                DisableImpactObject();
                                                return;
                                        }
                                }
                                if (!ray)
                                {
                                        DisableImpactObject();
                                        return;
                                }
                                if (hitTarget)
                                {
                                        actualDistance += ray.distance;
                                        distance = actualDistance;
                                        TargetFound(ray.transform, ray.point, direction);
                                        return;
                                }
                        }
                }

                private void TargetFound (Transform transform, Vector2 impactPoint, Vector2 direction)
                {
                        if (Clock.Timer(ref hitCounter, hitRate))
                        {
                                if (Health.IncrementHealth(this.transform, transform, -damage, direction * damageForce))
                                {
                                        ImpactPacket impact = ImpactPacket.impact.Set(impactEffect, impactPoint, direction);
                                        onImpact.Invoke(impact);
                                }
                        }
                        EnableImpactObject(impactPoint);
                }
        }
}
