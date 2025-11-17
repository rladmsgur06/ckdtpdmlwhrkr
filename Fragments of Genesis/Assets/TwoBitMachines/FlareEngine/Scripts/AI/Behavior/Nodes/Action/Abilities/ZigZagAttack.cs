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
        public class ZigZagAttack : Action
        {
                [SerializeField] public int attacks = 20;
                [SerializeField] public float speed = 80f;
                [SerializeField] public float angle = 5f;

                [System.NonSerialized] public bool setup;
                [System.NonSerialized] public int attackIndex;
                [System.NonSerialized] public float refVelX;
                [System.NonSerialized] private Vector2 attackDirection;

                public void Reset ()
                {
                        setup = false;
                        attackIndex = 0;
                        refVelX = 0;
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                Reset();
                        }
                        if (!setup)
                        {
                                setup = true;
                                attackDirection = (Vector2.right * root.direction).Rotate(angle * root.direction);
                                root.velocity = attackDirection * speed;
                        }
                        else
                        {
                                if (root.world.onWall)
                                {
                                        attackDirection.x = -attackDirection.x;
                                        attackIndex++;
                                }
                                if (root.world.onGround || root.world.onCeiling)
                                {
                                        attackDirection.y = -attackDirection.y;
                                        attackIndex++;
                                }
                                if (attackIndex >= attacks)
                                {
                                        return NodeState.Success;
                                }
                                root.velocity = attackDirection * speed;
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
                                Labels.InfoBoxTop(65, "Attack in a zig zag pattern, bouncing off walls. This requires an enclosed room with four walls." +
                                        "\n \nReturns Running, Success");
                        }

                        FoldOut.Box(3, color, offsetY: -2);
                        {
                                parent.Field("Attacks", "attacks");
                                parent.Field("Speed", "speed");
                                parent.Field("Angle", "angle");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
