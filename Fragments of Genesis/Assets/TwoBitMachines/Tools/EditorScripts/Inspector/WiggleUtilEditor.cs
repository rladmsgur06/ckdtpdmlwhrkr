#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public class WiggleUtilEditor
        {
                private static Color color;
                public static Color newColor;
                public static bool overrideColor;

                public static void TweenType (SerializedProperty child, string name, bool standAlone = false)
                {
                        Act act = (Act) child.Get("act").enumValueIndex;
                        int height = standAlone ? 1 : 0;

                        color = overrideColor ? newColor : FoldOut.boxColor;
                        overrideColor = false;

                        if (act == Act.Wait)
                        {
                                FoldOut.Box(1 + height, color, offsetY: -2, extraHeight: height == 1 ? 0 : 5);
                                if (standAlone)
                                        child.Field("Type", "act");
                                child.Field("Time", "time");
                                InitializeSettings(child, Axis.One, Interpolate.Float);

                                if (standAlone)
                                {
                                        Layout.VerticalSpacing(3);
                                }
                                else if (FoldOut.FoldOutButton(child.Get("settingsFoldOut")))
                                {
                                        ExtraSettings(child);
                                }
                        }
                        else if (act == Act.MoveX || act == Act.MoveToX || act == Act.ScaleToX)
                        {
                                FoldOut.Box(3 + height, color, offsetY: -2, extraHeight: height == 1 ? 0 : 5);
                                if (standAlone)
                                        child.Field("Type", "act");
                                Target(child, name);
                                child.Field("Time", "time");
                                child.Field("Tween", "tween");
                                InitializeSettings(child, Axis.One, Interpolate.Float);

                                if (standAlone)
                                {
                                        Layout.VerticalSpacing(3);
                                }
                                else if (FoldOut.FoldOutButton(child.Get("settingsFoldOut")))
                                {
                                        ExtraSettings(child);
                                }
                        }
                        else if (act == Act.MoveY || act == Act.MoveToY || act == Act.ScaleToY)
                        {
                                FoldOut.Box(3 + height, color, offsetY: -2, extraHeight: height == 1 ? 0 : 5);
                                if (standAlone)
                                        child.Field("Type", "act");
                                Target(child, name);
                                child.Field("Time", "time");
                                child.Field("Tween", "tween");
                                InitializeSettings(child, Axis.One, Interpolate.Float);

                                if (standAlone)
                                {
                                        Layout.VerticalSpacing(3);
                                }
                                else if (FoldOut.FoldOutButton(child.Get("settingsFoldOut")))
                                {
                                        ExtraSettings(child);
                                }
                        }
                        else if (act == Act.MoveZ || act == Act.MoveToZ || act == Act.ScaleToZ)
                        {
                                FoldOut.Box(3 + height, color, offsetY: -2, extraHeight: height == 1 ? 0 : 5);
                                if (standAlone)
                                        child.Field("Type", "act");
                                Target(child, name);
                                child.Field("Time", "time");
                                child.Field("Tween", "tween");
                                InitializeSettings(child, Axis.One, Interpolate.Float);

                                if (standAlone)
                                {
                                        Layout.VerticalSpacing(3);
                                }
                                else if (FoldOut.FoldOutButton(child.Get("settingsFoldOut")))
                                {
                                        ExtraSettings(child);
                                }
                        }
                        else if (act == Act.Move2D || act == Act.MoveTo2D || act == Act.ScaleTo2D)
                        {
                                FoldOut.Box(3 + height, color, offsetY: -2, extraHeight: height == 1 ? 0 : 5);
                                if (standAlone)
                                        child.Field("Type", "act");
                                Target2D(child, name);
                                child.Field("Time", "time");
                                child.Field("Tween", "tween");
                                InitializeSettings(child, Axis.Two, Interpolate.Float);

                                if (standAlone)
                                {
                                        Layout.VerticalSpacing(3);
                                }
                                else if (FoldOut.FoldOutButton(child.Get("settingsFoldOut")))
                                {
                                        ExtraSettings(child);
                                }
                        }
                        else if (act == Act.Move3D || act == Act.MoveTo3D || act == Act.ScaleTo3D)
                        {
                                FoldOut.Box(3 + height, color, offsetY: -2, extraHeight: height == 1 ? 0 : 5);
                                if (standAlone)
                                        child.Field("Type", "act");
                                child.Field(name, "target");
                                child.Field("Time", "time");
                                child.Field("Tween", "tween");
                                InitializeSettings(child, Axis.Three, Interpolate.Float);

                                if (standAlone)
                                {
                                        Layout.VerticalSpacing(3);
                                }
                                else if (FoldOut.FoldOutButton(child.Get("settingsFoldOut")))
                                {
                                        ExtraSettings(child);
                                }
                        }
                        else if (act == Act.RotateX || act == Act.RotateY || act == Act.RotateZ)
                        {
                                FoldOut.Box(2 + height, color, offsetY: -2, extraHeight: height == 1 ? 0 : 5);
                                if (standAlone)
                                        child.Field("Type", "act");
                                child.Get("target").vector3Value = Vector3.zero;
                                child.Get("tween").enumValueIndex = (int) Tween.Linear;
                                child.Field("Seconds", "time");
                                child.Field("Turns Per Second", "speed");
                                InitializeSettings(child, Axis.One, Interpolate.Rotation);

                                if (standAlone)
                                {
                                        Layout.VerticalSpacing(3);
                                }
                                else if (FoldOut.FoldOutButton(child.Get("settingsFoldOut")))
                                {
                                        ExtraSettings(child);
                                }
                        }
                        else if (act == Act.RotateAroundX || act == Act.RotateAroundY || act == Act.RotateAroundZ)
                        {
                                FoldOut.Box(3 + height, color, offsetY: -2, extraHeight: height == 1 ? 0 : 5);
                                if (standAlone)
                                        child.Field("Type", "act");
                                child.Field("Center", "target");
                                child.Field("Seconds", "time");
                                child.Field("Turns Per Second", "speed");
                                InitializeSettings(child, Axis.One, Interpolate.RotateAround);

                                if (standAlone)
                                {
                                        Layout.VerticalSpacing(3);
                                }
                                else if (FoldOut.FoldOutButton(child.Get("settingsFoldOut")))
                                {
                                        ExtraSettings(child);
                                }
                        }
                        else if (act == Act.RotateTo)
                        {
                                FoldOut.Box(3 + height, color, offsetY: -2, extraHeight: height == 1 ? 0 : 5);
                                if (standAlone)
                                        child.Field("Type", "act");
                                child.Field("Rotate To", "target");
                                child.Field("Seconds", "time");
                                child.Field("Tween", "tween");
                                child.Get("speed").floatValue = 0;
                                InitializeSettings(child, Axis.Three, Interpolate.Quaternion);

                                if (standAlone)
                                {
                                        Layout.VerticalSpacing(3);
                                }
                                else if (FoldOut.FoldOutButton(child.Get("settingsFoldOut")))
                                {
                                        ExtraSettings(child);
                                }
                        }
                }

                public static void ExtraSettings (SerializedProperty child)
                {
                        FoldOut.Box(3, color);
                        {
                                child.Field("Delay", "delay");
                                child.Field("Loop", "loopLimit");
                                child.Field("Parallel", "parallel");
                        }
                        Layout.VerticalSpacing(3);
                        Fields.EventFoldOut(child.Get("onComplete"), child.Get("onCompleteFoldOut"), "On Complete");
                }

                public static void InitializeSettings (SerializedProperty child, Axis axis, Interpolate interpolate)
                {
                        if (Application.isPlaying)
                                return;
                        child.SetTrue("active");
                        child.SetTrue("init");
                        child.Get("axis").enumValueIndex = (int) axis;
                        child.Get("interpolate").enumValueIndex = (int) interpolate;
                }

                public static void Target (SerializedProperty child, string name)
                {
                        child.Get("placeHolder").floatValue = child.Get("target").vector3Value.x;
                        child.Field(name, "placeHolder");
                        child.Get("target").vector3Value = new Vector3(child.Get("placeHolder").floatValue, 0, 0);
                }

                public static void Target2D (SerializedProperty child, string name)
                {
                        child.Get("placeHolderV2").vector2Value = child.Get("target").vector3Value;
                        child.Field(name, "placeHolderV2");
                        child.Get("target").vector3Value = (Vector3) child.Get("placeHolderV2").vector2Value;
                }
        }
}
#endif
