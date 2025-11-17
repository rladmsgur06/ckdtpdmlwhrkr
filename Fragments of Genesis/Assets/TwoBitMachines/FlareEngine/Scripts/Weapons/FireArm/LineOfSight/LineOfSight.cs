using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class LineOfSight
        {
                [SerializeField] public GameObject beam;
                [SerializeField] public GameObject beamEnd;
                [SerializeField] public LineOfSightType lineOfSight;
                [SerializeField] public UnityEventVector2 onTargetHit;
                [SerializeField] public UnityEventVector2 onBeamHit;
                [SerializeField] public UnityEvent onNothingHit;
                [SerializeField] public ReticleType reticleType;
                [SerializeField] public Transform reticle;
                [SerializeField] public LayerMask layer;
                [SerializeField] public LayerMask targetLayer;
                [SerializeField] public float reticleDistance = 10f;
                [SerializeField] public float maxLength = 100f;
                [SerializeField] public bool autoShoot = false;
                [SerializeField] public float autoShootRate = 0.1f;

                [System.NonSerialized] private bool changedDirection = false;
                [System.NonSerialized] private float previousAngle = 0;
                [System.NonSerialized] private Vector3 smoothDirection;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField] private bool eventsFoldOut = false;
                [SerializeField] private bool onBeamHitFoldOut = false;
                [SerializeField] private bool onNothingHitFoldOut = false;
                [SerializeField] private bool onTargetHitFoldOut = false;
                #pragma warning restore 0414
                #endif
                #endregion

                public void Reset ( )
                {
                        changedDirection = false;
                        previousAngle = 0;
                }

                public void Sight (Transform weapon, Transform firePoint, ref bool fire)
                {
                        if (lineOfSight == LineOfSightType.None)
                        {
                                return;
                        }
                        else if (lineOfSight == LineOfSightType.Beam) //* Make sure sprite is centered left 
                        {
                                if (beam != null)
                                {
                                        beam.transform.position = firePoint.position;
                                        ShootBeam (weapon, firePoint, ref fire);
                                }
                        }
                        else
                        {
                                RotateReticle (weapon, firePoint);
                        }

                }

                public void ShootBeam (Transform weapon, Transform firePoint, ref bool fire)
                {
                        Vector2 origin = firePoint.position;
                        float actualDistance = 0;
                        for (int i = 0; i < 25; i++) // will ignore up to twenty-five edge colliders (arbitrary number), most of the time it will only execute once
                        {
                                RaycastHit2D ray = Physics2D.Raycast (origin, weapon.right, maxLength, layer);
                                bool hitTarget = ray && ray.distance > 0;

                                if (hitTarget && ray.collider is EdgeCollider2D)
                                {
                                        origin = ray.point + (Vector2) weapon.right * 0.001f;
                                        actualDistance += ray.distance;
                                        continue;
                                }
                                if (!ray)
                                {

                                        beamEnd?.gameObject.SetActive (false);
                                        onNothingHit.Invoke ( );
                                        actualDistance = maxLength;
                                        break;
                                }
                                if (hitTarget)
                                {
                                        if (beamEnd != null)
                                        {
                                                beamEnd.gameObject.SetActive (true);
                                                beamEnd.gameObject.transform.position = ray.point;
                                        }
                                        if (((1 << ray.collider.gameObject.layer) & targetLayer) != 0)
                                        {
                                                onTargetHit.Invoke (ray.point);
                                                fire = fire || autoShoot;
                                        }
                                        actualDistance += ray.distance;
                                        onBeamHit.Invoke (ray.point);
                                        break;
                                }
                        }

                        Transform parent = beam.transform.parent;
                        Vector3 scale = beam.transform.localScale;
                        scale.x = parent != null ? actualDistance / parent.localScale.x : actualDistance; // keep the distance a 1: 1 incase parent is scaled
                        beam.transform.localScale = scale;
                }

                private void RotateReticle (Transform weapon, Transform firePoint)
                {
                        if (reticle == null) return;

                        if (reticleType == ReticleType.FixedPosition)
                        {
                                if (previousAngle != weapon.localEulerAngles.y) changedDirection = true;
                                Vector3 to = weapon.rotation * Vector2.right; //Compute.RotateVector (characterRight * weaponDirection, rotate);
                                smoothDirection = Compute.LerpConditional (smoothDirection, to, ref changedDirection).normalized;
                                reticle.position = firePoint.position + smoothDirection * reticleDistance;
                                reticle.rotation = Compute.Atan2Quaternion (smoothDirection);
                                previousAngle = weapon.localEulerAngles.y;
                        }
                        else
                        {
                                reticle.position = Util.MousePosition (reticle.position);
                                HideMouse ( );
                        }
                }

                private void HideMouse ( )
                {
                        Vector3 mp = Input.mousePosition;
                        if (!(mp.x >= 0f && mp.x <= Screen.width && mp.y >= 0f && mp.y <= Screen.height))
                        {
                                if (!Cursor.visible) Cursor.visible = true;
                                return;
                        }
                        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject ( ))
                        {
                                if (!Cursor.visible) Cursor.visible = true;
                                return;
                        }
                        if (Cursor.visible)
                        {
                                Cursor.visible = false;
                        }
                }
        }

        public enum LineOfSightType
        {
                None,
                Beam,
                Reticle
        }

        public enum ReticleType
        {
                FixedPosition,
                FollowMouse
        }

}