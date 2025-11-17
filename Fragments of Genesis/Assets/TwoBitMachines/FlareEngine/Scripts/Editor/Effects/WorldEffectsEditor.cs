using System.Collections.Generic;
using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(WorldEffects))]
        public class WorldEffectsEditor : UnityEditor.Editor
        {
                public WorldEffects main;
                public SerializedObject so;
                public List<string> bulletTypes = new List<string>();

                private void OnEnable ()
                {
                        main = target as WorldEffects;
                        so = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();

                        if (FoldOut.LargeButton("Grab Effects", Tint.Blue, Tint.WarmWhite, Icon.Get("BackgroundLight")))
                        {
                                main.effect.Clear();
                                for (int i = 0; i < main.transform.childCount; i++)
                                {
                                        WorldEffectPool pool = new WorldEffectPool();
                                        pool.gameObject = main.transform.GetChild(i).gameObject;
                                        main.effect.Add(pool);
                                }
                                Debug.Log("Found: " + main.effect.Count + " effects");
                        }


                        so.Update();

                        Block.Header(so).DropArrow(colorOn: Block.hardDarkToggle, colorOff: Block.hardDarkToggle).Build();

                        if (so.Bool("foldOut"))
                        {
                                for (int i = 0; i < main.effect.Count; i++)
                                {
                                        GUILayout.Label(" " + (i + 1).ToString() + ".  " + main.effect[i].gameObject.name);
                                }
                        }
                        so.ApplyModifiedProperties();
                }
        }
}

// using UnityEditor;
// using UnityEngine;
// using UnityEngine.UIElements;
// using TwoBitMachines.UIElement.Editor;

// namespace TwoBitMachines.FlareEngine.Editors
// {
//         [CustomEditor(typeof(WorldEffects))]
//         public class WorldEffectsEditor : Editor
//         {
//                 public WorldEffects main;
//                 public VisualElement root;
//                 public SerializedObject parent;
//                 public string tooltip = "Register all child gameobjects as effects.";//

//                 public void OnEnable ()
//                 {
//                         main = target as WorldEffects;
//                         parent = serializedObject;
//                         root = ElementTools.InitializeRoot();
//                 }

//                 public override VisualElement CreateInspectorGUI ()
//                 {
//                         Template.LargetButton(root, "Grab Effects", Tint.Blue, Tint.White, tooltip: tooltip).clicked += GrabEffects;
//                         return root;
//                 }

//                 private void GrabEffects ()
//                 {
//                         main.effect.Clear();
//                         for (int i = 0; i < main.transform.childCount; i++)
//                         {
//                                 WorldEffectPool pool = new WorldEffectPool();
//                                 pool.gameObject = main.transform.GetChild(i).gameObject;
//                                 main.effect.Add(pool);
//                         }
//                         Debug.Log("Found: " + main.effect.Count + " effects");
//                 }
//         }
// }
