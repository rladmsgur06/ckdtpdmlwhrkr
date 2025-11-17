using System.Collections.Generic;
using TwoBitMachines.TwoBitSprite;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("")]
        public class ProjectileShortRange : ProjectileBase
        {
                [SerializeField] public LayerMask layer;
                [SerializeField] public float hitRate;
                [SerializeField] public OnTriggerRelease release;
                [SerializeField] public BoxCollider2D boxCollider;
                [SerializeField] public UnityEventEffect onFire;

                [System.NonSerialized] public float hitCounter;
                [System.NonSerialized] public bool activated = false;
                [System.NonSerialized] public bool wasActivated = false;
                [System.NonSerialized] public float deactivateCounter;
                [System.NonSerialized] public bool deactivateActive;
                [System.NonSerialized] private List<Collider2D> contactResults = new List<Collider2D>();
                [System.NonSerialized] private ContactFilter2D contactFilter = new ContactFilter2D();
                [System.NonSerialized] public SpriteEngine spriteEngine;
                [System.NonSerialized] public Transform currentFirePoint;
                [System.NonSerialized] public Quaternion currentRotation;
                [System.NonSerialized] private FlameThrower flame;

                public void Start ()
                {
                        ammunition.RestoreValue();
                        contactFilter.useLayerMask = true;
                        contactFilter.layerMask = layer;
                        gameObject.SetActive(false);
                        flame = this.gameObject.GetComponent<FlameThrower>();
                        spriteEngine = gameObject.GetComponent<SpriteEngine>();
                }

                public override void Activate (bool value)
                {
                        gameObject.SetActive(value);
                }

                public override void ResetAll ()
                {
                        hitCounter = 0;
                        activated = false;
                        wasActivated = false;
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
                                        triggerReleased = true;
                                        if (flame == null)
                                                gameObject.SetActive(false);
                                }
                                else
                                {
                                        transform.position = currentFirePoint.position;
                                        AreaScan(currentRotation * Vector3.right);
                                        gameObject.SetActive(true);
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
                                        triggerReleased = true;
                                        if (flame == null)
                                                gameObject.SetActive(false);
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
                        base.playerVelocity = playerVelocity;
                        if (!ammunition.Consume(pattern.projectileRate, inventory))
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
                        AreaScan(rotation * Vector3.right);
                        gameObject.SetActive(true);
                        onFire.Invoke(ImpactPacket.impact.Set("", transform.position, rotation * Vector3.forward));
                        return true;
                }

                public void AreaScan (Vector2 direction)
                {
                        if (boxCollider != null && Clock.Timer(ref hitCounter, hitRate))
                        {
                                int hits = boxCollider.OverlapCollider(contactFilter, contactResults);
                                for (int i = 0; i < hits; i++)
                                {
                                        if (contactResults[i] != null)
                                        {
                                                Health.IncrementHealth(transform, contactResults[i].transform, -damage, direction * damageForce);
                                        }
                                }
                        }
                }

        }
}
