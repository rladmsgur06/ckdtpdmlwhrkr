using System.Collections.Generic;
using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite.Editors
{
        [CustomEditor(typeof(SpriteEngineUA), true)]
        [CanEditMultipleObjects]
        public class SpriteEngineUAEditor : UnityEditor.Editor
        {
                public SpriteEngineUA main;
                public SerializedObject parent;
                public List<string> animationNames = new List<string>();

                private void OnEnable ()
                {
                        main = target as SpriteEngineUA;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        serializedObject.Update();

                        SerializedProperty animations = parent.Get("animations");
                        if (Block.Header(parent).Style(Tint.SoftDark).Fold("Animator", "foldOut", true, Tint.White).Button().Build())
                        {
                                Block.Box(3, Tint.SoftDark, noGap: true);
                                {
                                        int type = parent.Enum("flipX");
                                        parent.Field_("Animator", "animator");
                                        parent.Field_("Flip X", "flipX", execute: type == 0);
                                        parent.FieldDouble_("Flip X", "flipX", "flipAngle", execute: type == 1);
                                        parent.FieldToggleAndEnable_("Initialize To First Animation", "setToFirst");

                                }

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
                                             .Field("name", rightSpace: 5)
                                             .Button("Delete")
                                             .Build();

                                        if (Header.SignalActive("Delete"))
                                        {
                                                animations.DeleteArrayElement(i);
                                                break;
                                        }

                                        if (element.Bool("foldOut"))
                                        {
                                                Block.Box(2, Tint.SoftDark, noGap: true);
                                                {
                                                        element.FieldAndEnable_("Synchronize", "syncID", "canSync");
                                                        Block.HelperText("Sync ID", rightSpacing: 18);
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
                                        }
                                }
                        }
                        animations.CreateNameList(animationNames);
                        SpriteTreeEditor.TreeInspector(this, main.tree, parent.Get("tree"), animationNames, main.tree.signals);
                        TransitionEditor.Transition(this, parent, parent.Get("animations"), "animations", animationNames, main.tree.signals);
                        SpriteEngineEditor.ShowCurrentState(main.currentAnimation);

                        serializedObject.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

        }
}
