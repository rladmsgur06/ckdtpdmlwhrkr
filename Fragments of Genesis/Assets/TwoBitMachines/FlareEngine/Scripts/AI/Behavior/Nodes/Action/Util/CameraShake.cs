#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class CameraShake : Action
        {
                [SerializeField] public float strength = 0.25f;
                [SerializeField] public float duration = 1f;
                [SerializeField] public float speed = 1f;
                [SerializeField] public CamShakeType type;
                public enum CamShakeType { Random, Perlin, Sine }
                private Vector3 one = new Vector3(1f, 1f, 0);

                public override NodeState RunNodeLogic (Root root)
                {
                        Safire2DCamera.Safire2DCamera safire = Safire2DCamera.Safire2DCamera.mainCamera;
                        if (safire == null)
                        {
                                return NodeState.Failure;
                        }
                        if (type == CamShakeType.Random)
                        {
                                safire.ShakeRandom(duration, one, strength, speed);
                        }
                        else if (type == CamShakeType.Perlin)
                        {
                                safire.ShakePerlin(duration, one, strength, speed);
                        }
                        else
                        {
                                safire.ShakeSine(duration, one, strength, speed);
                        }
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Shake Safire2D Camera. Shake module must be enabled." +
                                        "\n \nReturns Success, Failure");
                        }
                        FoldOut.Box(3, color, offsetY: -2);
                        {
                                parent.FieldDouble("Strength", "type", "strength");
                                parent.Field("Speed", "speed");
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
