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
        public class PlayAudio : Action
        {
                [SerializeField] public AudioManagerSO audioManagerSO;
                [SerializeField] public string audioName;

                public override NodeState RunNodeLogic (Root root)
                {
                        audioManagerSO?.PlayAudio(audioName);
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Play audio from the AudioMangerSO. " +
                                        "\n \nReturns Success");
                        }
                        FoldOut.Box(1, color, offsetY: -2);
                        {
                                parent.FieldDouble("Audio", "audioManagerSO", "audioName");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
