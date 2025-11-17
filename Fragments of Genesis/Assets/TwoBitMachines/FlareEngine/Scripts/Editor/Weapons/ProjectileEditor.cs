using System;
using System.Collections.Generic;
using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.BulletType;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Projectile))]
        public class ProjectileEditor : UnityEditor.Editor
        {
                public Projectile main;
                private GameObject objReference;
                public SerializedObject parent;
                public List<string> bulletTypes = new List<string>();

                private void OnEnable ()
                {
                        main = target as Projectile;
                        parent = serializedObject;
                        objReference = main.gameObject;
                        Util.GetFileNames("TwoBitMachines", "/FlareEngine/Scripts/Weapons/Projectile/ProjectileType/BulletType", bulletTypes);
                        Util.GetFileNames("", UserFolderPaths.Path(UserFolder.Bullet), bulletTypes, false);
                        bulletTypes.Remove("BulletBase");
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        parent.Update();
                        {
                                SerializedObject baseParent = ChooseType();
                                baseParent.Update();
                                int type = parent.Enum("type");

                                if (type == 0)
                                {
                                        ProjectilesEditor.Header("Bullet", baseParent, parent);
                                        ProjectilesEditor.Setup(baseParent, bulletTypes);
                                        ProjectilesEditor.BulletType(main, baseParent, type);
                                }
                                else if (type == 1)
                                {
                                        ProjectilesEditor.Header("Instant", baseParent, parent);
                                        ProjectilesEditor.Instant(baseParent, parent);
                                }
                                else if (type == 2)
                                {
                                        ProjectilesEditor.Header("Short Range", baseParent, parent);
                                        ProjectilesEditor.ShortRange(baseParent, parent);
                                }
                                else
                                {
                                        ProjectilesEditor.Header("Grappling Hook", baseParent, parent);
                                        ProjectilesEditor.GrapplingHook(baseParent, parent);
                                }
                                baseParent.ApplyModifiedProperties();
                        }
                        parent.ApplyModifiedProperties();
                }

                private SerializedObject ChooseType ()
                {
                        int type = parent.Enum("type");
                        if (type == 0)
                        {
                                if (main.gameObject.GetComponent<ProjectileBullet>() == null)
                                {
                                        Component comp = main.gameObject.AddComponent<ProjectileBullet>();
                                        parent.Get("projectile").objectReferenceValue = comp as ProjectileBase;
                                        comp.hideFlags = HideFlags.HideInInspector;
                                }
                                EditorTools.Remove<ProjectileInstant>(main.gameObject);
                                EditorTools.Remove<ProjectileShortRange>(main.gameObject);
                                EditorTools.Remove<ProjectileGrapplingHook>(main.gameObject);
                        }
                        if (type == 1)
                        {
                                if (main.gameObject.GetComponent<ProjectileInstant>() == null)
                                {
                                        Component comp = main.gameObject.AddComponent<ProjectileInstant>();
                                        parent.Get("projectile").objectReferenceValue = comp as ProjectileBase;
                                        comp.hideFlags = HideFlags.HideInInspector;
                                }
                                EditorTools.Remove<ProjectileBullet>(main.gameObject);
                                EditorTools.Remove<ProjectileShortRange>(main.gameObject);
                                EditorTools.Remove<ProjectileGrapplingHook>(main.gameObject);
                        }
                        if (type == 2)
                        {
                                if (main.gameObject.GetComponent<ProjectileShortRange>() == null)
                                {
                                        Component comp = main.gameObject.AddComponent<ProjectileShortRange>();
                                        parent.Get("projectile").objectReferenceValue = comp as ProjectileBase;
                                        comp.hideFlags = HideFlags.HideInInspector;
                                }
                                EditorTools.Remove<ProjectileBullet>(main.gameObject);
                                EditorTools.Remove<ProjectileInstant>(main.gameObject);
                                EditorTools.Remove<ProjectileGrapplingHook>(main.gameObject);
                        }
                        if (type == 3)
                        {
                                if (main.gameObject.GetComponent<ProjectileGrapplingHook>() == null)
                                {
                                        Component comp = main.gameObject.AddComponent<ProjectileGrapplingHook>();
                                        parent.Get("projectile").objectReferenceValue = comp as ProjectileBase;
                                        comp.hideFlags = HideFlags.HideInInspector;
                                }
                                EditorTools.Remove<ProjectileBullet>(main.gameObject);
                                EditorTools.Remove<ProjectileInstant>(main.gameObject);
                                EditorTools.Remove<ProjectileShortRange>(main.gameObject);
                        }
                        ProjectileBase projectile = main.gameObject.GetComponent<ProjectileBase>();
                        projectile.hideFlags = HideFlags.HideInInspector;
                        return new SerializedObject(projectile);
                }

                private void OnDisable ()
                {
                        if (main == null && objReference != null && !EditorApplication.isPlayingOrWillChangePlaymode)
                        {
                                objReference.AddComponent<ProjectileClean>();
                        }
                }
        }

        public static class ProjectilesEditor
        {
                public static List<SerializedProperty> events = new List<SerializedProperty>();

                public static void Header (string title, SerializedObject baseParent, SerializedObject parent)
                {
                        if (FoldOut.Bar(baseParent, Tint.Orange).Label(title).FoldOut())
                        {
                                FoldOut.Box(2, FoldOut.boxColor, offsetY: -2);
                                {
                                        baseParent.Field("Name", "projectileName");
                                        parent.Field("Projectile Type", "type");
                                }
                                Layout.VerticalSpacing(3);
                                ProjectilesEditor.Ammo(baseParent);
                        }
                }

                public static void Ammo (SerializedObject baseParent)
                {
                        SerializedProperty ammo = baseParent.Get("ammunition");
                        int type = ammo.Enum("type");
                        int height = type == 2 ? -1 : 0;
                        if (ammo.String("saveKey") == "")
                        {
                                ammo.Get("saveKey").stringValue = System.Guid.NewGuid().ToString();
                        }
                        FoldOut.Box(5 + height, FoldOut.boxColor, extraHeight: 3);
                        {
                                ammo.Field("Ammo Type", "type");
                                if (type != 2)
                                {
                                        ammo.FieldDouble("Ammo Amount", "ammunition", "max");
                                        Labels.FieldText("Max");
                                }
                                baseParent.FieldDouble("Ammo Damage", "damage", "damageForce");
                                Labels.FieldDoubleText("Damage", "Force");
                                baseParent.Field("Fire Rate", "fireRate");
                                ammo.FieldToggleAndEnable("Save Ammo", "save");
                        }

                        if (FoldOut.FoldOutButton(baseParent.Get("ammoEventFoldOut")))
                        {
                                Fields.EventFoldOut(ammo.Get("onAmmoReload"), ammo.Get("reloadFoldOut"), "On Ammo Reload");
                                Fields.EventFoldOut(ammo.Get("onAmmoEmpty"), ammo.Get("emptyFoldOut"), "On Ammo Empty");
                        }
                }

                public static void Setup (SerializedObject baseParent, List<string> bulletTypes)
                {
                        if (FoldOut.Bar(baseParent, Tint.Orange).Label("Setup And Pattern").FoldOut("setupFoldOut"))
                        {
                                SerializedProperty pattern = baseParent.Get("pattern");
                                FoldOut.Box(4, FoldOut.boxColor, offsetY: -2);
                                {
                                        baseParent.DropDownList(bulletTypes.ToArray(), "Bullet Type", "bulletType");
                                        baseParent.Field("Pool Size", "poolSize");
                                        pattern.FieldIntClamp("Projectile Rate", "projectileRate", 1, 10000);
                                        pattern.Field("Position Flux", "variance");
                                }
                                Layout.VerticalSpacing(3);

                                if (pattern.Int("projectileRate") > 1)
                                {
                                        FoldOut.Box(3, FoldOut.boxColor);
                                        {
                                                pattern.Field("Angle", "angle");
                                                pattern.Field("Separation", "separation");
                                                pattern.Field("Direction", "fireDirection");
                                        }
                                        Layout.VerticalSpacing(5);
                                }
                        }
                }

                public static void BulletType (Projectile main, SerializedObject baseParent, int projectileType)
                {
                        if (main.transform.childCount > 0)
                        {
                                baseParent.Get("projectileObject").objectReferenceValue = main.transform.GetChild(0).gameObject; // This is the reference to the bullet gameobject
                        }

                        if (main.transform.childCount == 0)
                        {
                                GUILayout.Label("Add a bullet child gameObject");
                                return;
                        }

                        Transform child = main.transform.GetChild(0);

                        if (child == null)
                        {
                                return;
                        }

                        SerializedProperty bulletType = baseParent.Get("bulletType");
                        BulletBase bulletBase = child.GetComponent<BulletBase>();
                        if (bulletBase == null || bulletBase.nameType != bulletType.stringValue)
                        {
                                if (bulletBase != null)
                                        MonoBehaviour.DestroyImmediate(bulletBase);
                                string actionType = "TwoBitMachines.FlareEngine.BulletType." + bulletType.stringValue;
                                if (EditorTools.RetrieveType(actionType, out Type type))
                                {
                                        bulletBase = child.gameObject.AddComponent(type) as BulletBase;
                                        bulletBase.nameType = bulletType.stringValue;
                                }
                        }

                        if (bulletBase != null)
                        {
                                SerializedObject bullet = new SerializedObject(bulletBase);
                                bullet.Update();

                                if (FoldOut.Bar(baseParent, Tint.Orange).Label("Properties").FoldOut("propertiesFoldOut"))
                                {
                                        int fields = EditorTools.CountObjectFields(bullet);
                                        if (fields != 0)
                                                FoldOut.Box(fields, FoldOut.boxColor, offsetY: -2);
                                        EditorTools.IterateObject(bullet, fields);
                                        if (fields == 0)
                                                Layout.VerticalSpacing(3);
                                }
                                bullet.ApplyModifiedProperties();
                        }
                }

                public static void Instant (SerializedObject baseParent, SerializedObject parent)
                {
                        if (FoldOut.Bar(baseParent, Tint.Orange).Label("Properties").FoldOut("propertiesFoldOut"))
                        {
                                FoldOut.Box(7, FoldOut.boxColor, extraHeight: 3);
                                baseParent.Field("Layer", "layer");
                                baseParent.Field("Edge Collider", "ignoreEdges");
                                baseParent.Field("On Idle", "release");
                                baseParent.Field("Max Length", "maxLength");
                                baseParent.Field("Hit Rate", "hitRate");
                                baseParent.Field("Impact Object", "impactObject");
                                baseParent.Field("Impact Effect", "impactEffect");
                                bool eventOpen = FoldOut.FoldOutButton(baseParent.Get("propertiesEventFoldOut"));
                                Fields.EventFoldOut(baseParent.Get("onFire"), baseParent.Get("foldOutOnFire"), "On Fire", execute: eventOpen);
                                Fields.EventFoldOut(baseParent.Get("onImpact"), baseParent.Get("impactFoldOut"), "On Impact", execute: eventOpen);
                        }
                }

                public static void GrapplingHook (SerializedObject baseParent, SerializedObject parent)
                {
                        if (FoldOut.Bar(baseParent, Tint.Orange).Label("Properties").FoldOut("propertiesFoldOut"))
                        {
                                FoldOut.Box(4, FoldOut.boxColor, offsetY: -2);
                                {
                                        baseParent.Field("Layer", "layer");
                                        baseParent.Field("Edge Collider", "ignoreEdges");
                                        baseParent.FieldDouble("Hook Object", "hook", "hookOffset");
                                        Labels.FieldText("Offset", rightSpacing: 3);
                                        baseParent.FieldToggleAndEnable("Set To FirePoint Position", "setToFirePoint");
                                }
                                Layout.VerticalSpacing(3);

                                FoldOut.Box(5, FoldOut.boxColorLight, extraHeight: 3);
                                {
                                        baseParent.FieldDouble("Line Renderer", "lineRenderer", "pointsInLine");
                                        Labels.FieldText("Points");
                                        baseParent.FieldDouble("Line Length", "minLength", "maxLength");
                                        Labels.FieldDoubleText("Min", "Max");
                                        baseParent.ClampInt("pointsInLine", min: 2, max: 1000);
                                        baseParent.FieldDouble("Shoot Curve", "shootCurve", "amplitude");
                                        Labels.FieldText("Amplitude");
                                        baseParent.Field("Shoot Speed", "speed");
                                        baseParent.FieldToggle("Off On Contact", "disableOnContact");
                                }
                                if (FoldOut.FoldOutButton(baseParent.Get("propertiesEventFoldOut")))
                                {
                                        Fields.EventFoldOutEffect(baseParent.Get("onHook"), baseParent.Get("hookWE"), baseParent.Get("hookFoldOut"), "On Hook", color: Tint.Blue);
                                        Fields.EventFoldOut(baseParent.Get("onEnable"), baseParent.Get("onEnableFoldOut"), "On Enable", color: Tint.Blue);
                                        Fields.EventFoldOut(baseParent.Get("onMissed"), baseParent.Get("missedFoldOut"), "On Missed", color: Tint.Blue);
                                        Fields.EventFoldOut(baseParent.Get("offOnContact"), baseParent.Get("offOnContactFoldOut"), "Off On Contact", color: Tint.Blue);
                                        Fields.EventFoldOut(baseParent.Get("onEmergencyExit"), baseParent.Get("emergencyFoldOut"), "On Emergency Exit", color: Tint.Blue);
                                }
                                FoldOut.Box(3, FoldOut.boxColorLight);
                                {
                                        baseParent.Field("Swing Force", "swingForce");
                                        baseParent.Field("Jump Away", "jumpAway");
                                        baseParent.Field("Gravity", "gravity");
                                }
                                Layout.VerticalSpacing(5);

                                if (FoldOut.Bar(baseParent, FoldOut.boxColorLight).Label("Retract", FoldOut.titleColor, false).BRE("retract").FoldOut("retractBarFoldOut"))
                                {
                                        FoldOut.Box(5, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);

                                        GUI.enabled = baseParent.Bool("retract");
                                        {
                                                int type = baseParent.Enum("retractType");
                                                baseParent.Field("Type", "retractType", execute: type == 0);
                                                baseParent.FieldDouble("Type", "retractType", "retractButton", execute: type == 1);
                                                baseParent.Field("Speed", "rate");
                                                baseParent.Field("Delay", "retractDelay");
                                                baseParent.Field("Friction", "retractFriction");
                                                baseParent.ClampV2("retractFriction");
                                                baseParent.FieldToggle("Off On Complete", "disableOnRetractComplete");
                                                if (FoldOut.FoldOutButton(baseParent.Get("retractMainFoldOut")))
                                                {
                                                        Fields.EventFoldOut(baseParent.Get("onRetractComplete"), baseParent.Get("retractFoldOut"), "On Retract Complete", color: Tint.Blue);
                                                }
                                        }
                                        GUI.enabled = true;
                                }

                                if (FoldOut.Bar(baseParent, FoldOut.boxColorLight).Label("Grab Item", FoldOut.titleColor, false).BRE("canGrabItem").FoldOut("grabItemFoldOut"))
                                {
                                        FoldOut.Box(4, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);

                                        GUI.enabled = baseParent.Bool("canGrabItem");
                                        {
                                                baseParent.Field("Find By Tag", "itemTag");
                                                baseParent.Field("Find By Tag Class", "itemTagID");
                                                baseParent.Field("Retract Speed", "grabRetractSpeed");
                                                baseParent.Field("Release Distance", "releaseDistance");

                                                if (FoldOut.FoldOutButton(baseParent.Get("grabMainFoldOut")))
                                                {
                                                        Fields.EventFoldOutEffect(baseParent.Get("onItemGrabComplete"), baseParent.Get("grabCompleteWE"), baseParent.Get("grabItemCompleteFoldOut"), "On Grab Item Complete", color: Tint.Blue);
                                                        Fields.EventFoldOut(baseParent.Get("onItemGrabFound"), baseParent.Get("grabItemFoundFoldOut"), "On Grab Item Found", color: Tint.Blue);
                                                        Fields.EventFoldOut(baseParent.Get("onItemGrabFailed"), baseParent.Get("grabItemFailedFoldOut"), "On Grab Item Failed", color: Tint.Blue);
                                                }
                                        }
                                        GUI.enabled = true;
                                }
                        }
                }

                public static void ShortRange (SerializedObject baseParent, SerializedObject parent)
                {
                        if (FoldOut.Bar(baseParent, Tint.Orange).Label("Properties").FoldOut("propertiesFoldOut"))
                        {
                                FoldOut.Box(4, FoldOut.boxColor, extraHeight: 3);
                                baseParent.Field("Layer", "layer");
                                baseParent.Field("Collider", "boxCollider");
                                baseParent.Field("On Idle", "release");
                                baseParent.Field("Hit Rate", "hitRate");
                                bool eventOpen = FoldOut.FoldOutButton(baseParent.Get("propertiesEventFoldOut"));
                                Fields.EventFoldOut(baseParent.Get("onFire"), baseParent.Get("foldOutOnFire"), "On Fire", execute: eventOpen);
                        }
                }
        }
}
