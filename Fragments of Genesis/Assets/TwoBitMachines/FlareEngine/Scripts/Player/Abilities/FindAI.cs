using System.Collections.Generic;
#region
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("")]
        public class FindAI : Ability
        {
                [SerializeField] public string button;
                [SerializeField] public float time = 2f;
                [SerializeField] public float delay = 0.5f;
                [SerializeField] public float radius = 25f;
                [SerializeField] public string signal;
                [SerializeField] public string newState = "NewState";
                [SerializeField] public UnityEvent onFound;
                [SerializeField] public UnityEvent onFailed;
                [SerializeField] public UnityEvent onSignalComplete;
                [SerializeField] public UnityEvent onDelayComplete;

                [System.NonSerialized] private bool found;
                [System.NonSerialized] private float signalCounter;
                [System.NonSerialized] private float delayCounter;
                [System.NonSerialized] private bool delaySet;
                [System.NonSerialized] private bool signalSet;
                [System.NonSerialized] private Collider2D[] results = new Collider2D[200];
                [System.NonSerialized] private List<AIFSM> aiList = new List<AIFSM>();

                public override void Reset (AbilityManager player)
                {
                        found = false;
                        delaySet = false;
                        signalSet = false;
                        signalCounter = 0;
                        delayCounter = 0;
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        return true;
                }

                public override void EarlyExecute (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                        {
                                return;
                        }
                        if (!found && player.inputs.Pressed(button))
                        {
                                Reset(player);
                                Search();
                        }
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        return !pause && found;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        delayCounter += Time.deltaTime;
                        signalCounter += Time.deltaTime;

                        if (!signalSet)
                        {
                                velocity.x = 0;
                                player.signals.Set(signal);
                                if (signalCounter >= time)
                                {
                                        signalSet = true;
                                        onSignalComplete.Invoke();
                                }
                        }

                        if (!delaySet && delayCounter >= delay)
                        {
                                for (int i = 0; i < aiList.Count; i++)
                                {
                                        aiList[i].ChangeState(newState);
                                }
                                delaySet = true;
                                aiList.Clear();
                                onDelayComplete.Invoke();
                        }

                        if (delaySet && signalSet)
                        {
                                Reset(player);
                        }
                }

                public void Search ()
                {
                        found = false;
                        aiList.Clear();
                        int hit = Physics2D.OverlapCircleNonAlloc(transform.position, radius, results);

                        for (int i = 0; i < hit; i++)
                        {
                                AIFSM ai = results[i].GetComponent<AIFSM>();
                                if (ai != null)
                                {
                                        found = true;
                                        aiList.Add(ai);
                                }
                        }
                        if (found)
                        {
                                onFound.Invoke();
                        }
                        if (!found)
                        {
                                onFailed.Invoke();
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool eventFoldOut;
                [SerializeField, HideInInspector] public bool foundFoldOut;
                [SerializeField, HideInInspector] public bool failedFoldOut;
                [SerializeField, HideInInspector] public bool delayCompleteFoldOut;
                [SerializeField, HideInInspector] public bool signalCompleteFoldOut;

                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Find AI", barColor, labelColor))
                        {
                                FoldOut.Box(4, FoldOut.boxColorLight, extraHeight: 5, offsetY: -2);
                                {
                                        parent.DropDownList(inputList, "Button ", "button");
                                        parent.Field("Search Radius", "radius");
                                        parent.FieldDouble("On Found Signal", "signal", "time");
                                        Labels.FieldDoubleText("Signal", "Time", rightSpacing: 3);
                                        parent.FieldDouble("Change State To", "newState", "delay");
                                        Labels.FieldText("Time Delay", rightSpacing: 3);
                                }

                                if (FoldOut.FoldOutButton(parent.Get("eventFoldOut")))
                                {
                                        Fields.EventFoldOut(parent.Get("onFound"), parent.Get("foundFoldOut"), "On Found", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOut(parent.Get("onFailed"), parent.Get("failedFoldOut"), "On Failed", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOut(parent.Get("onDelayComplete"), parent.Get("delayCompleteFoldOut"), "On Delay Complete", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOut(parent.Get("onSignalComplete"), parent.Get("signalCompleteFoldOut"), "On Signal Complete", color: FoldOut.boxColorLight);
                                }
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
