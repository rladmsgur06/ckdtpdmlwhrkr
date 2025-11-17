using TwoBitMachines.Editors;
using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CanEditMultipleObjects, CustomEditor(typeof(Journal))]
        public class JournalEditor : UnityEditor.Editor
        {
                private Journal main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as Journal;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();

                        FoldOut.Box(3 , Tint.PurpleDark , extraHeight: 3);
                        {
                                parent.Field("Name" , "journalName");
                                parent.Field("Save Key" , "saveKey");
                                parent.FieldDouble("Activate" , "toggle" , "pauseType");
                        }

                        if (FoldOut.FoldOutButton(parent.Get("eventsFoldOut")))
                        {
                                Fields.EventFoldOut(parent.Get("onOpen") , parent.Get("openFoldOut") , "On Open" , color: Tint.PurpleDark);
                                Fields.EventFoldOut(parent.Get("onClose") , parent.Get("closeFoldOut") , "On Close" , color: Tint.PurpleDark);
                        }

                        FoldOut.Box(2 , Tint.BoxTwo);
                        {
                                parent.FieldDouble("Back, Forward" , "back" , "forward");
                                parent.FieldAndEnable("Range View" , "range" , "hasRange");
                        }
                        Layout.VerticalSpacing(5);

                        FoldOut.Box(3 , Tint.BoxTwo);
                        {
                                parent.Field("Slot Prefab" , "slot");
                                parent.Field("Slot Parent" , "slotParent");
                                parent.FieldDouble("Slot Highlight" , "highlight" , "highlightOffset");
                        }
                        Layout.VerticalSpacing(5);

                        FoldOut.Box(4 , Tint.BoxTwo);
                        {
                                parent.Field("Icon" , "icon");
                                parent.Field("Title" , "title");
                                parent.Field("Extra Info" , "extraInfo");
                                parent.Field("Description" , "description");
                        }
                        Layout.VerticalSpacing(5);

                        SerializedProperty array = parent.Get("inventory");
                        if (array.arraySize == 0)
                        {
                                array.arraySize++;
                        }
                        FoldOut.Box(array.arraySize , FoldOut.boxColor);
                        for (int j = 0; j < array.arraySize; j++)
                        {
                                SerializedProperty element = array.Element(j);
                                Fields.ArrayProperty(array , element , j , "Inventory");
                        }
                        Layout.VerticalSpacing(5);

                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                }

        }
}
