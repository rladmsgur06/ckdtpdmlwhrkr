using System;
using UnityEngine;

namespace TwoBitMachines
{
        public static class Clock
        {
                public static float timeNow { get; private set; }
                public static float unscaledDeltaTime { get; private set; }

                public static void Initialize ()
                {
                        timeNow = Time.realtimeSinceStartup;
                        SimulateTimeEditor();
                }

                public static float TimeCount (ref float counter, float limit = 1f)
                {
                        return counter = Mathf.Clamp(counter + Time.deltaTime, 0, limit);
                        ;
                }

                public static bool Timer (ref float value, float duration)
                {
                        value += Time.deltaTime;
                        if (value >= duration)
                        {
                                value = 0;
                                return true;
                        }
                        return false;
                }

                public static bool Timer (ref float value, float duration, float deltaTime)
                {
                        value += deltaTime;
                        if (value >= duration)
                        {
                                value = 0;
                                return true;
                        }
                        return false;
                }

                public static bool Timer (ref float value, float duration, bool timeScale)
                {
                        value += timeScale ? Time.deltaTime : Time.unscaledDeltaTime;
                        if (value >= duration)
                        {
                                value = 0;
                                return true;
                        }
                        return false;
                }

                public static bool TimerExpired (ref float value, float duration)
                {
                        if (value >= duration)
                        {
                                value = duration;
                                return true;
                        }
                        value = Mathf.Clamp(value + Time.deltaTime, 0, duration);
                        return false;
                }

                public static bool TimerExpiredUnscaled (ref float value, float duration)
                {
                        if (value >= duration)
                        {
                                value = duration;
                                return true;
                        }
                        value = Mathf.Clamp(value + Time.unscaledDeltaTime, 0, duration);
                        return false;
                }

                public static bool TimerInverse (ref float value, float duration)
                {
                        if (value >= duration)
                        {
                                value = 0;
                                return false;
                        }
                        value = Mathf.Clamp(value + Time.deltaTime, 0, duration);
                        return true;
                }

                public static bool TimerReverse (ref float value, float limit)
                {
                        if (value <= limit)
                        {
                                value = limit;
                                return true;
                        }
                        value = Mathf.Max(value - Time.deltaTime, limit);
                        return false;
                }

                public static bool TimerInverseExpired (ref float value, float duration)
                {
                        if (value >= duration)
                        {
                                value = duration;
                                return false;
                        }
                        value = Mathf.Clamp(value + Time.deltaTime, 0, duration);
                        return true;
                }

                public static bool TimerInverseExpiredUnscaled (ref float value, float duration)
                {
                        if (value >= duration)
                        {
                                value = duration;
                                return false;
                        }
                        value = Mathf.Clamp(value + Time.unscaledDeltaTime, 0, duration);
                        return true;
                }

                public static bool TimerEditor (ref float value, float duration)
                {
                        SimulateTimeEditor();
                        value += unscaledDeltaTime; // unscaled time can have wonky numbers, use custom delta
                        if (value >= duration)
                        {
                                value = 0;
                                return true;
                        }
                        return false;
                }

                public static bool TimerEditorExpired (ref float value, float duration)
                {
                        if (value >= duration)
                        {
                                value = duration;
                                return true;
                        }
                        value = Mathf.Clamp(value + unscaledDeltaTime, 0, duration);
                        return false;
                }

                public static void SimulateTimeEditor () /// run every frame within the Update cycle
                {
                        float timeSinceStartUp = Time.realtimeSinceStartup;
                        unscaledDeltaTime = timeSinceStartUp - timeNow;
                        timeNow = timeSinceStartUp;
                }
        }

}
