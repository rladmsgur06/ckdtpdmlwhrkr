using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(SwapCharacter))]
        [CanEditMultipleObjects]
        public class SwapCharacterEditor : UnityEditor.Editor
        {
                private SwapCharacter main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as SwapCharacter;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();

                        FoldOut.Box(3 , FoldOut.boxColor , extraHeight: 3);
                        {
                                parent.Field("Button" , "buttonSwap");
                                parent.Field("Death Tag" , "instantDeathTag");
                                parent.FieldToggleAndEnable("Reset To First" , "resetToFirst");
                        }

                        if (FoldOut.FoldOutButton(parent.Get("eventsFoldOut")))
                        {
                                Fields.EventFoldOutEffect(parent.Get("onSwap") , parent.Get("swapWE") , parent.Get("swapFoldOut") , "On Swap" , color: Tint.Orange);
                                Fields.EventFoldOut(parent.Get("onFailedToSwap") , parent.Get("failedFoldOut") , "On Failed To Swap" , color: Tint.Orange);
                        }

                        Layout.Update(0.05f);
                        SerializedProperty array = parent.Get("item").Get("list");

                        for (int i = 0; i < array.arraySize; i++)
                        {
                                SerializedProperty element = array.Element(i);
                                FoldOut.Box(2 , Tint.Blue , extraHeight: 3);
                                {
                                        if (element.FieldDoubleAndButton("" , "character" , "name" , "Delete"))
                                        {
                                                array.DeleteArrayElementAtIndex(i);
                                                break;
                                        }
                                        Rect grip = Layout.GetLastRect(20 , 19);
                                        ListReorder.Grip(parent , array , grip.CenterRectHeight() , i , Tint.WarmWhite);
                                        Labels.FieldText("Name" , execute: element.String("name") == "" , rightSpacing: 18);
                                        element.FieldToggleAndEnable("Is Locked" , "locked");
                                }
                                if (FoldOut.FoldOutButton(element.Get("eventsFoldOut")))
                                {
                                        Fields.EventFoldOut(element.Get("onActivate") , element.Get("activateFoldOut") , "On Activate" , color: Tint.Orange);
                                        Fields.EventFoldOut(element.Get("onDeactivate") , element.Get("deactivateFoldOut") , "On Deactivate" , color: Tint.Orange);
                                        Fields.EventFoldOut(element.Get("isLocked") , element.Get("lockedFoldOut") , "Is Locked" , color: Tint.Orange);
                                        Fields.EventFoldOut(element.Get("isUnlocked") , element.Get("unlockedFoldOut") , "Is Unlocked" , color: Tint.Orange);
                                }

                        }
                        if (FoldOut.CornerButton(Tint.Blue) || array.arraySize == 0) // add state
                        {
                                array.arraySize++;
                        }

                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

        }
}
