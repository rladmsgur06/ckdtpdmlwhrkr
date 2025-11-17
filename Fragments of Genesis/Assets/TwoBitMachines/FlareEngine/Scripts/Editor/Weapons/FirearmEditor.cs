using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Firearm)), CanEditMultipleObjects]
        public class FirearmEditor : UnityEditor.Editor
        {
                private Firearm main;
                private SerializedObject parent;
                private Color eventColor => Tint.Orange;

                private void OnEnable ()
                {
                        main = target as Firearm;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        {
                                Firearm();
                                Rotation();
                                LineOfSight();
                                ChargedProjectile();

                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

                public void Firearm ()
                {
                        if (FoldOut.Bar(parent).Label("Firearm").FoldOut())
                        {
                                FoldOut.Box(2, FoldOut.boxColor, offsetY: -2);
                                {
                                        parent.Field("Name", "toolName");
                                        parent.Field("Local Position", "localPosition");
                                }
                                Layout.VerticalSpacing(3);

                                SerializedProperty projectile = parent.Get("defaultProjectile");
                                SerializedProperty animation = projectile.Get("waitForAnimation");
                                int extra = animation.Bool("wait") ? 1 : 0;

                                FoldOut.Box(4 + extra, FoldOut.boxColor, extraHeight: 3);
                                {
                                        projectile.Field("Projectile", "projectile");
                                        parent.Field("Fire Point", "firePoint");
                                        parent.FieldDouble("Button", "buttonTrigger", "input");
                                        animation.FieldAndEnable("Shoot Animation", "weaponAnimation", "wait");
                                        Labels.FieldText("Signal Name", rightSpacing: Layout.boolWidth, execute: animation.String("weaponAnimation") == "");

                                        if (extra > 0)
                                        {
                                                animation.FieldDouble("Extra Animation", "spriteEngine", "extraSignal");
                                        }
                                }

                                if (FoldOut.FoldOutButton(parent.Get("mainFoldOut")))
                                {
                                        Fields.EventFoldOut(parent.Get("onActivated"), parent.Get("eventFoldOut"), "On Enable", color: eventColor);
                                        Fields.EventFoldOut(projectile.Get("onFireSuccess"), projectile.Get("fireFoldOut"), "On Fire Success", color: eventColor);
                                        Fields.EventFoldOut(projectile.Get("onOutOfAmmo"), projectile.Get("outOfAmmoFoldOut"), "On Out Of Ammo", color: eventColor);
                                }


                                if (FoldOut.Bar(parent, FoldOut.boxColor * Tint.WarmGrey).Label("Options", FoldOut.titleColor, bold: false).FoldOut("optionsFoldOut"))
                                {
                                        FoldOut.Box(6, FoldOut.boxColor, offsetY: -2);
                                        {
                                                projectile.Field("Auto Discharge", "autoDischarge");
                                                parent.Field("Inventory", "projectileInventory");
                                                parent.FieldDoubleAndEnable("Pick Up", "pickUpLayer", "pickUpType", "pickUp");

                                                int stopType = parent.Enum("stopType");
                                                parent.FieldDoubleAndEnable("Stop Vel X", "stopType", "stopTime", "stopVelocity", execute: stopType == 0);
                                                parent.FieldAndEnable("Stop Vel X", "stopType", "stopVelocity", execute: stopType == 1);
                                                Labels.FieldText("Time", rightSpacing: 18, execute: stopType == 0);
                                                parent.FieldToggleAndEnable("Off Near Wall", "turnOffNearWall");
                                                SerializedProperty rotate = parent.Get("rotate");
                                                rotate.FieldToggleAndEnable("Set Rotation Signals", "setRotationSignals");
                                        }
                                        Layout.VerticalSpacing(3);
                                }

                                Recoil("Recoil", projectile.Get("recoil"), null);
                        }
                }

                public void Rotation ()
                {
                        SerializedProperty rotate = parent.Get("rotate");

                        if (FoldOut.Bar(parent).Label("Rotation").FoldOut("rotateFoldOut"))
                        {
                                int index = rotate.Enum("rotate");

                                if (index == 4)
                                {
                                        FoldOut.Box(2, FoldOut.boxColor);
                                        {
                                                rotate.Field("Rotate With", "rotate");
                                                rotate.Field("Fixed Direction", "fixedDirection");
                                        }
                                        Layout.VerticalSpacing(5);
                                        return;
                                }
                                if (index == 5)
                                {
                                        FoldOut.Box(1, FoldOut.boxColor);
                                        {
                                                rotate.Field("Rotate With", "rotate");
                                        }
                                        Layout.VerticalSpacing(5);
                                        return;
                                }

                                int orientation = rotate.Enum("orientation");
                                FoldOut.Box(2, FoldOut.boxColor, offsetY: -2);
                                {
                                        rotate.Field("Rotate With", "rotate");
                                        rotate.Field("Point Towards", "orientation", execute: orientation < 2);
                                        rotate.FieldDouble("Point Towards", "orientation", "transformDirection", execute: orientation > 1);
                                }
                                Layout.VerticalSpacing(3);

                                if (index == 0)
                                {
                                        FoldOut.Box(5, FoldOut.boxColor);
                                        {
                                                if (orientation != 0)
                                                {
                                                        GUI.enabled = false;
                                                        rotate.Slider("Top Limit", "maxLimit", 0, 180);
                                                        rotate.Get("maxLimit").floatValue = 180;
                                                        GUI.enabled = true;
                                                }
                                                else
                                                {
                                                        rotate.Slider("Top Limit", "maxLimit", 0, 180);
                                                }
                                                rotate.Slider("Bottom Limit", "minLimit", -180, 0);

                                                if (rotate.Float("maxLimit") < rotate.Float("minLimit"))
                                                {
                                                        rotate.Get("maxLimit").floatValue = rotate.Float("minLimit");
                                                }
                                                rotate.Field("Angle Offset", "angleOffset");
                                                rotate.FieldAndEnable("Round Direction", "roundMouse", "roundMouseDirection");
                                                rotate.Field("JoyStick", "joyStick");
                                        }
                                        Layout.VerticalSpacing(5);
                                }
                                if (index == 1)
                                {
                                        FoldOut.Box(3, FoldOut.boxColor);
                                        {
                                                rotate.FieldDouble("Left,  Right", "left", "right");
                                                rotate.FieldDouble("Up,  Down", "up", "down");
                                                rotate.Field("Diagonal", "diagonal");
                                        }
                                        Layout.VerticalSpacing(5);
                                }
                                if (index == 2)
                                {
                                        AutoSeek(rotate);
                                }
                                if (index == 3)
                                {
                                        FoldOut.Box(1, FoldOut.boxColor);
                                        {
                                                rotate.Field("Fixed Angle", "fixedAngle");
                                        }
                                        Layout.VerticalSpacing(5);
                                }

                        }
                }

                public void LineOfSight ()
                {
                        if (FoldOut.Bar(parent).Label("Aim").FoldOut("lineOfSightFoldOut"))
                        {
                                SerializedProperty lineOfSight = parent.Get("lineOfSight");

                                int type = lineOfSight.Enum("lineOfSight");

                                if (type == 0)
                                {
                                        FoldOut.Box(1, FoldOut.boxColor, offsetY: -2);
                                        lineOfSight.Field("Line Of Sight", "lineOfSight");
                                        Layout.VerticalSpacing(3);
                                }
                                else if (type == 1)
                                {
                                        FoldOut.Box(7, FoldOut.boxColor, offsetY: -2, extraHeight: 3);
                                        {
                                                lineOfSight.Field("Line Of Sight", "lineOfSight");
                                                lineOfSight.Field("Layer", "layer");
                                                lineOfSight.Field("Target", "targetLayer");

                                                lineOfSight.Field("Beam", "beam");
                                                lineOfSight.Field("Beam End", "beamEnd");
                                                lineOfSight.Field("Max Length", "maxLength");
                                                lineOfSight.FieldToggleAndEnable("Auto Shoot", "autoShoot");
                                        }

                                        if (FoldOut.FoldOutButton(lineOfSight.Get("eventsFoldOut"), offsetY: -2))
                                        {
                                                Fields.EventFoldOut(lineOfSight.Get("onTargetHit"), lineOfSight.Get("onTargetHitFoldOut"), "On Target Hit", color: eventColor);
                                                Fields.EventFoldOut(lineOfSight.Get("onBeamHit"), lineOfSight.Get("onBeamHitFoldOut"), "On Beam Hit", color: eventColor);
                                                Fields.EventFoldOut(lineOfSight.Get("onNothingHit"), lineOfSight.Get("onNothingHitFoldOut"), "On Nothing Hit", color: eventColor);
                                        }

                                }
                                else if (type == 2)
                                {
                                        bool rType = (int) lineOfSight.Enum("reticleType") == 0;
                                        FoldOut.Box(rType ? 4 : 3, FoldOut.boxColor, offsetY: -2);
                                        lineOfSight.Field("Line Of Sight", "lineOfSight");
                                        lineOfSight.Field("Aim Reticle", "reticle");
                                        lineOfSight.Field("Follow Type", "reticleType");
                                        if ((int) lineOfSight.Enum("reticleType") == 0)
                                        {
                                                lineOfSight.Field("Distance", "reticleDistance");
                                        }
                                        Layout.VerticalSpacing(3);
                                }
                        }
                }

                public void ChargedProjectile ()
                {
                        SerializedProperty charge = parent.Get("chargedProjectile");
                        if (FoldOut.Bar(charge).Label("Charge").SR(5).BRE("canCharge").FoldOut("chargeFoldOut"))
                        {
                                GUI.enabled = charge.Bool("canCharge");
                                {
                                        SerializedProperty animation = charge.Get("waitForAnimation");
                                        int extra = animation.Bool("wait") ? 1 : 0;
                                        if (Application.isPlaying && charge.Bool("canCharge"))
                                        {
                                                FoldOut.Box(6 + extra, FoldOut.boxColor, offsetY: -2, extraHeight: 3);
                                                GUILayout.Label("Fully Charged:   " + charge.Get("percentCharged").floatValue.ToString());
                                        }
                                        else
                                        {
                                                FoldOut.Box(5 + extra, FoldOut.boxColor, offsetY: -2, extraHeight: 3);
                                        }
                                        charge.Field("Projectile", "projectile");
                                        charge.FieldDouble("Charge Time", "chargeMaxTime", "chargeMinTime");
                                        Labels.FieldDoubleText("Max", "Min");
                                        charge.Field("Discharge Time", "dischargeTime");
                                        charge.Field("Cooldown Time", "coolDownTime");
                                        animation.FieldAndEnable("Shoot Animation", "weaponAnimation", "wait");
                                        Labels.FieldText("Signal Name", rightSpacing: Layout.boolWidth, execute: animation.String("weaponAnimation") == "");
                                        if (extra > 0)
                                                animation.FieldDouble("Extra Animation", "spriteEngine", "extraSignal");

                                        if (FoldOut.FoldOutButton(charge.Get("eventsFoldOut"), offsetY: -2))
                                        {
                                                Fields.EventFoldOut(charge.Get("onCharging"), charge.Get("onChargingFoldOut"), "On Charging", color: eventColor);
                                                Fields.EventFoldOut(charge.Get("onChargingBegin"), charge.Get("onChargingBeginFoldOut"), "On Charging Begin", color: eventColor);
                                                Fields.EventFoldOut(charge.Get("onChargingComplete"), charge.Get("onChargingCompleteFoldOut"), "On Charging Complete", color: eventColor);
                                                Fields.EventFoldOut(charge.Get("onDischarging"), charge.Get("onDischargingFoldOut"), "On Discharging", color: eventColor);
                                                Fields.EventFoldOut(charge.Get("onDischargingComplete"), charge.Get("onDischargingCompleteFoldOut"), "On Discharging Complete", color: eventColor);
                                                Fields.EventFoldOut(charge.Get("onDischargingFailed"), charge.Get("onDischargingFailedFoldOut"), "On Discharging Failed", color: eventColor);
                                                Fields.EventFoldOut(charge.Get("onCoolingDown"), charge.Get("onCoolDownFoldOut"), "On Cooling Down", color: eventColor);
                                        }
                                        Recoil("Recoil", charge.Get("recoil"), charge, extraHeight: 1);
                                }
                                GUI.enabled = true;
                        }
                }

                public void AutoSeek (SerializedProperty parent)
                {
                        SerializedProperty autoSeek = parent.Get("autoSeek");
                        FoldOut.Box(4, FoldOut.boxColor, extraHeight: 3);
                        {
                                autoSeek.Field("Target Layer", "layer");
                                autoSeek.Field("Search Radius", "maxRadius");
                                autoSeek.Field("Search Rate", "searchRate");
                                autoSeek.FieldToggle("Auto Shoot", "autoShoot");
                        }
                        if (FoldOut.FoldOutButton(autoSeek.Get("eventsFoldOut")))
                        {
                                Fields.EventFoldOut(autoSeek.Get("onFoundTarget"), autoSeek.Get("onFoundTargetFoldOut"), "On Found New Target", color: eventColor);
                        }
                }

                public void Recoil (string title, SerializedProperty recoil, SerializedProperty discharge, int extraHeight = 0)
                {
                        if (FoldOut.Bar(recoil, FoldOut.boxColor * Tint.WarmGrey).Label("Recoil", FoldOut.titleColor, bold: false).BRE("recoil").FoldOut())
                        {
                                FoldOut.Box(5 + extraHeight, FoldOut.boxColor, offsetY: -2);
                                GUI.enabled = GUI.enabled && recoil.Bool("recoil");
                                {
                                        recoil.Field("Type", "type");
                                        if (discharge != null)
                                                discharge.Field("On Discharge", "recoilOnDischarge");
                                        recoil.Field("Recoil When", "condition");
                                        recoil.Field("Distance", "recoilDistance");
                                        recoil.Field("Time", "recoilTime");
                                        recoil.FieldToggle("Has Gravity", "hasGravity");
                                }
                                GUI.enabled = true;
                                Layout.VerticalSpacing(3);
                        }
                }

        }
}
