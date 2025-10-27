#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace cowsins2D
{
    [System.Serializable]
    [CustomEditor(typeof(WeaponController))]
    public class WeaponControllerEditor : Editor
    {
        private string[] tabs = { "Main", "Assignables", "Settings", "Events" };
        private int currentTab = 0;

        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            WeaponController myScript = target as WeaponController;

            Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/weaponController_CustomEditor") as Texture2D;
            GUILayout.Label(myTexture);


            EditorGUILayout.BeginVertical();
            currentTab = GUILayout.SelectionGrid(currentTab, tabs, 6);
            EditorGUILayout.Space(10f);
            EditorGUILayout.EndVertical();
            #region variables

            if (currentTab >= 0 || currentTab < tabs.Length)
            {
                switch (tabs[currentTab])
                {
                    case "Assignables":
                        EditorGUILayout.LabelField("ASSIGNABLES", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("playerCamera"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponHolder"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("controllerAimSensitivity"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("hitLayer"));
                        break;
                    case "Main":
                        EditorGUILayout.LabelField("MAIN SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("initialWeapons"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("customShot"));
                        break;

                    case "Settings":
                        EditorGUILayout.LabelField("SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("canShootWhileLadder"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("canShootWhileGliding"));
                        break;
                    case "Events":
                        EditorGUILayout.LabelField("SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("events"));
                        break;
                }
            }

            #endregion
            EditorGUILayout.Space(10f);
            serializedObject.ApplyModifiedProperties();

        }
    }
}
#endif