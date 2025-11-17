using UnityEngine;

namespace TwoBitMachines
{
        public enum Tween
        {
                EaseIn,
                EaseOut,
                EaseInOut,
                EaseInBack,
                EaseOutBack,
                EaseInOutBack,
                EaseInBounce,
                EaseOutBounce,
                EaseInOutBounce,
                EaseInCubic,
                EaseOutCubic,
                EaseInOutCubic,
                EaseInElastic,
                EaseOutElastic,
                EaseInOutElastic,
                EaseInExpo,
                EaseOutExpo,
                EaseInOutExpo,
                EaseInSine,
                EaseOutSine,
                EaseInOutSine,
                Spike,
                Gravity,
                GravityBounce,
                GravitySingleBounce,
                OnOff,
                Linear,
                Sin
        }

        public static class EasingFunction
        {
                public delegate float TweenFunction (float t);

                public static TweenFunction[] operation = new TweenFunction[]
                {
                        EaseIn,
                        EaseOut,
                        EaseInOut,
                        EaseInBack,
                        EaseOutBack,
                        EaseInOutBack,
                        EaseInBounce,
                        EaseOutBounce,
                        EaseInOutBounce,
                        EaseInCubic,
                        EaseOutCubic,
                        EaseInOutCubic,
                        EaseInElastic,
                        EaseOutElastic,
                        EaseInOutElastic,
                        EaseInExpo,
                        EaseOutExpo,
                        EaseInOutExpo,
                        EaseInSine,
                        EaseOutSine,
                        EaseInOutSine,
                        Spike,
                        Gravity,
                        GravityBounce,
                        GravitySingleBounce,
                        OnOff,
                        Linear,
                        Sin
                };

                public static float Run (Tween tween , float t)
                {
                        return operation[(int) tween](t > 1f ? 1f : t);
                }

                public static float Run (Tween tween , float c , float d) // counter, duration
                {
                        float t = d <= 0 ? 1f : c / d;
                        return operation[(int) tween](t > 1f ? 1f : t);
                }

                public static float EaseIn (float t)
                {
                        return t * t;
                }

                public static float EaseOut (float t)
                {
                        return Flip(EaseIn(Flip(t)));
                }

                public static float EaseInOut (float t)
                {
                        if (t <= 0.5f)
                        {
                                return EaseIn(t * 2) * 0.5f;
                        }
                        else
                        {
                                t -= 0.5f;
                                return (EaseOut(t * 2) * 0.5f) + 0.5f;
                        }
                }

                public static float EaseInBack (float t)
                {
                        float c = 1.70158f;
                        float c2 = c + 1f;

                        return c2 * t * t * t - c * t * t;
                }

                public static float EaseOutBack (float t)
                {
                        float c = 1.70158f;
                        float c2 = c + 1;

                        return 1f + c2 * Mathf.Pow(t - 1f , 3f) + c * Mathf.Pow(t - 1f , 2f);
                }

                public static float EaseInOutBack (float t)
                {
                        float c = 1.70158f;
                        float c2 = c * 1.525f;

                        return t < 0.5f ?
                                (Mathf.Pow(2f * t , 2f) * ((c2 + 1f) * 2f * t - c2)) / 2f :
                                (Mathf.Pow(2f * t - 2f , 2f) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) / 2f;
                }

                public static float EaseInBounce (float t)
                {
                        return 1f - EaseOutBounce(1f - t);
                }

                public static float EaseOutBounce (float t)
                {
                        float n = 7.5625f;
                        float d = 2.75f;

                        if (t < 1f / d)
                        {
                                return n * t * t;
                        }
                        else if (t < 2f / d)
                        {
                                return n * (t -= 1.5f / d) * t + 0.75f;
                        }
                        else if (t < 2.5 / d)
                        {
                                return n * (t -= 2.25f / d) * t + 0.9375f;
                        }
                        else
                        {
                                return n * (t -= 2.625f / d) * t + 0.984375f;
                        }
                }

                public static float EaseInOutBounce (float x)
                {
                        return x < 0.5 ? (1f - EaseOutBounce(1f - 2f * x)) * 0.5f : (1f + EaseOutBounce(2f * x - 1f)) * 0.5f;
                }

                public static float EaseInCubic (float t)
                {
                        return t * t * t;
                }

                public static float EaseOutCubic (float t)
                {
                        return 1f - Mathf.Pow(1f - t , 3f);
                }

                public static float EaseInOutCubic (float t)
                {
                        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f , 3f) * 0.5f;
                }

                public static float EaseInElastic (float t)
                {
                        float c = (2f * Mathf.PI) / 3f;

                        if (t == 0)
                                return 0;
                        if (t == 1f)
                                return 1f;
                        else
                                return -Mathf.Pow(2f , 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * c);
                }

                public static float EaseOutElastic (float t)
                {
                        float c = (2f * Mathf.PI) / 3f;

                        if (t == 0)
                                return 0;
                        if (t == 1f)
                                return 1f;
                        else
                                return Mathf.Pow(2f , -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c) + 1f;
                }

                public static float EaseInOutElastic (float t)
                {
                        float c = (2f * Mathf.PI) / 4.5f;

                        if (t == 0)
                                return 0;
                        if (t == 1)
                                return 1f;
                        if (t < 0.5f)
                                return -(Mathf.Pow(2f , 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * c)) * 0.5f;
                        else
                                return (Mathf.Pow(2f , -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * c)) * 0.5f + 1f;
                }

                public static float EaseInExpo (float t)
                {
                        return t == 0 ? 0 : Mathf.Pow(2f , 10f * t - 10f);
                }

                public static float EaseOutExpo (float t)
                {
                        return t == 1f ? 1f : 1f - Mathf.Pow(2f , -10f * t);
                }

                public static float EaseInOutExpo (float t)
                {
                        if (t == 0)
                                return 0;
                        if (t == 1f)
                                return 1f;
                        if (t < 0.5f)
                                return Mathf.Pow(2f , 20f * t - 10f) * 0.5f;
                        else
                                return (2f - Mathf.Pow(2f , -20f * t + 10f)) * 0.5f;
                }

                public static float EaseInSine (float t)
                {
                        return 1f - Mathf.Cos((t * Mathf.PI) / 2f);
                }

                public static float EaseOutSine (float t)
                {
                        return Mathf.Sin((t * Mathf.PI) / 2f);
                }

                public static float EaseInOutSine (float t)
                {
                        return -(Mathf.Cos(t * Mathf.PI) - 1f) * 0.5f;
                }

                public static float Spike (float t)
                {

                        return t <= 0.5f ? EaseIn(t / 0.5f) : EaseIn(Flip(t) / 0.5f);
                }

                public static float Flip (float t)
                {
                        return 1f - t;
                }

                public static float Linear (float t)
                {
                        return t;
                }

                public static float Gravity (float t)
                {
                        return t <= 0 ? 0 : Mathf.Sin(180f * t * Mathf.Deg2Rad);
                }

                public static float Sin (float t)
                {
                        return t <= 0 ? 0 : Mathf.Sin(360f * t * Mathf.Deg2Rad);
                }

                public static float GravityBounce (float t)
                {
                        float bounce1Time = 0.60f;
                        float bounce2Time = 0.25f;
                        float bounce3Time = 0.15f; // all must add up to 1
                        if (t <= 0)
                                return 0;
                        if (t <= bounce1Time)
                                return Mathf.Sin(180f * Mathf.Deg2Rad * t / bounce1Time);
                        if (t <= (bounce1Time + bounce2Time))
                                return Mathf.Sin(180f * Mathf.Deg2Rad * (t - bounce1Time) / bounce2Time) * 0.35f;
                        else
                                return Mathf.Sin(180f * Mathf.Deg2Rad * (t - (bounce1Time + bounce2Time)) / bounce3Time) * 0.15f;
                }

                public static float GravitySingleBounce (float t)
                {
                        float bounce1Time = 0.75f;
                        float bounce2Time = 0.25f;
                        if (t <= 0)
                                return 0;
                        if (t <= bounce1Time)
                                return Mathf.Sin(180f * Mathf.Deg2Rad * t / bounce1Time);
                        else
                                return Mathf.Sin(180f * Mathf.Deg2Rad * (t - bounce1Time) / bounce2Time) * 0.2f;
                }

                public static float OnOff (float t)
                {
                        return t <= 0.5f ? 0 : 1f;
                }

                public static float None (float t)
                {
                        return 1f;
                }
        }

}
