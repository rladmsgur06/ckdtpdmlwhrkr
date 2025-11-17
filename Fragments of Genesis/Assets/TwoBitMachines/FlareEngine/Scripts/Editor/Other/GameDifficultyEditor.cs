using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(GameDifficulty) , true)]
        public class GameDifficultyEditor : UnityEditor.Editor
        {
                private GameDifficulty main;
                private SerializedObject parent;
                private string saveFolder;
                public static SaveOptions save = new SaveOptions();

                private void OnEnable ()
                {
                        main = target as GameDifficulty;
                        parent = serializedObject;
                        Layout.Initialize();

                        Storage.encrypt = false; // probably not right?
                        SaveOptions.Load(ref save);
                        saveFolder = save.RetrieveSaveFolder();
                        main.Restore(saveFolder);
                }

                public override void OnInspectorGUI ()
                {
                        //bool saveChanges = false;
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        {
                                EditorGUI.BeginChangeCheck();
                                SerializedProperty difficulty = parent.Get("difficulty");
                                FoldOut.Box(1 , Tint.Delete);
                                {
                                        difficulty.Field("Difficulty" , "difficulty");
                                }
                                Layout.VerticalSpacing(5);

                                SerializedProperty array = difficulty.Get("level");


                                difficulty.ClampInt("difficulty" , 0 , Mathf.Max(0 , array.arraySize - 1));

                                for (int i = 0; i < array.arraySize; i++)
                                {
                                        SerializedProperty level = array.Element(i);
                                        bool open = level.Bool("foldOut");
                                        if (FoldOut.Bar(level , Tint.Blue)
                                        .Grip(difficulty , array , i , color: Tint.WarmWhite)
                                        .Label("Difficulty: " + i.ToString() , Color.white)
                                        .RightButton("delete" , "Delete" , toolTip: "Delete Difficulty" , execute: open)
                                        .RightButton(toolTip: "Add Behaviour" , execute: open)
                                        .FoldOut())
                                        {
                                                if (level.ReadBool("delete"))
                                                {
                                                        array.DeleteArrayElement(i);
                                                        break;
                                                }
                                                SerializedProperty behaviour = level.Get("behaviour");
                                                if (level.ReadBool("add"))
                                                {
                                                        behaviour.arraySize++;
                                                }
                                                if (behaviour.arraySize == 0)
                                                {
                                                        Layout.VerticalSpacing(3);
                                                }
                                                for (var j = 0; j < behaviour.arraySize; j++)
                                                {
                                                        SerializedProperty behaviourE = behaviour.Element(j);
                                                        FoldOut.Box(2 , Tint.Box);
                                                        {
                                                                if (behaviourE.FieldAndButton("Behaviour" , "type" , "Delete"))
                                                                {
                                                                        behaviour.DeleteArrayElementAtIndex(j);
                                                                        break;
                                                                }
                                                                behaviourE.Field("Value" , "value");
                                                        }
                                                        Layout.VerticalSpacing(5);
                                                }

                                        }
                                }
                                if (FoldOut.CornerButton(Tint.Blue))
                                {
                                        array.arraySize++;
                                }
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                        if (EditorGUI.EndChangeCheck())
                        {
                                main.Save(saveFolder);
                        }
                }



        }
}
