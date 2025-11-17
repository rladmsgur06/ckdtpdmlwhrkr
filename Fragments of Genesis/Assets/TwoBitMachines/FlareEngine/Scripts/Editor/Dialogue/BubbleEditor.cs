using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CanEditMultipleObjects, CustomEditor(typeof(Bubble))]
        public class BubbleEditor : UnityEditor.Editor
        {
                private Bubble main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as Bubble;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();
                        {
                                FoldOut.Box(3 , Tint.PurpleDark);
                                {
                                        parent.Field("Text Mesh" , "text");
                                        parent.Field("Image" , "image");
                                        parent.FieldToggle("Is Directional" , "isDirectional");
                                }
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(5 , FoldOut.boxColor , extraHeight: 3);
                                {
                                        parent.Field("Max Size" , "maxSize");
                                        parent.Field("Min Size" , "minSize");
                                        parent.Field("Padding" , "padding");
                                        parent.Field("Text Offset Y" , "offsetY");
                                        parent.Field("Bubble Offset X" , "directionXOffset");
                                }

                                if (FoldOut.FoldOutButton(parent.Get("eventsFoldOut")))
                                {
                                        Fields.EventFoldOut(parent.Get("transitionIn") , parent.Get("inFoldOut") , "On Transition In");
                                        Fields.EventFoldOut(parent.Get("transitionOut") , parent.Get("outFoldOut") , "On Transition Out");
                                        Fields.EventFoldOut(parent.Get("messageLoaded") , parent.Get("loadFoldOut") , "On Load Message");
                                }

                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }
        }
}
