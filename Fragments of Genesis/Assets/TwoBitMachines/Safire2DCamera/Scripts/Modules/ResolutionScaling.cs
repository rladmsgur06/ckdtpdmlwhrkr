#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        [System.Serializable]
        public class ResolutionScaling
        {
                [SerializeField] public bool enable;
                [SerializeField] public ResolutionScalingMonobehaviour resolution;

                public void Execute (Camera camera)
                {
                        if (enable && resolution != null && Application.isPlaying)
                        {
                                resolution.cam = camera;
                                resolution.Execute();
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool close;
                [SerializeField, HideInInspector] public bool edit;
                [SerializeField, HideInInspector] public bool add;

                public static void CustomInspector (SerializedProperty parent, Color barColor, Color labelColor, Safire2DCamera main)
                {
                        if (!parent.Bool("edit"))
                                return;

                        main.resolutionScaling.Create(main);

                        if (Follow.Open(parent, "Resolution Scaling", barColor, labelColor))
                        {
                                SerializedProperty property = parent.Get("resolution");
                                if (property.objectReferenceValue == null)
                                {
                                        return;
                                }
                                SerializedObject newParent = new SerializedObject(property.objectReferenceValue);

                                newParent.Update();

                                int type = newParent.Enum("type");
                                bool ortho = main.cameraRef.orthographic;
                                FoldOut.Box(3, Tint.Box);
                                {
                                        newParent.Field("Match", "type", execute: type != 0);
                                        newParent.FieldDouble("Type", "type", "color", execute: type == 0);
                                        newParent.Field("Resolution", "resolution");
                                        newParent.Field("Target PPU", "targetPPU", execute: ortho);
                                        newParent.Field("Target FOV", "targetFOV", execute: !ortho);
                                }
                                Layout.VerticalSpacing(5);


                                newParent.ApplyModifiedProperties();

                        };
                }

                public void Create (Safire2DCamera main)
                {
                        if (Application.isPlaying)
                        {
                                return;
                        }
                        if (enable && resolution == null)
                        {
                                resolution = main.gameObject.AddComponent<ResolutionScalingMonobehaviour>();
                                resolution.resolutionRef = main.resolutionScaling;
                                resolution.cam = main.cameraRef;
                        }
                        if (!enable && resolution != null)
                        {
                                MonoBehaviour.DestroyImmediate(resolution);
                        }
                        if (enable && resolution != null && main.cameraRef != null)
                        {
                                resolution.cam = main.cameraRef;
                        }
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
