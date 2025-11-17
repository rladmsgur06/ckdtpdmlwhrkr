#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class Dash : Action
        {
                [SerializeField] public Blackboard target;
                [SerializeField] public DashType type;
                [SerializeField] public float speed = 40f;
                [SerializeField] public float time = 0.1f;
                [SerializeField] public bool canTakeDamage;
                [SerializeField] public bool changeHeight;
                [SerializeField] public float height = 1f;

                [System.NonSerialized] private bool isDashing;
                [System.NonSerialized] private float lerpTime;
                [System.NonSerialized] private float direction;
                [System.NonSerialized] private Health health;
                [System.NonSerialized] private Root root;

                public enum DashType
                {
                        ConstantVelocity,
                        SlowDown
                }

                private void Awake ()
                {
                        health = gameObject.GetComponent<Health>();
                }

                public override void OnReset (bool skip = false, bool enteredState = false)
                {
                        lerpTime = 0;
                        if (changeHeight && root != null && root.world.boxCollider.size.y != height) // CROUCH
                        {
                                root.world.box.ColliderReset();
                        }
                        if (!canTakeDamage && health != null)
                        {
                                health.CanTakeDamage(true);
                        }
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (target == null)
                        {
                                return NodeState.Failure;
                        }
                        this.root = root;
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                lerpTime = 0;
                                isDashing = true;
                                direction = Mathf.Sign(target.GetTarget().x - root.position.x);
                                if (this.target.hasNoTargets)
                                {
                                        return NodeState.Failure;
                                }
                                if (changeHeight)
                                {
                                        root.world.box.ChangeColliderHeight(height);
                                }
                                if (!canTakeDamage && health != null)
                                {
                                        health.CanTakeDamage(false);
                                }
                        }
                        DashNow(root);
                        return isDashing ? NodeState.Running : NodeState.Success;

                }

                private void DashNow (Root root)
                {
                        root.signals.Set("dashing");
                        float oldVelX = root.velocity.x;
                        float targetSpeed = type == DashType.ConstantVelocity ? direction * speed : 0;
                        root.velocity.x = Compute.Lerp(direction * speed, targetSpeed, time, ref lerpTime);

                        if (lerpTime >= time)
                        {
                                if (!changeHeight)
                                {
                                        isDashing = false;
                                }
                                else if (SafelyStandUp(root.world.box))
                                {
                                        isDashing = false;
                                        root.world.box.ColliderReset();
                                }
                                else
                                {
                                        root.signals.Set("crouch");
                                        root.velocity.x = oldVelX * 0.5f;
                                }
                                if (!isDashing && !canTakeDamage && health != null)
                                {
                                        health.CanTakeDamage(true);
                                }
                        }
                }

                public bool SafelyStandUp (BoxInfo ray)
                {
                        float length = Mathf.Abs(ray.boxSize.y - ray.collider.size.y) * ray.collider.transform.localScale.y;
                        for (int i = 0; i < ray.rays.y; i++)
                        {
                                Vector2 origin = ray.cornerTopLeft + ray.right * (ray.spacing.x * i);
                                #region Debug
#if UNITY_EDITOR
                                if (WorldManager.viewDebugger)
                                {
                                        Debug.DrawRay(origin, ray.up * length, Color.white);
                                }
#endif
                                #endregion
                                RaycastHit2D hit = Physics2D.Raycast(origin, ray.up, length, WorldManager.collisionMask);
                                if (hit && hit.distance == 0 && hit.transform.gameObject.layer == WorldManager.platformLayer)
                                {
                                        continue;
                                }
                                if (hit)
                                {
                                        return false;
                                }
                        }
                        return true;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(65, "Dash towards the target for the specified time. The AI can also change its height while dashing. Signals: dashing" +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(5, color, offsetY: -2);
                        {
                                AIBase.SetRef(ai.data, parent.Get("target"), 0);
                                parent.FieldDouble("Dash Type", "type", "speed");
                                parent.Field("Time", "time");
                                parent.FieldAndEnable("Change Height", "height", "changeHeight");
                                parent.FieldToggle("Can Take Damage", "canTakeDamage");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
