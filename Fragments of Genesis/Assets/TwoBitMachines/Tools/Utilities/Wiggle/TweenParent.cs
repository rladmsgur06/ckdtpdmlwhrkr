using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines
{
        [System.Serializable]
        public class TweenParent
        {
                [SerializeField] public GameObject obj;
                [SerializeField] public bool active = false;
                [SerializeField] public bool useAnchors = false;
                [SerializeField] public Transform transform;
                [SerializeField] public RectTransform rectTransform;
                [SerializeField] public List<TweenChild> children = new List<TweenChild> ( );

                [SerializeField] private int loopLimit = 0;
                [SerializeField] private int loopCount = 0;
                [SerializeField] private bool deactivate = false;
                [SerializeField] private bool standAlone = false;
                [SerializeField] private bool unscaledTime = false;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField] private bool activeR = false;
                [SerializeField] private bool foldOut = false;
                [SerializeField] private bool add = false;
                [SerializeField] private int chosenIndex = -1;
                [SerializeField] private int signalIndex = 0;
                [SerializeField] private string name;
                #pragma warning restore 0414
                #endif
                #endregion

                public void Reset (GameObject obj)
                {
                        this.obj = obj;
                        this.loopLimit = 0;
                        this.loopCount = 0;
                        this.active = true;
                        this.deactivate = false;
                        this.standAlone = false;
                        this.useAnchors = false;
                        this.transform = obj.transform;
                        this.rectTransform = obj.GetComponent<RectTransform> ( );
                        if (!obj.activeInHierarchy) obj.SetActive (true);
                        if (children.Count > 0) Wiggle.TransferChildren (children);
                }

                public void StandAloneReset ( )
                {
                        if (obj == null) return;
                        if (!obj.activeInHierarchy) obj.SetActive (true);
                        loopCount = 0;
                        active = true;
                        ReactivateChildren ( );
                }

                public void DeactivateImmediately ( )
                {
                        active = false;
                }

                public bool Run ( )
                {
                        if (!active) return true;

                        bool stillActive = false;
                        bool foundParallel = false;

                        for (int i = 0; i < children.Count; i++)
                        {
                                TweenChild child = children[i];
                                if (child.active)
                                {
                                        if (!stillActive)
                                        {
                                                child.Run (transform, rectTransform, useAnchors, unscaledTime);
                                                foundParallel = child.parallel;
                                                stillActive = true;
                                                continue;
                                        }
                                        if ((foundParallel || stillActive) && !child.parallel)
                                        {
                                                break;
                                        }
                                        if (foundParallel && child.parallel)
                                        {
                                                child.Run (transform, rectTransform, useAnchors, unscaledTime);
                                        }
                                }
                        }

                        if (!stillActive)
                        {
                                if ((loopLimit > 0 && ++loopCount < loopLimit) || loopLimit == int.MaxValue)
                                {
                                        ReactivateChildren ( );
                                }
                                else
                                {
                                        active = false;
                                        if (!standAlone) Wiggle.Remove (this);
                                        if (deactivate) obj.SetActive (false);
                                        return true;
                                }
                        }
                        return false;
                }

                private TweenParent Add (float target, float time, float speed, Tween tween, Act act, Axis axis, Interpolate interpolate = Interpolate.Float)
                {
                        TweenChild tweenChild = Wiggle.GetTweenChild ( );
                        tweenChild.Set (Vector3.right * target, time <= 0 ? 1f : time, speed, act, axis, tween, interpolate);
                        children.Add (tweenChild);
                        return this;
                }

                private TweenParent Add (Vector3 target, float time, float speed, Tween tween, Act act, Axis axis, Interpolate interpolate = Interpolate.Float)
                {
                        TweenChild tweenChild = Wiggle.GetTweenChild ( );
                        tweenChild.Set (target, time <= 0 ? 1f : time, speed, act, axis, tween, interpolate);
                        children.Add (tweenChild);
                        return this;
                }

                private void ReactivateChildren ( )
                {
                        for (int i = 0; i < children.Count; i++)
                        {
                                children[i].Reactivate ( );
                        }
                }

                #region move
                public TweenParent MoveX (float move, float time, Tween tween)
                {
                        return Add (move, time, 0, tween, Act.MoveX, Axis.One);
                }

                public TweenParent MoveY (float move, float time, Tween tween)
                {
                        return Add (move, time, 0, tween, Act.MoveY, Axis.One);
                }

                public TweenParent MoveZ (float move, float time, Tween tween)
                {
                        return Add (move, time, 0, tween, Act.MoveZ, Axis.One);
                }

                public TweenParent MoveToX (float moveTo, float time, Tween tween)
                {
                        return Add (moveTo, time, 0, tween, Act.MoveToX, Axis.One);
                }

                public TweenParent MoveToY (float moveTo, float time, Tween tween)
                {
                        return Add (moveTo, time, 0, tween, Act.MoveToY, Axis.One);
                }

                public TweenParent MoveToZ (float moveTo, float time, Tween tween)
                {
                        return Add (moveTo, time, 0, tween, Act.MoveToZ, Axis.One);
                }

                public TweenParent Move2D (Vector2 move, float time, Tween tween)
                {
                        return Add (move, time, 0, tween, Act.Move2D, Axis.Two);
                }

                public TweenParent Move3D (Vector3 move, float time, Tween tween)
                {
                        return Add (move, time, 0, tween, Act.Move3D, Axis.Three);
                }

                public TweenParent MoveTo2D (Vector2 moveTo, float time, Tween tween)
                {
                        return Add (moveTo, time, 0, tween, Act.MoveTo2D, Axis.Two);
                }

                public TweenParent MoveTo3D (Vector3 moveTo, float time, Tween tween)
                {
                        return Add (moveTo, time, 0, tween, Act.MoveTo3D, Axis.Three);
                }
                #endregion

                #region scale
                public TweenParent ScaleToX (float scaleTo, float time, Tween tween)
                {
                        return Add (scaleTo, time, 0, tween, Act.ScaleToX, Axis.One);
                }

                public TweenParent ScaleToY (float scaleTo, float time, Tween tween)
                {
                        return Add (scaleTo, time, 0, tween, Act.ScaleToY, Axis.One);
                }

                public TweenParent ScaleToZ (float scaleTo, float time, Tween tween)
                {
                        return Add (scaleTo, time, 0, tween, Act.ScaleToZ, Axis.One);
                }

                public TweenParent ScaleTo2D (Vector2 scaleTo, float time, Tween tween)
                {
                        return Add (scaleTo, time, 0, tween, Act.ScaleTo2D, Axis.Two);
                }

                public TweenParent ScaleTo3D (Vector3 scaleTo, float time, Tween tween)
                {
                        return Add (scaleTo, time, 0, tween, Act.ScaleTo3D, Axis.Three);
                }
                #endregion

                #region rotate
                public TweenParent RotateX (float turnsPerSecond, float seconds)
                {
                        return Add (0, seconds, turnsPerSecond, Tween.Linear, Act.RotateX, Axis.One, Interpolate.Rotation);
                }

                public TweenParent RotateY (float turnsPerSecond, float seconds)
                {
                        return Add (0, seconds, turnsPerSecond, Tween.Linear, Act.RotateY, Axis.One, Interpolate.Rotation);
                }

                public TweenParent RotateZ (float turnsPerSecond, float seconds)
                {
                        return Add (0, seconds, turnsPerSecond, Tween.Linear, Act.RotateZ, Axis.One, Interpolate.Rotation);
                }

                public TweenParent RotateAroundX (Vector3 center, float turnsPerSecond, float seconds)
                {
                        return Add (center, seconds, turnsPerSecond, Tween.Linear, Act.RotateAroundX, Axis.One, Interpolate.RotateAround);
                }

                public TweenParent RotateAroundY (Vector3 center, float turnsPerSecond, float seconds)
                {
                        return Add (center, seconds, turnsPerSecond, Tween.Linear, Act.RotateAroundY, Axis.One, Interpolate.RotateAround);
                }

                public TweenParent RotateAroundZ (Vector3 center, float turnsPerSecond, float seconds)
                {
                        return Add (center, seconds, turnsPerSecond, Tween.Linear, Act.RotateAroundZ, Axis.One, Interpolate.RotateAround);
                }

                public TweenParent RotateTo (Vector3 rotateTo, float time, Tween tween)
                {
                        return Add (rotateTo, time, 0, tween, Act.RotateTo, Axis.Three, Interpolate.Quaternion);
                }
                public TweenParent Wait (float time)
                {
                        return Add (0, time, 0, 0, Act.Wait, Axis.One, Interpolate.None);
                }
                #endregion

                #region Extras
                public TweenParent Loop (int limit = int.MaxValue)
                {
                        if (children.Count > 0) children[children.Count - 1].loopLimit = limit;
                        return this;
                }

                public TweenParent LoopAll (int limit = int.MaxValue)
                {
                        loopLimit = limit;
                        return this;
                }

                public TweenParent Deactivate ( )
                {
                        deactivate = true;
                        return this;
                }

                public TweenParent Delay (float value)
                {
                        if (children.Count > 0) children[children.Count - 1].delay = value;
                        return this;
                }

                public TweenParent IsParallel ( )
                {
                        if (children.Count > 0) children[children.Count - 1].parallel = true;
                        return this;
                }

                public TweenParent UnscaledTime (bool value)
                {
                        unscaledTime = value;
                        return this;
                }

                public TweenParent OnComplete (UnityAction onComplete)
                {
                        if (children.Count > 0) children[children.Count - 1].onComplete.AddListener (onComplete);
                        return this;
                }

                public TweenParent StartPosition (Vector3 position)
                {
                        if (rectTransform != null)
                        {
                                rectTransform.anchoredPosition = position;
                        }
                        else
                        {
                                transform.localPosition = position;
                        }
                        return this;
                }

                public TweenParent StartRotation (Vector3 angle)
                {
                        if (rectTransform != null)
                        {
                                rectTransform.localEulerAngles = angle;
                        }
                        else
                        {
                                transform.localEulerAngles = angle;
                        }
                        return this;
                }

                public TweenParent StartScale (Vector3 scale)
                {
                        if (rectTransform != null)
                        {
                                rectTransform.localScale = scale;
                        }
                        else
                        {
                                transform.localScale = scale;
                        }
                        return this;
                }
                #endregion
        }
}