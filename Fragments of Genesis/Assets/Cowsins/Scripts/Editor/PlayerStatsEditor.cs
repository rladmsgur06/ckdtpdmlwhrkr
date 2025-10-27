#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace cowsins2D
{
    [System.Serializable]
    [CustomEditor(typeof(PlayerStats))]
    public class PlayerStatsEditor : Editor
    {
        private string[] tabs = { "Main", "Health Regeneration", "Fall Damage", "Others", "Events" };
        private int currentTab = 0;

        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            PlayerStats myScript = target as PlayerStats;

            Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/playerStats_CustomEditor") as Texture2D;
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
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("health"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("shield"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealth"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxShield"));
                        break;
                    case "Health Regeneration":
                        EditorGUILayout.LabelField("HEALTH REGENERATION", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("healthRegenerationMethod"));
                        if (myScript._HealthRegenerationMethod != PlayerStats.HealthRegenerationMethod.None)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("healthRegenerationAmount"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("healthRegenerationRate"));

                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("timeToRevive"));
                        break;
                    case "Fall Damage":
                        EditorGUILayout.LabelField("FALL DAMAGE", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("takeFallDamage"));
                        if (myScript.takeFallDamage)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("dontTakeFallDamageIfWallSliding"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("dontTakeFallDamageIfGliding"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumHeightDifferenceToApplyDamage"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("fallDamageMultiplier"));
                            EditorGUI.indentLevel--;
                        }
                        break;
                    case "Others":
                        EditorGUILayout.LabelField("HURT/DAMAGE EFFECTS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("hurtCameraShake"));
                        GUILayout.Space(5);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("flashesOnDamage"));
                        if (myScript.FlashesOnDamage)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("flashDuration"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("flashSpeed"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("sounds"));
                            EditorGUI.indentLevel--;
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