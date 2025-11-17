using TwoBitMachines.Editors;
using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(BasicTimer) , true)]
        [CanEditMultipleObjects]
        public class BasicTimerEditor : UnityEditor.Editor
        {
                private BasicTimer main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as BasicTimer;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        {
                                FoldOut.Box(1 , Tint.Box);
                                parent.Field("Time Out" , "time");
                                Layout.VerticalSpacing(5);

                                Fields.EventFoldOut(parent.Get("onRestart") , parent.Get("restartFoldOut") , "On Restart");
                                Fields.EventFoldOut(parent.Get("onEnable") , parent.Get("enableFoldOut") , "On Enable");

                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

        }
}
