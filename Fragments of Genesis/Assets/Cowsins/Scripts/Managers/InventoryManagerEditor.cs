#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace cowsins2D
{
    [System.Serializable]
    [CustomEditor(typeof(InventoryManager))]
    public class InventoryManagerEditor : Editor
    {
        private int currentTab = 0;

        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            InventoryManager myScript = target as InventoryManager;

            Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/inventoryManager_CustomEditor") as Texture2D;
            GUILayout.Label(myTexture);

            string[] tabs = myScript.InventoryMethod == InventoryMethod.Full
                ? new string[] { "Main", "Others", "Events" }
                : new string[] { "Main" };

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

                        bool isFull = myScript.InventoryMethod == InventoryMethod.Full;
                        bool isNone = myScript.InventoryMethod == InventoryMethod.None;

                        if (isNone)
                            EditorGUILayout.HelpBox($"Inventory is fully disabled.", MessageType.Info, true);

                        if (!isFull)
                            EditorGUILayout.HelpBox($"´Others´ and ´Events´ tabs are hidden because the Inventory Method is not set to Full.", MessageType.Warning, true);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("inventoryMethod"));
                        
                        if(!isNone) EditorGUILayout.PropertyField(serializedObject.FindProperty("hotbarSize"));

                        if (isFull)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("inventory"));
                            EditorGUILayout.HelpBox($"Inventory Size: {myScript.InventorySize} Slots", MessageType.Info, true);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("inventoryRowsAmount"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("inventoryColumnsAmount"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("dropOnOutsideRelease"));
                            EditorGUI.indentLevel--;
                        }
                        break;
                    case "Others":
                        EditorGUILayout.LabelField("OTHER SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        if(myScript.InventoryMethod == InventoryMethod.Full)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("openInventorySFX"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("closeInventorySFX"));
                        }
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