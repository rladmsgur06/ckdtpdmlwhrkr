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
        public class WallBounce : Action
        {
                [SerializeField] public float time = 2f;
                [SerializeField] public float bounceForce = 1f;

                private float counter;
                private float speed;
                private bool isActive;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                isActive = false;
                                counter = 0;
                                speed = 0;
                        }
                        if (!isActive && root.world.onWall)
                        {
                                isActive = true;
                                float direction = root.world.leftWall ? 1 : -1;
                                speed = Mathf.Abs(root.velocity.x) * direction;
                                root.velocity.y = bounceForce;
                                root.hasJumped = true;
                        }
                        if (isActive)
                        {
                                root.velocity.x = Compute.Lerp(speed, 0f, time, ref counter, out bool complete);
                                if (complete) //TwoBitMachines.Clock.Timer (ref counter, time))
                                {
                                        isActive = false;
                                        return NodeState.Success;
                                }
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
                                Labels.InfoBoxTop(55, "The AI will bounce off the wall and continue moving for the specified time." +
                                        "\n \nReturns Running, Success");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        parent.Field("Bounce Force", "bounceForce");
                        parent.Field("Time", "time");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
