using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        public class PriorityInspectorEditor
        {
                public static void Display (SerializedObject parent, SerializedProperty abilities, string[] names)
                {
                        SortAbilities(abilities);
                        if (FoldOut.Bar(parent, Tint.Orange).Label("Priority", Color.white).FoldOut("priorityFoldOut"))
                        {
                                for (int i = 0; i < abilities.arraySize; i++)
                                {
                                        SerializedObject ability = new SerializedObject(abilities.Element(i).objectReferenceValue);
                                        Label(parent, abilities, ability, ability.String("abilityName"), names, (i + 1).ToString() + ".", i, space: 5);
                                }
                                for (int i = 0; i < abilities.arraySize; i++)
                                {
                                        SerializedObject ability = new SerializedObject(abilities.Element(i).objectReferenceValue);
                                        ability.Update();
                                        ability.Get("ID").intValue = i;
                                        ability.ApplyModifiedProperties();
                                }
                        }
                }

                public static void SortAbilities (SerializedProperty array)
                {
                        int size = array.arraySize;
                        for (int i = 0; i < size; i++)
                        {
                                for (int j = 0; j < size - 1; j++)
                                {
                                        SerializedObject a = new SerializedObject(array.Element(j).objectReferenceValue);
                                        SerializedObject b = new SerializedObject(array.Element(j + 1).objectReferenceValue);

                                        if (b.Int("ID") < a.Int("ID"))
                                        {
                                                array.MoveArrayElement(j + 1, j);
                                        }
                                }
                        }
                }

                public static void Label (SerializedObject so, SerializedProperty array, SerializedObject ability, string name, string[] names, string index, int i, int space = 0)
                {
                        ability.Update();

                        bool open = FoldOut.Bar(ability, Tint.Box, height: 20).SL(17).Label(index + "  " + name, Color.black, false).FoldOut("editMask");
                        ListReorder.Grip(so, array, Bar.barStart.CenterRectHeight(), i, Tint.WarmWhite);
                        if (open)
                        {
                                SerializedProperty exceptions = ability.Get("exception");

                                FoldOut.BoxSingle(1, Tint.Box * Tint.LightGrey);
                                {
                                        if (Labels.LabelAndButton("Add Exception", "Add"))
                                        {
                                                GenericMenu menu = new GenericMenu();
                                                for (int j = 0; j < names.Length; j++)
                                                {
                                                        string tempName = names[j];
                                                        menu.AddItem(new GUIContent(names[j]), false, () =>
                                                        {
                                                                exceptions.serializedObject.Update();
                                                                exceptions.arraySize++;
                                                                exceptions.LastElement().stringValue = tempName;
                                                                exceptions.serializedObject.ApplyModifiedProperties();
                                                        });
                                                }
                                                menu.ShowAsContext();
                                        }
                                }
                                Layout.VerticalSpacing(2);

                                if (exceptions.arraySize > 0)
                                {
                                        FoldOut.Box(exceptions.arraySize, Tint.Box * Tint.LightGrey, offsetY: -2);
                                        for (int j = 0; j < exceptions.arraySize; j++)
                                        {
                                                if (Labels.LabelAndButton(exceptions.Element(j).stringValue, "Delete"))
                                                {
                                                        exceptions.DeleteArrayElement(j);
                                                }
                                        }
                                        Layout.VerticalSpacing(3);
                                }
                        }
                        ability.ApplyModifiedProperties();
                }
        }
}
