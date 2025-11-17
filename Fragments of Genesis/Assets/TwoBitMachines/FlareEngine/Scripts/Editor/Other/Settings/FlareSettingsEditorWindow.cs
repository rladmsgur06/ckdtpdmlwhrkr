using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace TwoBitMachines.FlareEngine.Editors
{
        public class FlareSettingsEditorWindow : EditorWindow
        {
                [MenuItem("Window/Flare Settings")]
                public static void ShowWindow ()
                {
                        GetWindow<FlareSettingsEditorWindow>("Flare Settings");
                }


                private List<string> paths = new List<string>();
                private List<Object> pathObjects = new List<Object>();

                private void OnEnable ()
                {
                        paths.Clear();
                        pathObjects.Clear();

                        for (int i = 0; i < UserFolderPaths.paths.Length; i++)
                        {
                                string path = PlayerPrefs.GetString(UserFolderPaths.paths[i], "");
                                paths.Add(path);
                                pathObjects.Add(string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Object>(path));
                        }
                }

                private void OnGUI ()
                {
                        GUILayout.Label("");
                        for (int i = 0; i < UserFolderPaths.paths.Length; i++)
                        {
                                Object previousFolder = pathObjects[i];
                                pathObjects[i] = EditorGUILayout.ObjectField(UserFolderPaths.pathLabel[i], pathObjects[i], typeof(DefaultAsset), false);

                                if (previousFolder != pathObjects[i])
                                {
                                        if (pathObjects[i] != null)
                                        {
                                                string newPath = AssetDatabase.GetAssetPath(pathObjects[i]);
                                                PlayerPrefs.SetString(UserFolderPaths.paths[i], newPath);
                                                PlayerPrefs.SetString(UserFolderPaths.paths[i] + "Name", pathObjects[i].name);
                                        }
                                        else
                                        {
                                                PlayerPrefs.SetString(UserFolderPaths.paths[i], "");
                                                PlayerPrefs.SetString(UserFolderPaths.paths[i] + "Name", "");
                                        }
                                        AIEditor.dataList.Clear();
                                        AIEditor.actionList.Clear();
                                }
                        }
                        GUILayout.Label("");
                        EditorGUILayout.LabelField("To refresh folder references, recompile code or enter and exit playmode.", EditorStyles.wordWrappedLabel);
                }
        }


}
