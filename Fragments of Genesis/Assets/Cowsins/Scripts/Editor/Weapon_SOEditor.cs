#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace cowsins2D
{
    [System.Serializable]
    [CustomEditor(typeof(Weapon_SO))]
    public class Weapon_SOEditor : Editor
    {
        private string[] tabs = { "Basic", "Shooting", "Stats","Visuals", "Others" };
        private int currentTab = 0;
        private float lockIconSize = 40;
        private Texture btnTexture;
        private GUIContent button;
        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            Weapon_SO myScript = target as Weapon_SO;

            Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/weaponWorkshop_CustomEditor") as Texture2D;
            GUILayout.Label(myTexture);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10f);
            if (GUILayout.Button("Adding New Weapons Tutorial", GUILayout.Height(20)))
            {
                Application.OpenURL("https://youtu.be/KoZu1D2gnR4");
            }
            EditorGUILayout.Space(10f);
            currentTab = GUILayout.Toolbar(currentTab, tabs);
            EditorGUILayout.Space(10f);
            EditorGUILayout.EndVertical();
            #region variables

            if (currentTab >= 0 || currentTab < tabs.Length)
            {
                switch (tabs[currentTab])
                {
    
                    case "Basic":
                        EditorGUILayout.LabelField("MAIN SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(20);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
                        GUILayout.Space(10);
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();

                            Texture2D tex = AssetPreview.GetAssetPreview(myScript.itemIcon);
                            GUILayout.Label("", GUILayout.Height(50), GUILayout.Width(50));
                            GUI.DrawTexture(GUILayoutUtility.GetLastRect(), tex);

                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemIcon"));
                        myScript.maxStack = 1;
                        EditorGUILayout.LabelField("This represents your weapon in the game. It must be a WeaponIdentification", EditorStyles.helpBox);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponObject"));
                        break;
                    case "Shooting":
                        EditorGUILayout.LabelField("SHOOTING SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(20);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("shootingStyle"));
                        switch (myScript.shootingStyle)
                        {
                            case Weapon_SO.ShootingStyle.Raycast:
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletsPerShot"));
                                if(myScript.bulletsPerShot > 1)
                                {
                                    EditorGUI.indentLevel++;
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("timeBetweenBullets"));
                                    EditorGUI.indentLevel--;
                                }
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoPerShot"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("spread"));
                                break;
                            case Weapon_SO.ShootingStyle.Projectile:
      
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletsPerShot"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("timeBetweenBullets"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoPerShot"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("spread"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("projectile"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("projectileSpeed"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("projectileDuration"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("projectileCollisionSize"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("explosiveProjectile"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("explosiveDamage"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("explosiveForce"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionRadius"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletsPerShot"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("timeBetweenBullets"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoPerShot"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("spread"));
                                EditorGUI.indentLevel--;
                                break;
                            case Weapon_SO.ShootingStyle.Melee:
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeAttackRadius"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeDistance"));
                                EditorGUI.indentLevel--;
                                break;
                            case Weapon_SO.ShootingStyle.Custom:
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletsPerShot"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("timeBetweenBullets"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoPerShot"));
                                break;
                        }
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("aimingMethod"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("shootingMethod"));
                        if (myScript.shootingMethod == Weapon_SO.ShootingMethod.ReleaseWhenReady || myScript.shootingMethod == Weapon_SO.ShootingMethod.ShootWhenReady)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("chargeRequiredToShoot"));
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("visualRecoil"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("visualRecoilRecovery"));
         
                        break;
                    case "Stats":
                        EditorGUILayout.LabelField("WEAPON STATISTICS SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(20);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("infiniteBullets"));
                        if(!myScript.infiniteBullets)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("limitedMagazines"));
                            if (myScript.limitedMagazines)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("amountOfMagazines"));
                                EditorGUI.indentLevel--;
                            }
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("magazineSize"));
                        }
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("fireRate"));
                        if (myScript.fireRate < myScript.timeBetweenBullets * myScript.bulletsPerShot) myScript.fireRate = myScript.timeBetweenBullets * myScript.bulletsPerShot; 
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoType"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadingMethod"));
                        if (myScript.reloadingMethod == Weapon_SO.ReloadingMethod.Overheat)
                        {
                            EditorGUI.indentLevel++; 
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadTime"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("coolSpeed"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("allowShootingAfterCoolingPercentage"));
                            EditorGUI.indentLevel--;
                        }
                        else
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoReload"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadTime"));
                            EditorGUI.indentLevel--;
                        }
                        if (myScript.canParry)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("canParry"));
                            EditorGUI.indentLevel--;
                        }

                        if(myScript.canParry && myScript.shootingStyle == Weapon_SO.ShootingStyle.Melee)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("parryProjectileSpeed"));
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("weight"));
                        break;
                    case "Visuals":
                        EditorGUILayout.LabelField("VISUAL SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(20);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("hitEffects"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("camShake"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("camShakeAmount"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("muzzleFlashVFX"));
                        break;
                    case "Others":
                        EditorGUILayout.LabelField("OTHER SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(20);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("crosshairShootingSpread"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("sounds"));
                        break;

                }
                GUILayout.Space(20);
                GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
            }

            #endregion
            EditorGUILayout.Space(10f);

            EditorGUILayout.Space(20f);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                // LOCK ICON 

                GUI.backgroundColor = Color.clear;

                btnTexture = ActiveEditorTracker.sharedTracker.isLocked
                    ? Resources.Load<Texture2D>("CustomEditor/lockedIcon") as Texture2D
                    : Resources.Load<Texture2D>("CustomEditor/unlockedIcon") as Texture2D;

                button = new GUIContent(btnTexture);

                if (GUILayout.Button(button, GUIStyle.none, GUILayout.Width(lockIconSize), GUILayout.Height(lockIconSize)))
                    ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();


            serializedObject.ApplyModifiedProperties();

        }
    }
}
#endif