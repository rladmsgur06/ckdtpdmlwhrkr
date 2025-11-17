using TwoBitMachines.Editors;
using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(OptionsBindingMenu) , true)]
        public class BindingMenuEditor : UnityEditor.Editor
        {
                private OptionsBindingMenu main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as OptionsBindingMenu;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        {
                                FoldOut.Box(7 , Tint.PurpleDark);
                                {
                                        parent.Field("Full Screen" , "fullScreen");
                                        parent.Field("Resolutions" , "resolutions");
                                        parent.Field("Music Volume" , "music");
                                        parent.Field("SFX Volume" , "sfx");
                                        parent.Field("Reset All Keys" , "resetAll");
                                        parent.Field("Audio Manager" , "audioManager");
                                        parent.Field("Input Action" , "inputAction");
                                }
                                Layout.VerticalSpacing(5);
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

        }
}
