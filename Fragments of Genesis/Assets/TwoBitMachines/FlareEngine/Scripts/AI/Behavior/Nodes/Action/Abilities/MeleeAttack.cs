#region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class MeleeAttack : Action
        {
                [SerializeField] public Collider2D colliderRef;
                [SerializeField] public LayerMask layer;
                [SerializeField] public string animationSignal;
                [SerializeField] public float damage = 1f;
                [SerializeField] public MeleeCollider enableCollider;
                [SerializeField] public Vector2 forceDirection = Vector2.right;
                [SerializeField] public Vector2 velocity;

                [System.NonSerialized] private ContactFilter2D filter = new ContactFilter2D();
                [System.NonSerialized] private List<Collider2D> list = new List<Collider2D>();
                [System.NonSerialized] private bool success = false;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (colliderRef == null)
                        {
                                return NodeState.Failure;
                        }
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                success = false;
                                filter.useLayerMask = true;
                                filter.useTriggers = true;
                                filter.layerMask = layer;
                                if (enableCollider == MeleeCollider.EnableOnStart)
                                {
                                        colliderRef.enabled = true;
                                }
                                if (velocity.y != 0)
                                {
                                        root.velocity.y = velocity.y;
                                        root.hasJumped = true;
                                }
                        }
                        if (velocity.x != 0)
                        {
                                root.velocity.x = velocity.x * Mathf.Sign(root.direction);
                        }
                        root.signals.Set("meleeCombo", true);
                        root.signals.Set(animationSignal, true);

                        int size = colliderRef.OverlapCollider(filter, list);
                        for (int i = 0; i < size; i++)
                        {
                                if (list[i].transform == this.transform)
                                        continue;
                                float direction = colliderRef.transform.position.x < list[i].transform.position.x ? 1f : -1f;
                                Vector2 newForceDirection = new Vector2(forceDirection.x * direction, forceDirection.y);
                                Health.IncrementHealth(transform, list[i].transform, -damage, newForceDirection);
                        }

                        FlipCollider(root.direction, colliderRef.transform);
                        return success ? NodeState.Success : NodeState.Running;
                }

                public void FlipCollider (float direction, Transform transform)
                {
                        transform.localPosition = Util.FlipXSign(transform.localPosition, direction); // change weapon position x depending on side
                        Vector3 r = transform.localEulerAngles;
                        transform.localRotation = Quaternion.Euler(r.x, direction < 0 ? 180f : 0f, r.z);
                }

                public void CompleteAttack ()
                {
                        success = true;
                        if (colliderRef != null)
                                colliderRef.enabled = false;
                }

                public override void OnReset (bool skip = false, bool enteredState = false)
                {
                        CompleteAttack();
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(120, "Perform a melee attack. If velocity is non zero, the y velocity will be treated as a jump force. CompleteAttack() must be called once the animation is complete, or the FSM will get stuck on this state. This method is available on the Melee Attack class. Signals: meleeCombo, customSignal" +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(7, color, offsetY: -2);
                        parent.Field("Collider2D", "colliderRef");
                        parent.Field("Collider Enable", "enableCollider");
                        parent.Field("Layer", "layer");
                        parent.Field("Animation Signal", "animationSignal");
                        parent.Field("Damage", "damage");
                        parent.Field("Force", "forceDirection");
                        parent.Field("Velocity", "velocity");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
