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
        public class DashToWall : Action
        {
                [SerializeField] public float speed = 40f;
                [SerializeField] public bool canTakeDamage;
                [SerializeField] public bool changeHeight;
                [SerializeField] public float height = 1f;

                [System.NonSerialized] private float direction;
                [System.NonSerialized] private Health health;
                [System.NonSerialized] private Root root;

                private void Awake ()
                {
                        health = gameObject.GetComponent<Health>();
                }

                public override void OnReset (bool skip = false, bool enteredState = false)
                {
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
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                this.root = root;
                                direction = root.direction;
                                if (root.world.onWall)
                                {
                                        direction = root.world.rightWall ? -1f : 1f;
                                        root.world.rightWall = root.world.leftWall = false;
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

                        root.signals.Set("dashing");
                        root.velocity.x = direction * speed;
                        root.velocity.y = 0;

                        if (root.world.onWall)
                        {
                                root.world.box.ColliderReset();
                                if (!canTakeDamage && health != null)
                                {
                                        health.CanTakeDamage(true);
                                }
                                return NodeState.Success;
                        }
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(65, "Dash towards a wall. The AI can also change its height while dashing. Signals: dashing" +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(3, color, offsetY: -2);
                        {
                                parent.Field("Speed ", "speed");
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
