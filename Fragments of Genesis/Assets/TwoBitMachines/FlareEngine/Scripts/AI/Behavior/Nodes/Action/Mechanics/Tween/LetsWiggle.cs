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
        public class LetsWiggle : Action
        {
                [SerializeField] public TwoBitMachines.LetsWiggle wiggle;

                private void Start ()
                {
                        if (wiggle != null)
                        {
                                wiggle.Deactivate();
                                wiggle.Remove();
                        }
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (wiggle == null)
                        {
                                return NodeState.Failure;
                        }
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                wiggle.Wiggle();
                        }
                        Vector3 position = this.transform.localPosition;
                        bool running = wiggle.Run();
                        if (root.type == CharacterType.MovingPlatform)
                        {
                                Vector2 velocity = this.transform.localPosition - position;
                                this.transform.localPosition = position;
                                if (Root.deltaTime != 0)
                                        root.velocity = velocity / Root.deltaTime;
                        }
                        return running ? NodeState.Running : NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(65, "Execute a Lets Wiggle tween. If this AI is a moving platform, then only its position should be modified." +
                                        "\n \nReturns Running, Success, Failure");
                        }
                        FoldOut.Box(1, color, offsetY: -2);
                        parent.Field("Lets Wiggle", "wiggle");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
