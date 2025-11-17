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
        public class ApplyGravity : Action
        {
                [SerializeField] public Vector2 direction;
                [SerializeField] public float force;
                [System.NonSerialized] private float gravity;
                [System.NonSerialized] private Vector2 previousVel;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                gravity = 0;
                                previousVel = root.velocity;
                        }

                        gravity += force;
                        if (root.type == CharacterType.NoCollisionChecks)
                        {
                                previousVel += direction * force * Root.deltaTime;
                                if (direction.x != 0)
                                        root.velocity.x = previousVel.x;
                                if (direction.y != 0)
                                        root.velocity.y = previousVel.y;
                        }
                        else
                        {
                                if (direction.x != 0)
                                        root.velocity.x = direction.x * gravity * Root.deltaTime;
                                if (direction.y != 0)
                                        root.velocity.y = direction.y * gravity * Root.deltaTime;
                        }
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "This will simulate gravity. Mostly needed by moving platforms." +
                                        "\n \nReturns Running");
                        }
                        FoldOut.Box(2, color, offsetY: -2);
                        {
                                parent.Field("Force", "force");
                                parent.Field("Direction", "direction");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
