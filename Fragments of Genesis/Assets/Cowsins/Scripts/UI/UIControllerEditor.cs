#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace cowsins2D
{
    [System.Serializable]
    [CustomEditor(typeof(UIController))]
    public class UIControllerEditor : Editor
    {
        private string[] tabs = { "Player Stats", "Player Movement", "Weapons", "Interaction", "Experience & Coins", "Inventory" };
        private int currentTab = 0;

        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            UIController myScript = target as UIController;

            Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/uiController_CustomEditor") as Texture2D;
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

                    case "Player Stats":
                        EditorGUILayout.LabelField("REFERENCES", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("player"));
                        
                        GUILayout.Space(10);

                        EditorGUILayout.LabelField("HEALTH & SHIELD", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("healthDisplayMethod"));

                        if(myScript._HealthDisplayMethod == UIController.HealthDisplayMethod.Bar || myScript._HealthDisplayMethod == UIController.HealthDisplayMethod.Both)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("healthUI"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("shieldUI"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("lerpBarValueSpeed"));
                            EditorGUI.indentLevel--;
                        }
                        if(myScript._HealthDisplayMethod == UIController.HealthDisplayMethod.Numeric || myScript._HealthDisplayMethod == UIController.HealthDisplayMethod.Both)
                        {
                            EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("healthText"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("shieldText"));
                            EditorGUI.indentLevel--;
                        }
                        GUILayout.Space(10);

                        EditorGUILayout.LabelField("EFFECTS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("healthVignette"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("hurtVignetteColor"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("healVignetteColor"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("lerpVignetteSpeed"));
                        break;
                    case "Player Movement":
                        EditorGUILayout.LabelField("STAMINA", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaSlider"));
                        
                        GUILayout.Space(10);
                        
                        EditorGUILayout.LabelField("DASH", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashIcon"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashUIContainer"));
                        
                    break;
                    case "Weapons":
                        EditorGUILayout.LabelField("WEAPONS UI", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletsUI"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("magazineUI"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("overheatUI"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponDisplay"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("lowAmmoUI"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadUI"));
                    break;
                    case "Interaction":
                        EditorGUILayout.LabelField("INTERACTION", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("interactUI"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("interactText"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("interactProgress"));
                        break;
                    case "Experience & Coins":
                        EditorGUILayout.LabelField("COINS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("coinsUI"));
                        GUILayout.Space(10);
                        
                        EditorGUILayout.LabelField("EXPERIENCE", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("xpImage"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("currentLevel"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("nextLevel"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("lerpXpSpeed"));
                    break;
                     case "Inventory":
                        EditorGUILayout.LabelField("INVENTORY", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("inventoryContainer"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("hotbarContainer"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("chestContainer"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("inventoryGraphics"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("chestGraphics"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("inventorySlot"));
                        GUILayout.Space(10);
                        
                        EditorGUILayout.LabelField("STYLE", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("gapX"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("gapY"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("inventoryPadding"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("hotbarPadding"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("chestPadding"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("hotbarSelected"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("hotbarDefault"));
                        GUILayout.Space(10);
                        
                        EditorGUILayout.LabelField("DEBUG", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("currentInventorySlot"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("highlightedInventorySlot"));
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