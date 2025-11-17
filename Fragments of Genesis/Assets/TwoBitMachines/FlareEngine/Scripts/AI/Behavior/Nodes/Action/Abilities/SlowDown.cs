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
        public class SlowDown : Action// slow down sprite engine also?
        {
                [SerializeField] public float intensity = 0.5f;
                [SerializeField] public float duration = 1f;

                [System.NonSerialized] public float counter;
                [System.NonSerialized] public bool isActive;
                [System.NonSerialized] public AIBase aiBase;

                private void Awake ()
                {
                        aiBase = GetComponent<AIBase>();
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                counter = 0;
                                isActive = false;
                                aiBase.slowDownActive = false;
                        }
                        if (isActive && aiBase != null)
                        {
                                aiBase.slowDownActive = true;
                                aiBase.timeScaleIntensity = intensity;

                                if (TwoBitMachines.Clock.Timer(ref counter, duration))
                                {
                                        isActive = false;
                                        aiBase.slowDownActive = false;
                                        Root.deltaTime = Time.deltaTime;
                                        return NodeState.Success;
                                }
                        }
                        return NodeState.Running;
                }

                public void Activate ()
                {
                        aiBase.slowDownActive = true;
                        aiBase.timeScaleIntensity = intensity;
                        isActive = true;
                        counter = 0;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Apply slow motion to this character only. Place node in an Alway State. Call Activate()." +
                                        "\n \nReturns Running Success");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        {
                                parent.Slider("Intensity", "intensity");
                                parent.Field("Duration", "duration");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
