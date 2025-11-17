using TwoBitMachines.Editors;
using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(ScreenTransition))]
        public class ScreenTransitionEditor : UnityEditor.Editor
        {
                private ScreenTransition main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as ScreenTransition;
                        parent = serializedObject;

                        Layout.Initialize();

                }
                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        {
                                SceneManagement();
                        }
                        parent.ApplyModifiedProperties();
                }

                public void SceneManagement ()
                {

                        int type = parent.Enum("type");
                        int height = type == 2 ? 2 : 0;
                        FoldOut.Box(2 + height , FoldOut.boxColor , extraHeight: 3);

                        parent.Field("Type" , "type");
                        parent.Field("Transition Time" , "time");

                        if (type == 2)
                        {
                                parent.Field("Hold Time" , "holdTime");
                                parent.FieldToggle("Reset Game" , "resetGame");
                        }

                        if (FoldOut.FoldOutButton(parent.Get("eventFoldOut")))
                        {
                                Fields.EventFoldOut(parent.Get("onStart") , parent.Get("startFoldOut") , "On Start");
                                Fields.EventFoldOut(parent.Get("onComplete") , parent.Get("completeFoldOut") , "On Complete");
                        }

                        SerializedProperty text = parent.Get("text");
                        if (text.arraySize == 0)
                                text.arraySize++;
                        FoldOut.Box(text.arraySize , FoldOut.boxColor);
                        text.FieldProperty("Random Text");
                        Layout.VerticalSpacing(5);

                }

        }
}
