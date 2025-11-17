using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        //* do not change fixed delta time. If using physics set Rigidbody.interpolation to RigidbodyInterpolation.Interpolate.
        public class SlowMotion
        {
                [System.NonSerialized] private bool timeScaleWasPaused = false;
                [System.NonSerialized] private bool constant = false;
                [System.NonSerialized] private float currentTimeScale = 1;

                [System.NonSerialized] private float counter = 0;
                [System.NonSerialized] private float duration = 1f;
                [System.NonSerialized] private float startValue = 1f;

                private const float slowMotionLimit = 0.001f;
                public bool slowMotion { get; private set; }

                public void Initialize ( )
                {
                        currentTimeScale = Time.timeScale;
                }

                public void Set (float scale, float duration, bool constant = false)
                {
                        Time.timeScale = Mathf.Clamp (scale, slowMotionLimit, 0.99f);
                        duration = Mathf.Clamp (duration, 0.001f, Mathf.Abs (duration));

                        this.counter = 0;
                        this.slowMotion = true;
                        this.duration = duration;
                        this.constant = constant;
                        this.startValue = Time.timeScale;
                }

                public void Execute ( )
                {
                        if (Time.timeScale == 0)
                        {
                                timeScaleWasPaused = true;
                                return; // user has paused game, this tool will never set TimeScale = 0
                        }
                        if (timeScaleWasPaused && slowMotion)
                        {
                                Time.timeScale = currentTimeScale;
                        }

                        timeScaleWasPaused = false;
                        currentTimeScale = Time.timeScale;

                        if (Time.timeScale == 1 || constant || !slowMotion)
                        {
                                return;
                        }
                        counter += Time.unscaledDeltaTime;
                        Time.timeScale = Mathf.Lerp (startValue, 1f, counter / duration);
                        if (Time.timeScale >= 1)
                        {
                                Reset ( );
                        }
                }

                public void Reset ( )
                {
                        counter = 0;
                        duration = 1f;
                        startValue = 1f;
                        Time.timeScale = 1;
                        currentTimeScale = 1;
                        timeScaleWasPaused = slowMotion = constant = false;
                }
        }

        public enum TimeScale
        {
                TimeScale,
                TimeUnscaled
        }

}