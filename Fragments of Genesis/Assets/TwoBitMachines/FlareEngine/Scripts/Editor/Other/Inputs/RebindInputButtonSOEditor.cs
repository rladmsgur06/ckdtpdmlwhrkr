using TwoBitMachines.Editors;
using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(RebindInputButtonSO) , true)]
        public class RebindInputButtonSOEditor : UnityEditor.Editor
        {
                private RebindInputButtonSO main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as RebindInputButtonSO;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        {
                                FoldOut.Box(5 , Tint.PurpleDark);
                                {
                                        parent.Field("Input Button SO" , "inputButtonSO");
                                        int type = parent.Enum("resetTypeTo");
                                        parent.FieldDouble("Reset Key To" , "resetTypeTo" , "resetKeyTo" , execute: type == 0);
                                        parent.FieldDouble("Reset Key To" , "resetTypeTo" , "resetMouseTo" , execute: type == 1);
                                        parent.Field("Key Label" , "buttonLabel");
                                        parent.Field("Binding Label" , "bindingLabel");
                                        parent.FieldToggle("Override key Label" , "overrideButtonLabel");
                                }
                                Layout.VerticalSpacing(5);

                                Fields.EventFoldOut(parent.Get("rebindStartEvent") , parent.Get("startFoldOut") , "Rebind Start" , color: Tint.Box);
                                Fields.EventFoldOut(parent.Get("rebindStopEvent") , parent.Get("stopFoldOut") , "Rebind Stop" , color: Tint.Box);
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

        }
}
