using UnityEngine;
using System;

namespace TwoBitMachines.FlareEngine.Timeline
{
        public class ChronoInterpolate
        {
                public static System.Object Run (System.Object dataStart , System.Object dataEnd , float time , float duration , Tween tween)
                {
                        if (dataStart == null || dataEnd == null)
                        {
                                return dataEnd;
                        }

                        var type = dataStart.GetType();

                        if (type == typeof(float))
                        {
                                return Mathf.Lerp((float) dataStart , (float) dataEnd , EasingFunction.Run(tween , time / duration));
                        }
                        else if (type == typeof(Color))
                        {
                                return Color.Lerp((Color) dataStart , (Color) dataEnd , EasingFunction.Run(tween , time / duration));
                        }
                        else if (type == typeof(Color32))
                        {
                                return Color32.Lerp((Color32) dataStart , (Color32) dataEnd , EasingFunction.Run(tween , time / duration));
                        }
                        else if (type == typeof(Vector2))
                        {
                                return Vector2.Lerp((Vector2) dataStart , (Vector2) dataEnd , EasingFunction.Run(tween , time / duration));
                        }
                        else if (type == typeof(Vector3))
                        {
                                return Vector3.Lerp((Vector3) dataStart , (Vector3) dataEnd , EasingFunction.Run(tween , time / duration));
                        }
                        return dataEnd;
                }
        }
}
