#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class ThrowAndRetrieveWeapon : Action
        {
                [SerializeField] public Collider2D weapon;
                [SerializeField] public Transform firePoint;
                [SerializeField] public WeaponWallType weaponType;
                [SerializeField] public float stickOffset = 0.5f;
                [SerializeField] public float rotateSpeed = 100f;
                [SerializeField] public bool rotateWeapon;

                [SerializeField] public float throwSpeed = 25f;
                [SerializeField] public float throwTime = 1f;
                [SerializeField] public string throwSignal;

                [SerializeField] public string waitSignal;
                [SerializeField] public float waitHoldTime = 1f;

                [SerializeField] public float retrieveSpeed = 15f;
                [SerializeField] public string retrieveSignal;

                [SerializeField] public float holdWeaponTime = 1f;
                [SerializeField] public string holdWeaponSignal;

                [System.NonSerialized] private RetrieveWeapon state;
                [System.NonSerialized] private float weaponAngleOffset;
                [System.NonSerialized] private float timeOut;
                [System.NonSerialized] private float throwDirection;
                [System.NonSerialized] private float throwCounter;
                [System.NonSerialized] private float waitCounter;
                [System.NonSerialized] private float decelerate;
                [System.NonSerialized] private float holdWeaponCounter;
                [System.NonSerialized] private bool weaponThrown;
                [System.NonSerialized] private bool weaponBouncing;
                [System.NonSerialized] private bool isRetrieving;

                public Vector2 throwingDirection => (Vector2.right * throwDirection).normalized;
                public enum WeaponWallType { BounceOffWall, StickToWall }
                public enum RetrieveWeapon { Throw, Wait, Retrieve, HoldWeapon }

                public void Start ()
                {
                        if (weapon != null)
                                weaponAngleOffset = weapon.transform.localEulerAngles.z;
                }

                public void Reset ()
                {
                        weapon?.transform.SetParent(this.transform);
                        weapon?.gameObject.SetActive(false);
                        timeOut = Time.time;
                        isRetrieving = false;
                        weaponThrown = false;
                        weaponBouncing = false;
                        holdWeaponCounter = 0;
                        decelerate = 0;
                        waitCounter = 0;
                        throwCounter = 0;
                        state = 0;
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (weapon == null || firePoint == null)
                        {
                                return NodeState.Failure;
                        }
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                Reset();
                                throwDirection = root.direction;
                                Vector3 p = firePoint.localPosition;
                                firePoint.localPosition = new Vector3(root.direction < 0 ? -Mathf.Abs(p.x) : Mathf.Abs(p.x), p.y, p.z);
                        }
                        RunWeapon();
                        if (state == RetrieveWeapon.Throw)
                        {
                                if (root.world.onGround)
                                {
                                        root.signals.Set(throwSignal);
                                        if (TwoBitMachines.Clock.Timer(ref throwCounter, throwTime))
                                        {
                                                ThrowWeaponNow();
                                                state = RetrieveWeapon.Wait;
                                        }
                                }
                        }
                        if (state == RetrieveWeapon.Wait)
                        {
                                root.signals.Set(waitSignal);
                                if (TwoBitMachines.Clock.Timer(ref waitCounter, waitHoldTime))
                                {
                                        timeOut = Time.time;
                                        state = RetrieveWeapon.Retrieve;
                                }
                                CatchWeapon(root);
                        }
                        if (state == RetrieveWeapon.Retrieve)
                        {
                                isRetrieving = true;
                                root.signals.Set(retrieveSignal);
                                root.velocity.x = retrieveSpeed * throwDirection;
                                CatchWeapon(root);
                        }
                        if (state == RetrieveWeapon.HoldWeapon)
                        {
                                if (isRetrieving)
                                {
                                        decelerate = Mathf.Clamp01(decelerate + Root.deltaTime);
                                        float time = decelerate / 0.75f;
                                        root.velocity.x = Mathf.Lerp(retrieveSpeed * throwDirection, 0, time);
                                }
                                root.signals.Set(holdWeaponSignal);
                                if (TwoBitMachines.Clock.Timer(ref holdWeaponCounter, holdWeaponTime))
                                {
                                        return NodeState.Success;
                                }
                        }
                        return NodeState.Running;
                }

                private void ThrowWeaponNow ()
                {
                        weaponThrown = true;
                        weapon.gameObject.SetActive(true);
                        weapon.transform.SetParent(null);
                        weapon.transform.position = firePoint.position;
                        Vector2 throwDir = throwingDirection;
                        float angle = Mathf.Atan2(throwDir.y, throwDir.x) * Mathf.Rad2Deg;
                        weapon.transform.rotation = Quaternion.Euler(0f, 0f, angle + weaponAngleOffset);
                }

                private void CatchWeapon (Root root)
                {
                        bool caught = false;
                        Debug.DrawRay(root.world.box.center, Vector2.right * throwingDirection * root.world.box.sizeX * 3.5f, Color.red);
                        if (weapon.IsTouching(root.world.boxCollider) || root.world.onWall || Time.time > (timeOut + 15f))
                        {
                                caught = true;
                        }
                        if (weaponType == WeaponWallType.StickToWall && Physics2D.Raycast(root.world.box.center, Vector2.right * throwingDirection, root.world.box.sizeX * SlowDistance() * 0.5f, WorldManager.collisionMask))
                        {
                                caught = true;
                        }
                        if (throwDirection > 0 && weapon.transform.position.x < firePoint.position.x - 0.1f)
                        {
                                caught = true;
                        }
                        if (throwDirection < 0 && weapon.transform.position.x > firePoint.position.x + 0.1f)
                        {
                                caught = true;
                        }
                        if (caught)
                        {
                                weapon.gameObject.SetActive(false);
                                state = RetrieveWeapon.HoldWeapon;
                        }
                }

                private void RunWeapon ()
                {
                        if (weaponThrown)
                        {
                                if (Compute.OverlapCollider2D(weapon, WorldManager.collisionMask) > 0)
                                {
                                        weaponThrown = false;
                                        weaponBouncing = weaponType == WeaponWallType.BounceOffWall;
                                        weapon.transform.position += Vector3.right * throwDirection * stickOffset;
                                        if (weaponBouncing)
                                        {
                                                Vector2 throwDir = -throwingDirection;
                                                float angle = Mathf.Atan2(throwDir.y, throwDir.x) * Mathf.Rad2Deg;
                                                weapon.transform.rotation = Quaternion.Euler(0f, 0f, angle + weaponAngleOffset);
                                        }
                                }
                                else
                                {
                                        weapon.transform.position += Vector3.right * throwDirection * throwSpeed * Root.deltaTime;
                                }
                        }
                        else if (weaponBouncing)
                        {
                                weapon.transform.position += Vector3.right * -throwDirection * throwSpeed * Root.deltaTime * 0.9f;
                        }
                        if (rotateWeapon && (weaponThrown || weaponBouncing))
                        {
                                weapon.transform.Rotate(Vector3.forward, rotateSpeed * 10f * Root.deltaTime);
                        }

                }

                public float SlowDistance ()
                {
                        float stopTime = 0.75f;
                        float decelerate = retrieveSpeed / stopTime;
                        return retrieveSpeed * stopTime - 0.5f * decelerate * stopTime * stopTime;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(65, "Throw a weapon and retrieve it. Action sequence: Throw, Wait, Retrieve, Hold." +
                                        "\n \nReturns Success, Running");
                        }

                        int type = parent.Enum("weaponType");
                        FoldOut.Box(4, color, offsetY: -2);
                        {
                                parent.Field("Weapon Collider2D", "weapon");
                                parent.Field("Weapon Type", "weaponType", execute: type == 0);
                                parent.FieldDouble("Weapon Type", "weaponType", "stickOffset", execute: type == 1);
                                if (type == 1)
                                        Labels.FieldText("Offset");
                                parent.Field("Fire Point", "firePoint");
                                parent.FieldAndEnable("Rotate Weapon", "rotateSpeed", "rotateWeapon");
                        }
                        Layout.VerticalSpacing(3);

                        FoldOut.Box(5, color, -2);
                        {
                                parent.FieldDouble("Throw", "throwSpeed", "throwTime");
                                Labels.FieldDoubleText("Speed", "Time");

                                parent.Field("Throw Signal", "throwSignal");

                                parent.FieldDouble("Wait", "waitHoldTime", "waitSignal");
                                Labels.FieldDoubleText("Time", "Signal");

                                parent.FieldDouble("Retrieve", "retrieveSpeed", "retrieveSignal");
                                Labels.FieldDoubleText("Speed", "Signal");

                                parent.FieldDouble("Hold Weapon", "holdWeaponTime", "holdWeaponSignal");
                                Labels.FieldDoubleText("Time", "Signal");
                        }
                        Layout.VerticalSpacing(5);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
