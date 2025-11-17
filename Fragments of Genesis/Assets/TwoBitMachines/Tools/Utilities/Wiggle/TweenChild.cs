using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines
{
        [System.Serializable]
        public class TweenChild
        {
                [SerializeField] public bool parallel;
                [SerializeField] public bool active;
                [SerializeField] public bool init;

                [SerializeField] public float delay;
                [SerializeField] public float speed;
                [SerializeField] public float time = 1f;

                [SerializeField] public float memory;
                [SerializeField] public float counter;
                [SerializeField] public float delayCounter;

                [SerializeField] public int loopLimit;
                [SerializeField] public int loopCount;

                [SerializeField] public Vector3 from;
                [SerializeField] public Vector3 target;
                [SerializeField] public Vector3 tempTarget;

                [SerializeField] public Act act;
                [SerializeField] public Axis axis;
                [SerializeField] public Tween tween;
                [SerializeField] public Interpolate interpolate;
                [SerializeField] public UnityEvent onComplete = new UnityEvent();

                [SerializeField] public AnimationCurve curve;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] private bool foldOut = true;
                [SerializeField] private bool delete;
                [SerializeField] private bool settingsFoldOut;
                [SerializeField] private bool onCompleteFoldOut;
                [SerializeField] private float placeHolder;
                [SerializeField] private Vector2 placeHolderV2;
#pragma warning restore 0414
#endif
                #endregion

                public void Reactivate (bool resetAll = true)
                {
                        loopCount = resetAll ? 0 : loopCount;
                        active = true;
                        init = true;
                }

                public void Set (Vector3 target , float time , float speed , Act act , Axis axis , Tween tween , Interpolate interpolate)
                {
                        this.parallel = false;
                        this.active = true;
                        this.init = true;
                        this.loopLimit = 0;
                        this.loopCount = 0;
                        this.delay = 0;
                        this.act = act;
                        this.axis = axis;
                        this.time = time;
                        this.speed = speed;
                        this.tween = tween;
                        this.target = target;
                        this.interpolate = interpolate;
                        this.onComplete.RemoveAllListeners();
                }

                public void Run (Transform T , RectTransform rectT , bool useAnchors , bool unscaledTime)
                {
                        if (init)
                        {
                                init = false;
                                counter = 0;
                                memory = 0;
                                delayCounter = 0;
                                tempTarget = target;
                                SetTween.Get(act , ref from , ref tempTarget , T , rectT , useAnchors);
                        }

                        if (delay > 0 && Clock.TimerInverseExpired(ref delayCounter , delay))
                        {
                                return;
                        }
                        float deltaTime = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                        counter += deltaTime;
                        InterpolateTween(deltaTime , EasingFunction.Run(tween , counter / time) , tempTarget , T , rectT , useAnchors);

                        if (counter >= time)
                        {
                                if ((loopLimit > 0 && ++loopCount < loopLimit) || loopLimit == int.MaxValue)
                                {
                                        Reactivate(false);
                                }
                                else
                                {
                                        active = false;
                                        onComplete.Invoke();
                                }
                        }

                }

                public void InterpolateTween (float deltaTime , float easing , Vector3 target , Transform T , RectTransform rectT , bool useAnchors)
                {
                        if (interpolate == Interpolate.Float)
                        {
                                if (axis == Axis.One)
                                {
                                        SetTween.Set(act , Mathf.LerpUnclamped(from.x , target.x , easing) , T , rectT , useAnchors); // x can represent x,y, or z, jus a placeholder
                                }
                                else if (axis == Axis.Two)
                                {
                                        SetTween.Set(act , Vector2.LerpUnclamped(from , target , easing) , T , rectT , useAnchors);
                                }
                                else if (axis == Axis.Three)
                                {
                                        SetTween.Set(act , Vector3.LerpUnclamped(from , target , easing) , T , rectT , useAnchors);
                                }
                        }
                        else if (interpolate == Interpolate.Rotation)
                        {
                                SetTween.Rotate(act , ref memory , deltaTime , speed , time , T , rectT); // speed = turnsPerSecond
                        }
                        else if (interpolate == Interpolate.RotateAround)
                        {
                                SetTween.RotateAround(act , target , ref memory , deltaTime , speed , time , T , rectT);
                        }
                        else if (interpolate == Interpolate.Quaternion)
                        {
                                SetTween.SetQuaternion(act , Quaternion.LerpUnclamped(Quaternion.Euler(from) , Quaternion.Euler(target) , easing) , T , rectT);
                        }
                }
        }

        public enum Interpolate
        {
                Float,
                Rotation,
                RotateAround,
                Quaternion,
                Color,
                None
        }

        public enum Axis
        {
                One,
                Two,
                Three
        }

        public enum Act
        {
                Move2D,
                Move3D,
                MoveX,
                MoveY,
                MoveZ,
                MoveTo2D,
                MoveTo3D,
                MoveToX,
                MoveToY,
                MoveToZ,
                ScaleTo2D,
                ScaleTo3D,
                ScaleToX,
                ScaleToY,
                ScaleToZ,
                RotateTo,
                RotateX,
                RotateY,
                RotateZ,
                RotateAroundX,
                RotateAroundY,
                RotateAroundZ,
                Wait
        }
}
