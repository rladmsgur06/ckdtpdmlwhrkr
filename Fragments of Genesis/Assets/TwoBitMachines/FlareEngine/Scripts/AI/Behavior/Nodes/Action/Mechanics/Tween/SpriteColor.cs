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
        public class SpriteColor : Action
        {
                [SerializeField] public SpriteRenderer sprite;
                [SerializeField] public Color from = Color.white;
                [SerializeField] public Color to = Color.white;
                [SerializeField] public float time = 1f;
                [SerializeField] public bool revertOnReset = true;

                [System.NonSerialized] private float counter;
                [System.NonSerialized] private Color origin;

                public void Awake ()
                {
                        if (sprite != null)
                                origin = sprite.color;
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (sprite == null)
                                return NodeState.Failure;

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                counter = 0;
                        }

                        if (TwoBitMachines.Clock.TimerInverse(ref counter, time))
                        {
                                sprite.color = Color.Lerp(from, to, counter / time);
                                return NodeState.Running;
                        }
                        sprite.color = to;
                        return NodeState.Success;
                }

                public override void OnReset (bool skip = false, bool enteredState = false)
                {
                        if (sprite != null && revertOnReset)
                        {
                                sprite.color = origin;
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Lerp the sprite's color." +
                                        "\n \nReturns Running, Success, Failure");
                        }
                        FoldOut.Box(3, color, offsetY: -2);
                        {
                                parent.FieldDouble("Sprite Renderer", "sprite", "time");
                                Labels.FieldText("Time", rightSpacing: 3);
                                parent.FieldDouble("From, To", "from", "to");
                                parent.Field("Revert On Reset", "revertOnReset");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
