using TwoBitMachines.Editors;
using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Quest))]
        [CanEditMultipleObjects]
        public class QuestEditor : UnityEditor.Editor
        {
                public Quest main;
                public SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as Quest;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        {
                                FoldOut.Box(3 , Tint.Delete);
                                {
                                        parent.Field("Quest SO" , "questSO");
                                        parent.Field("Quest UI" , "questUI");
                                        parent.Field("Add To Journal" , "journal");
                                }
                                Layout.VerticalSpacing(5);

                                Fields.EventFoldOut(parent.Get("onQuestAccepted") , parent.Get("acceptFoldOut") , "On Quest Accepted" , color: FoldOut.boxColor);
                                Fields.EventFoldOut(parent.Get("onQuestCompleted") , parent.Get("completeFoldOut") , "On Quest Completed" , color: FoldOut.boxColor);
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                }

        }
}
