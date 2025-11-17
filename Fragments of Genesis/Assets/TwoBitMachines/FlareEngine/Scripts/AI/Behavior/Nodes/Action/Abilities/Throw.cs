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
        public class Throw : Action
        {
                [SerializeField] public Vector2 velocity = new Vector2(5f, 5f);

                [System.NonSerialized] public Vector2 origin;
                [System.NonSerialized] public float velocityXReference;

                private void Awake ()
                {
                        origin = velocity;
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                root.velocity = new Vector2(velocityXReference = velocity.x * root.direction, velocity.y);
                                root.hasJumped = true;
                        }
                        else
                        {
                                if (root.world.onGround || root.world.onWall)
                                {
                                        return NodeState.Success;
                                }
                                root.velocity.x = velocityXReference;
                        }
                        return NodeState.Running;
                }

                public void SetForce (Vector2 boost)
                {
                        velocity.x = origin.x + Mathf.Abs(boost.x) * 0.5f;
                        velocity.y = origin.y + (boost.y > 0 ? boost.y * 0.5f : 0);
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Throw the AI." +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(1, color, offsetY: -2);
                        parent.Field("Velocity", "velocity");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
