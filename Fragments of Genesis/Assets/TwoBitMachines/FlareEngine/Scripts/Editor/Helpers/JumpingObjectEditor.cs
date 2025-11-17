using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(JumpingObject))]
        [CanEditMultipleObjects]
        public class JumpingObjectEditor : UnityEditor.Editor
        {
                private JumpingObject main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as JumpingObject;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        parent.Update();
                        {
                                Layout.VerticalSpacing(10);
                                FoldOut.Box(2 , Tint.Box);
                                parent.Field("Object Radius" , "objectRadius");
                                parent.Field("Time Out" , "timeOut");
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
                        }
                        parent.ApplyModifiedProperties();
                }
        }
}
