using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.Interactables;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Foliage))]
        public class FoliageEditor : UnityEditor.Editor
        {
                private Foliage main;
                private GameObject brush;
                private string instances;
                private SpriteRenderer renderer;
                private SerializedObject parent;
                private FoliageBrushes sceneBrush = new FoliageBrushes();

                private void OnEnable ()
                {
                        parent = serializedObject;
                        main = target as Foliage;
                        main.transform.localScale = Vector3.one;

                        Layout.Initialize();
                        RenderFoliage(main);
                }

                private void OnDisable ()
                {
                        if (brush != null)
                                DestroyImmediate(brush);
                }

                public override void OnInspectorGUI ()
                {
                        if (main.textureArray.material == null)
                        {
                                string path = "Assets/TwoBitMachines/FlareEngine/AssetsFolder/Materials/Foliage.mat";
                                Material newMaterial = (Material) AssetDatabase.LoadAssetAtPath(path, typeof(Material));
                                if (newMaterial != null)
                                {
                                        main.textureArray.material = MonoBehaviour.Instantiate(newMaterial); //     create new instance, or else different game objects with this material will have the same textures
                                        main.textureArray.mesh = QuadMesh.Create();
                                        main.textureArray.mesh.MarkDynamic();
                                        EditorUtility.SetDirty(main);
                                }
                        }

                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        SerializedProperty textArray = parent.Get("textureArray");
                        SerializedProperty textures = textArray.Get("textures");
                        SerializedProperty foliage = textArray.Get("foliage");
                        int listSize = foliage.arraySize;
                        bool refresh = false;

                        parent.Update();
                        {
                                CreateTexture(textures, textArray);
                                FoliageInspector.Properties(parent.Get("foliage"), parent);

                                if (FoldOut.Bar(parent, Tint.Blue).Label("Paint", Color.white).FoldOut("foldOutPaint"))
                                {
                                        FoldOut.Box(3, FoldOut.boxColor);
                                        if (textArray.FieldAndButton("Add Texture2D", "textureSize", "Add"))
                                                textArray.SetTrue("createTexture");
                                        textArray.Field("PPU", "PPU");
                                        textArray.Field("Material", "material");
                                        Layout.VerticalSpacing(5);
                                        FoliageInspector.TextureBoxes(parent, textures, textArray.Get("textureSize"), textArray.Get("PPU"), this);
                                        FoliageInspector.TextureProperties(parent.Get("index"), textures, foliage, ref refresh);
                                        FoliageInspector.PaintBar(parent, parent.Get("brushes"));
                                }

                                instances = main.textureArray.foliage.Count.ToString() + " / 1023";
                                FoldOut.BoxSingle(1, Tint.Orange);
                                Labels.Label("Instances :     " + instances);
                                Layout.VerticalSpacing(2);

                        }
                        parent.ApplyModifiedProperties();

                        Layout.VerticalSpacing(20);

                        if (listSize != foliage.arraySize || refresh) // a texture has been added or deleted, recreate text array
                        {
                                main.Create();
                                SceneView.RepaintAll();
                        }
                }

                public void OnSceneGUI ()
                {
                        Layout.Update();
                        RenderFoliage(main);

                        if (EditorApplication.isPlaying || Mouse.MouseRightDown() || main.brushes < 0 || main.brushes == FoliageBrush.None)
                        {
                                main.brushes = FoliageBrush.None;
                                if (brush != null)
                                        MonoBehaviour.DestroyImmediate(brush);
                                return;
                        }
                        if (main.brushes == FoliageBrush.Eraser)
                        {
                                if (brush != null)
                                        MonoBehaviour.DestroyImmediate(brush);
                        }
                        if (brush == null && main.brushes != FoliageBrush.None && main.brushes != FoliageBrush.Eraser)
                        {
                                brush = new GameObject();
                                brush.name = "FoliageBrush";
                                brush.hideFlags = HideFlags.DontSave;
                                brush.transform.parent = main.transform;
                                brush.transform.localScale = Vector3.one;
                                renderer = brush.AddComponent<SpriteRenderer>();
                        }

                        SerializedProperty foliage = parent.Get("textureArray").Get("foliage");
                        int listSize = foliage.arraySize;

                        parent.Update();
                        sceneBrush.SetBrushSprite(main, parent.Get("index"), renderer, brush);
                        sceneBrush.SingleBrush(parent, foliage, brush, main.brushes);
                        sceneBrush.RandomBrush(parent, foliage, brush, main.brushes);
                        sceneBrush.EraseBrush(parent, foliage, main.brushes);
                        parent.ApplyModifiedProperties();

                        if (listSize != foliage.arraySize) // an instance has been added or deleted, recreate textarray
                        {
                                main.Create();
                                SceneView.RepaintAll();
                        }
                        if (Event.current.type == EventType.Layout)
                        {
                                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                        }
                }

                public void CreateTexture (SerializedProperty textures, SerializedProperty textureArray)
                {
                        if (textures.arraySize == 0 || textureArray.ReadBool("createTexture"))
                        {
                                textures.arraySize++;
                                textures.LastElement().Get("interact").floatValue = 1f;
                        }
                }

                public static void RenderFoliage (Foliage main)
                {
                        if (main == null)
                                return;
                        if (EditorApplication.isPlaying)
                        {
                                SceneView.duringSceneGui -= main.DrawMeshes;
                                SceneView.duringSceneGui -= main.DrawMeshesOnPause;
                                return;
                        }

                        if (EditorApplication.isPlaying)
                        {
                                SceneView.duringSceneGui -= main.DrawMeshes;
                                SceneView.duringSceneGui -= main.DrawMeshesOnPause;
                                if (EditorApplication.isPaused)
                                {
                                        SceneView.duringSceneGui -= main.DrawMeshesOnPause;
                                        SceneView.duringSceneGui += main.DrawMeshesOnPause;
                                }
                        }
                        else
                        {
                                SceneView.duringSceneGui -= main.DrawMeshes;
                                SceneView.duringSceneGui += main.DrawMeshes;
                                SceneView.duringSceneGui -= main.DrawMeshesOnPause;
                        }
                }

                [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.InSelectionHierarchy)]
                public static void DrawWhenObjectIsNotSelected (Foliage main, GizmoType gizmoType)
                {
                        RenderFoliage(main);
                        if (main.textureArray.mesh == null && main.textureArray.foliage.Count > 0)
                        {
                                main.Create();
                                SceneView.RepaintAll();
                        }
                }
        }

}
