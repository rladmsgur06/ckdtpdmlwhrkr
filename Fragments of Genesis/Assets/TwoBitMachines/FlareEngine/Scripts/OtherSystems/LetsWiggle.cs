using TwoBitMachines.FlareEngine;
using UnityEngine;

namespace TwoBitMachines
{
        [AddComponentMenu ("Flare Engine/LetsWiggle")]
        public class LetsWiggle : ReactionBehaviour
        {
                [SerializeField] private TweenParent tween = new TweenParent ( );
                [SerializeField] private Vector3 startScale;
                [SerializeField] private Vector3 startPosition;
                [SerializeField] private Vector3 startRotation;

                [SerializeField] private bool startOnAwake;
                [SerializeField] private bool useStartScale;
                [SerializeField] private bool useStartPosition;
                [SerializeField] private bool useStartRotation;
                [SerializeField] private bool useOnEnable = true;

                public void Awake ( )
                {
                        if (tween.obj == null)
                        {
                                tween.obj = gameObject;
                                tween.transform = transform;
                        }
                        if (tween.useAnchors && tween.rectTransform == null)
                        {
                                tween.rectTransform = tween.obj.GetComponent<RectTransform> ( );
                        }
                        if (startOnAwake)
                        {
                                Wiggle ( );
                        }
                }

                private void OnEnable ( )
                {
                        if (useOnEnable) Wiggle ( );
                }

                private void OnDisable ( )
                {
                        tween.DeactivateImmediately ( );
                }

                public void Deactivate ( )
                {
                        tween.DeactivateImmediately ( );
                }

                public void Remove ( )
                {
                        if (TwoBitMachines.Wiggle.temporaryTween.Contains (tween))
                        {
                                TwoBitMachines.Wiggle.temporaryTween.Remove (tween);
                        }
                }

                public void PauseWiggle (bool value)
                {
                        tween.active = value;
                }

                public bool Run ( )
                {
                        tween.Run ( );
                        return tween.active;
                }

                public void WiggleIfActive ( )
                {
                        if (gameObject.activeInHierarchy)
                        {
                                Wiggle ( );
                        }
                }

                public void Wiggle ( )
                {
                        if (tween.rectTransform != null) // set rect transform
                        {
                                if (useStartPosition) tween.rectTransform.anchoredPosition = startPosition;
                                if (useStartRotation) tween.rectTransform.localEulerAngles = startRotation;
                                if (useStartScale) tween.rectTransform.localScale = startScale;
                        }
                        else if (tween.transform != null) // set transform
                        {
                                if (useStartPosition) tween.transform.position = startPosition;
                                if (useStartRotation) tween.transform.localEulerAngles = startRotation;
                                if (useStartScale) tween.transform.localScale = startScale;
                        }
                        if (tween.rectTransform == null && tween.transform == null)
                        {
                                return;
                        }
                        TwoBitMachines.Wiggle.AddTemporaryTween (tween);
                        tween.StandAloneReset ( );
                }

                public override void Activate (ImpactPacket impact)
                {
                        if (impact.transform == null)
                        {
                                return;
                        }
                        tween.obj = impact.transform.gameObject;
                        tween.transform = impact.transform;
                        Wiggle ( );
                }
        }
}