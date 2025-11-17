using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.Interactables;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Water))]
        public class WaterEditor : UnityEditor.Editor
        {
                private Water main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as Water;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();
                        int type = parent.Enum("type");
                        Water(type, parent.Get("waterMesh"), parent);
                        SquareWave(type, parent.Get("waterBatch"));
                        bool create = FoldOut.LargeButton("Create +", Tint.Orange, Tint.White, Icon.Get("BackgroundLight"));
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                        if (create)
                        {
                                if (main.type == WaterType.Square)
                                {
                                        main.CreateWaves();
                                }
                                else if (main.waterMesh.readyToCreate)
                                {
                                        main.CreateWaves();
                                }
                        }
                }

                public void Water (int type, SerializedProperty waterMesh, SerializedObject parent)
                {
                        if (FoldOut.Bar(parent, Tint.Blue).Label("Water", Color.white).FoldOut())
                        {
                                FoldOut.Box(5, FoldOut.boxColor, offsetY: -2);
                                parent.Field("Shape", "type");
                                parent.Field("Type", "swimType");
                                parent.Field("Switch Type", "canSwitch");
                                parent.Field("Segments", "particles");
                                parent.FieldToggle("Create On Awake", "createOnAwake");
                                Layout.VerticalSpacing(3);

                                if (type == 1)
                                {
                                        FoldOut.Box(2, FoldOut.boxColor);
                                        waterMesh.Field("Texture", "texture2D");
                                        waterMesh.Field("Material", "material");
                                        Layout.VerticalSpacing(5);

                                        if (main.GetComponent<MeshFilter>() == null)
                                                main.gameObject.AddComponent<MeshFilter>();
                                        if (main.GetComponent<MeshRenderer>() == null)
                                                main.gameObject.AddComponent<MeshRenderer>();
                                }

                                FoldOut.Box(5, FoldOut.boxColor);
                                SerializedProperty wave = parent.Get("wave");
                                wave.Slider("Amplitude", "amplitude", 0, 2f);
                                wave.Slider("Frequency", "frequency", 0, 5f);
                                wave.Slider("Speed", "speed", -10f, 10f);
                                wave.Slider("Spring", "spring", 0.01f, 0.1f);
                                wave.Slider("Damping", "damping", 0.01f, 0.2f);
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(2, FoldOut.boxColor);
                                wave.Slider("Turbulence", "turbulence", max: 1f);
                                wave.Slider("Random Current", "randomCurrent", 0, 5f);
                                Layout.VerticalSpacing(5);
                        }
                }

                public void RemoveMesh ()
                {
                        if (main.GetComponent<MeshFilter>() != null)
                                DestroyImmediate(main.GetComponent<MeshFilter>());
                        if (main.GetComponent<MeshRenderer>() != null)
                                DestroyImmediate(main.GetComponent<MeshRenderer>());
                }

                public void SquareWave (int type, SerializedProperty element)
                {
                        if (type != 0)
                                return;

                        RemoveMesh();
                        if (FoldOut.Bar(parent, Tint.Blue).Label("Body", Color.white).FoldOut("bodyFoldOut"))
                        {
                                FoldOut.Box(3, FoldOut.boxColor, offsetY: -2);
                                element.Field("Top", "crestColor", bold: true);
                                element.Field("Thickness", "crestThickness");
                                element.Slider("Taper", "crestTaper");
                                Layout.VerticalSpacing(3);

                                FoldOut.Box(1, FoldOut.boxColor);
                                element.Field("Middle", "bodyColor", bold: true);
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(4, FoldOut.boxColor);
                                element.Field("Bottom", "bottomColor", bold: true);
                                element.Field("Phase", "phaseShift");
                                element.Field("Offset", "bottomOffset");
                                element.Slider("Speed", "bottomSpeed", -15f, 15f);
                                Layout.VerticalSpacing(5);
                        }
                }

                public void OnSceneGUI ()
                {
                        parent.Update();
                        Vector2 position = main.transform.position;
                        Vector2 size = parent.Get("size").vector2Value;
                        SceneTools.DrawAndModifyBounds(ref position, ref size, Color.yellow);
                        parent.Get("size").vector2Value = size;
                        parent.ApplyModifiedProperties();
                        main.transform.position = Compute.Round(position, 0.25f);
                }

                public static void RenderWater (Water main)
                {
                        if (main == null)
                                return;

                        SceneView.duringSceneGui -= main.DrawBatches;
                        if (EditorApplication.isPlaying && EditorApplication.isPaused)
                        {
                                SceneView.duringSceneGui += main.DrawBatches;
                        }
                        else if (!EditorApplication.isPlaying)
                        {
                                SceneView.duringSceneGui += main.DrawBatches;
                        }
                }

                [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.InSelectionHierarchy)]
                public static void DrawWhenObjectIsNotSelected (Water main, GizmoType gizmoType)
                {
                        RenderWater(main);
                }

        }
}
