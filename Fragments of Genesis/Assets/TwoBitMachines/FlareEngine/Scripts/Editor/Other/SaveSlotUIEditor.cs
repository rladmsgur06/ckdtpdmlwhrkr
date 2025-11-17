using TwoBitMachines.Editors;
using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(SaveSlotUI) , true)]
        [CanEditMultipleObjects]
        public class SaveSlotUIEditor : UnityEditor.Editor
        {
                private SaveSlotUI main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as SaveSlotUI;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        {
                                FoldOut.Box(4 , Tint.PurpleDark);
                                parent.Field("Play Time Text" , "playTime");
                                parent.Field("Level Text" , "levelNumber");
                                parent.Field("Highlight Offset" , "highlightOffset");
                                parent.Field("Selected Offset" , "selectedOffset");
                                Layout.VerticalSpacing(5);

                                Fields.EventFoldOut(parent.Get("slotClicked") , parent.Get("slotClickedFoldOut") , "Slot Clicked");
                                Fields.EventFoldOut(parent.Get("slotSelected") , parent.Get("slotSelectedFoldOut") , "Slot Selected");
                                Fields.EventFoldOut(parent.Get("slotDataDeleted") , parent.Get("slotDataDeletedFoldOut") , "Slot Data Deleted");
                                Fields.EventFoldOut(parent.Get("slotIsInitialized") , parent.Get("slotIsInitializedFoldOut") , "Slot Is Initialized");
                                Fields.EventFoldOut(parent.Get("slotHasBeenInitialized") , parent.Get("slotHasBeenInitializedFoldOut") , "Slot Has Been Initialized");
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

        }
}
