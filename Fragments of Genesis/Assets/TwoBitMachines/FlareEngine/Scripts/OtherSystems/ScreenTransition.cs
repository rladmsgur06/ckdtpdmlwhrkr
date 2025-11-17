using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TwoBitMachines.FlareEngine
{
        public class ScreenTransition : MonoBehaviour
        {
                [SerializeField] private TransitionType type;
                [SerializeField] private float time = 1;
                [SerializeField] private float holdTime = 0.25f;
                [SerializeField] private bool resetGame = false;
                [SerializeField] private UnityEvent onStart;
                [SerializeField] private UnityEvent onComplete;
                [SerializeField] private List<Texture2D> text = new List<Texture2D> ( );

                [System.NonSerialized] private Image image;
                [System.NonSerialized] private State transition;
                [System.NonSerialized] private int skipFrames;
                [System.NonSerialized] private float counterHold;
                [System.NonSerialized] private float counterTransition;

                private enum State { In, Hold, Out }
                public bool canResetGame => resetGame && type == TransitionType.Both;

                private void Start ( )
                {
                        image = gameObject.GetComponentInChildren<Image> (true);
                }
                private void OnEnable ( )
                {
                        Begin ( );
                }

                public void Begin ( )
                {
                        if (!gameObject.activeInHierarchy)
                        {
                                gameObject.SetActive (true);
                        }

                        transition = State.In;
                        counterTransition = 0;
                        counterHold = 0;
                        skipFrames = 0;
                        onStart.Invoke ( );

                        if (image != null && text.Count > 1)
                        {
                                int newTransition = Random.Range (0, text.Count);
                                if (text[newTransition] != null)
                                {
                                        image.material.SetTexture ("_Transition", text[newTransition]);
                                }
                        }
                        if (canResetGame)
                        {
                                Time.timeScale = 0;
                        }
                }
                private void Update ( )
                {
                        if (type == TransitionType.TransitionIn)
                        {
                                TransitionIn ( );
                        }
                        else if (type == TransitionType.TransitionOut)
                        {
                                TransitionOut ( );
                        }
                        else
                        {
                                TransitionBoth ( );
                        }
                }

                private void TransitionIn ( )
                {
                        TransitionLerp (-0.1f, 1.1f);
                }

                private void TransitionOut ( )
                {
                        if (skipFrames++ < 8) // prevents huge lag spikes during opening scene
                        {
                                image.material.SetFloat ("_CutOff", 1.1f);
                                return;
                        }
                        TransitionLerp (1.1f, -0.1f);
                }

                private void TransitionBoth ( )
                {
                        switch (transition)
                        {
                                case State.In:

                                        if (TransitionLerp (-0.1f, 1.1f, false))
                                        {
                                                if (canResetGame)
                                                {
                                                        Time.timeScale = 1f;
                                                        WorldManager.get.ResetAll ( );
                                                }
                                                transition = State.Hold;
                                        }

                                        break;
                                case State.Hold:

                                        if (Clock.Timer (ref counterHold, holdTime))
                                        {
                                                transition = State.Out;
                                        }

                                        break;
                                case State.Out:

                                        TransitionLerp (1.1f, -0.1f);

                                        break;
                        }
                }

                private bool TransitionLerp (float start, float end, bool turnOff = true)
                {
                        float value = Compute.LerpUnscaled (start, end, time, ref counterTransition, out bool complete);
                        if (image != null && image.material != null)
                        {
                                image.material.SetFloat ("_CutOff", value);
                        }
                        if (complete && turnOff)
                        {
                                gameObject.SetActive (false);
                                onComplete.Invoke ( );
                        }
                        return complete;
                }

                #region  ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField, HideInInspector] private bool foldOut = false;
                [SerializeField, HideInInspector] private bool eventFoldOut = false;
                [SerializeField, HideInInspector] private bool startFoldOut = false;
                [SerializeField, HideInInspector] private bool completeFoldOut = false;
                void OnDestroy ( )
                {
                        if (image != null) image.material.SetFloat ("_CutOff", -0.1f);
                }
                #pragma warning restore 0414
                #endif
                #endregion
        }

}