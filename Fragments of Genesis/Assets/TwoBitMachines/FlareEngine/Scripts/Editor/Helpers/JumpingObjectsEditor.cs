using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(JumpingObjects))]
        [CanEditMultipleObjects]
        public class JumpingObjectsEditor : UnityEditor.Editor
        {
                private JumpingObjects main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as JumpingObjects;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        parent.Update();
                        {
                                Layout.VerticalSpacing(10);
                                FoldOut.Box(4 , Tint.Box);
                                parent.Field("Object Radius" , "objectRadius");
                                parent.Field("Time Out" , "timeOut");
                                parent.Field("Fade Out" , "fadeOut");
                                parent.FieldDoubleAndEnable("Random Sprites" , "randomMin" , "randomMax" , "isRandom");
                                Labels.FieldDoubleText("Min" , "Max" , rightSpacing: 19);
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(6 , Tint.Box);
                                parent.FieldDouble("Jump Force X" , "jumpForceXMin" , "jumpForceXMax");
                                Labels.FieldDoubleText("Min" , "Max");
                                parent.FieldDouble("Jump Force Y" , "jumpForceYMin" , "jumpForceYMax");
                                Labels.FieldDoubleText("Min" , "Max");
                                parent.Field("Bounce Friction" , "bounceFriction");
                                parent.Field("Gravity" , "gravity");
                                parent.FieldAndEnable("Random Rotation" , "randomRotation" , "useRandomRotation");
                                parent.FieldToggleAndEnable("Use Damage Direction" , "useDamageDirection");
                                Layout.VerticalSpacing(5);

                                bool flyToTarget = parent.Bool("flyToTarget");

                                FoldOut.Box(5 , Tint.Box);
                                parent.FieldAndEnable("Fly To Target" , "target" , "flyToTarget");
                                GUI.enabled = flyToTarget;
                                parent.Field("Wait Time" , "waitTime");
                                parent.Field("Fly Time" , "flyTime");
                                parent.Field("Curve Path" , "curve");
                                parent.FieldAndEnable("Has Radius" , "flyToRadius" , "useRadius");
                                GUI.enabled = true;
                                Layout.VerticalSpacing(5);

                                SerializedProperty item = parent.Get("item");
                                if (item.arraySize == 0)
                                        item.arraySize++;

                                FoldOut.Box(item.arraySize , Tint.Box);
                                for (int i = 0; i < item.arraySize; i++)
                                {
                                        Fields.ArrayProperty(item , item.Element(i).Get("sprite") , i , "Sprite");
                                }
                                Layout.VerticalSpacing(5);
                        }
                        parent.ApplyModifiedProperties();
                }
        }
}
