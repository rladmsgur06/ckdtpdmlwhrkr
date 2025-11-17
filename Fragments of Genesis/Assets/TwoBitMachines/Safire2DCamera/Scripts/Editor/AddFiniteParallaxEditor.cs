using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.Safire2DCamera.Editors
{
        [CanEditMultipleObjects, CustomEditor(typeof(AddFiniteParallax))]
        public class AddFiniteParallaxEditor : UnityEditor.Editor
        {
                private AddFiniteParallax main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as AddFiniteParallax;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();
                        {
                                FoldOut.Box(3 , FoldOut.boxColor);
                                {
                                        parent.Field("Rate" , "parallaxRate");
                                        parent.Field("Offset" , "startingOffset");
                                        parent.FieldToggleAndEnable("Must Be Visible" , "mustBeVisible");
                                }
                                Layout.VerticalSpacing(5);
                        }
                        parent.ApplyModifiedProperties();
                }

        }
}
