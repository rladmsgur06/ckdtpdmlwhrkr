#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace cowsins2D
{
    [System.Serializable]
    [CustomEditor(typeof(InteractionManager))]
    public class InteractionManagerEditor : Editor
    {
        private string[] tabs = { "Main", "Assignables", "Others", "Events" };
        private int currentTab = 0;

        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            InteractionManager myScript = target as InteractionManager;

            Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/interactionManager_CustomEditor") as Texture2D;
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

                    case "Main":
                        EditorGUILayout.LabelField("MAIN SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("timeToInteract"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("canDrop"));
                        if (myScript.CanDrop)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("dropDistance"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("dropImpulse"));
                            EditorGUI.indentLevel--;
                        }
                        break;
                    case "Assignables":
                        EditorGUILayout.LabelField("ASSIGNABLES", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("genericWeaponPickeable"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("genericAmmoPickeable"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("genericPickeable"));
                        break;
                    case "Others":
                        EditorGUILayout.LabelField("OTHER SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("pickUpItemSFX"));
                        break;
                    case "Events":
                        EditorGUILayout.LabelField("EVENTS", EditorStyles.boldLabel);
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