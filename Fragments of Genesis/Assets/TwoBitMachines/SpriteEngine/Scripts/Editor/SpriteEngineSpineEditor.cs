using System.Collections.Generic;
using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

#if SPINE_UNITY
using Spine.Unity;
#endif

namespace TwoBitMachines.TwoBitSprite.Editors
{
        [CustomEditor(typeof(SpriteEngineSpine), true)]
        [CanEditMultipleObjects]
        public class SpriteEngineSpineEditor : UnityEditor.Editor
        {
#if SPINE_UNITY
#pragma warning disable CS1061
                public SpriteEngineSpine main;
                public SerializedObject parent;
                public static List<string> animations = new List<string>();
                public List<string> names = new List<string>() { "Empty" };
                public int index = -1;

                private void OnEnable ()
                {
                        main = target as SpriteEngineSpine;
                        parent = serializedObject;
                        Layout.Initialize();
                        index = -1;
                }

                private void GetAnimationList ()
                {
                        if (main == null || main.animator == null)
                                return;
                        try
                        {
                                if (main.animator.skeleton.Data == null || main.animator.skeleton.Data.Animations == null)
                                        return;
                        }
                        catch
                        {
                                return;
                        }
                        if (main.animator.Skeleton.Data.Animations.Count > 0 && names.Count != index)
                        {
                                Spine.Animation[] anim = main.animator.Skeleton.Data.Animations.ToArray();
                                names.Clear();
                                for (int i = 0; i < anim.Length; i++)
                                {
                                        names.Add(anim[i].Name);
                                }
                                index = anim.Length;
                        }
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        GetAnimationList();
                        serializedObject.Update();

                        if (Block.Header(parent).Style(Tint.SoftDark).Fold("Animator", "foldOut", true, Tint.White).Button().Build())
                        {
                                Block.Box(2, Tint.SoftDark, noGap: true);
                                {
                                        parent.Field_("Skeleton Anim", "animator");
                                        parent.FieldToggleAndEnable_("Initialize To First Animation", "setToFirst");                   
                                }

                                SerializedProperty animations = parent.Get("animations");

                                if (Header.SignalActive("Add"))
                                {
                                        animations.CreateNewElement();
                                }

                                for (int i = 0; i < animations.arraySize; i++)
                                {
                                        SerializedProperty element = animations.Element(i);
                                        Block.Header(element).Style(Tint.SoftDark)
                                             .Grip(this, animations, i)
                                             .DropArrow()
                                             .DropList("name", names, 5)
                                             .Button("Delete")
                                             .Build();

                                        if (Header.SignalActive("Delete"))
                                        {
                                                animations.DeleteArrayElement(i);
                                                break;
                                        }

                                        if (element.Bool("foldOut"))
                                        {
                                                Block.Box(3, Tint.SoftDark, noGap: true);
                                                {
                                                        element.FieldAndEnable_("Synchronize", "syncID", "canSync");
                                                        Block.HelperText("Sync ID", rightSpacing: 18);
                                                        element.FieldToggleAndEnable_("Loop Once", "loopOnce");
                                                        element.FieldToggleAndEnable_("Random Animations", "isRandom");
                                                }

                                                if (element.Bool("isRandom"))
                                                {
                                                        SerializedProperty array = element.Get("randomAnimations");
                                                        Block.BoxArray(array, Tint.SoftDark, 23, false, 1, "Animation Name, Probability (0-1)", (height, index) =>
                                                        {
                                                                Block.Header(array.Element(index)).BoxRect(Tint.SoftDark, leftSpace: 5, height: height)
                                                                                  .Field("animation", weight: 0.75f)
                                                                                  .Field("weight", weight: 0.25f)
                                                                                  .ArrayButtons()
                                                                                  .BuildGet()
                                                                                  .ReadArrayButtons(array, index);
                                                        });
                                                }
                                                if (element.Bool("loopOnce"))
                                                {
                                                        Fields.EventField(element.Get("onLoopOnce"));
                                                }
                                        }
                                        // FoldOut.BoxSingle(1, Tint.SoftDark, yOffset: -2);
                                        // {
                                        //         //SerializedProperty element = animations.Element(i);
                                        //         Fields.ConstructField(-2);
                                        //         Fields.ConstructSpace(20);
                                        //         element.ConstructList(names, "name", S.FW - 45);

                                        //         if (Fields.ConstructButton("Delete"))
                                        //         {
                                        //                 animations.DeleteArrayElement(i);
                                        //                 break;
                                        //         }
                                        //         if (Fields.ConstructButton("Reopen"))
                                        //         {
                                        //                 element.Toggle("foldOut");
                                        //         }
                                        //         if (element.Bool("foldOut"))
                                        //         {
                                        //                 FoldOut.BoxSingle(3, Tint.SoftDark, extraHeight: 2, yOffset: -2);
                                        //                 element.FieldAndEnable("Synchronize", "syncID", "canSync");
                                        //                 Labels.FieldText("Sync ID", rightSpacing: 18);
                                        //                 element.FieldToggle("Loop Once", "loopOnce");
                                        //                 element.FieldToggle("Random Animation", "isRandom");
                                        //                 if (element.Bool("isRandom"))
                                        //                 {
                                        //                         SerializedProperty array = element.Get("randomAnimations");
                                        //                         if (array.arraySize == 0)
                                        //                                 array.arraySize++;
                                        //                         FoldOut.Box(array.arraySize, Tint.SoftDark, yOffset: -2);
                                        //                         for (int j = 0; j < array.arraySize; j++)
                                        //                         {
                                        //                                 Fields.ArrayPropertyFieldDouble(array, j, "Random Animation", "animation", "weight");
                                        //                         }
                                        //                         Layout.VerticalSpacing(3);
                                        //                 }
                                        //                 Fields.EventFoldOut(element.Get("onLoopOnce"), element.Get("loopFoldOut"), "On Loop Once", color: Tint.SoftDark);

                                        //         }
                                        // }
                                        // ListReorder.Grip(parent, animations, Fields.fieldRect, i, Tint.WarmWhite);
                                }

                        }
                        SpriteTreeEditor.TreeInspector(this, main.tree, parent.Get("tree"), AnimationList(), main.tree.signals);
                        TransitionEditor.Transition(this, parent, parent.Get("animations"), "animations", AnimationList(), main.tree.signals);
                        SpriteEngineEditor.ShowCurrentState(main.currentAnimation);

                        serializedObject.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                        if (GUI.changed && !EditorApplication.isPlaying)
                        {
                                Repaint();
                        }
                }


                public List<string> AnimationList ()
                {
                        animations.Clear();
                        for (int i = 0; i < main.animations.Count; i++)
                        {
                                animations.Add(main.animations[i].name);
                        }
                        return animations;
                }
#pragma warning restore CS1061
#else
                public override void OnInspectorGUI ()
                {
                        Labels.InfoBox(60,
                                "First install Spine-Unity. Then add the SPINE_UNITY symbol into Project" +
                                " Settings > Player > Script Compilation > Scripting Define Symbols and click Apply."
                        );
                }
#endif
        }
}
