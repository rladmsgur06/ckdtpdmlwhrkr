#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.TwoBitSprite;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class RandomAnimation : Action
        {
                [SerializeField] public string targetAnimation;
                [SerializeField] public string randomAnimation;
                [SerializeField] public string signalAnimation;
                [SerializeField] public float time;
                [SerializeField] public int loop = 1;

                [System.NonSerialized] private bool found;
                [System.NonSerialized] private float counter;
                [System.NonSerialized] private int loopCounter;
                [System.NonSerialized] private SpriteEngine spriteEngine;

                private void Awake ()
                {
                        spriteEngine = gameObject.GetComponent<SpriteEngine>();
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (spriteEngine == null)
                        {
                                return NodeState.Failure;
                        }
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                counter = 0;
                                loopCounter = 0;
                                found = false;
                        }

                        if (!found)
                        {
                                if (spriteEngine.currentAnimation == targetAnimation)
                                {
                                        if (TwoBitMachines.Clock.Timer(ref counter, time))
                                        {
                                                found = true;
                                                loopCounter = 0;
                                                spriteEngine.player.looped = false;
                                                root.signals.Set(signalAnimation, true);
                                        }
                                }
                                else
                                {
                                        counter = 0;
                                }
                        }
                        else if (found)
                        {
                                if (spriteEngine.player.looped)
                                {
                                        spriteEngine.player.looped = false;
                                        if (++loopCounter >= loop)
                                        {
                                                counter = 0;
                                                found = false;
                                                return NodeState.Success;
                                        }
                                }
                                if (spriteEngine.currentAnimation != randomAnimation)
                                {
                                        spriteEngine.player.looped = false;
                                        counter = 0;
                                        found = false;
                                }
                                root.signals.Set(signalAnimation, found);
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
                                Labels.InfoBoxTop(70, "When the specified animation has been playing for a set amount of time, a random animation will be triggered to play." +
                                        "\n \nReturns Success, Failure");
                        }

                        FoldOut.Box(5, color, offsetY: -2);
                        parent.Field("Target Animation", "targetAnimation");
                        parent.Field("Random Animation", "randomAnimation");
                        parent.Field("Signal", "signalAnimation");
                        parent.Field("Time", "time");
                        parent.Field("Loop", "loop");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

}
