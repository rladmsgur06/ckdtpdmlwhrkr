#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class CombinationLock : Action
        {
                [SerializeField] public int keyCode;
                [SerializeField] public UnityEvent onFailed;
                [SerializeField] public UnityEvent onSuccess;

                [System.NonSerialized] private bool answerIsTrue = false;
                [System.NonSerialized] private List<int> keys = new List<int>();
                [System.NonSerialized] private List<int> answer = new List<int>();

                private void Start ()
                {
                        GetKeys();
                }

                private void GetKeys ()
                {
                        string numberString = Mathf.Abs(keyCode).ToString();
                        foreach (char digitChar in numberString)
                        {
                                int digit = int.Parse(digitChar.ToString());
                                keys.Add(digit);
                        }
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                answerIsTrue = false;
                                answer.Clear();
                        }
                        return answerIsTrue ? NodeState.Success : NodeState.Running;
                }

                public void EnterKey (int key)
                {
                        answer.Add(key);
                }

                public void ClearAnswer ()
                {
                        answer.Clear();
                }

                public void CheckAnswer (bool clearAnswerOnFail)
                {
                        if (answer.Count == 0 || answer.Count != keys.Count)
                        {
                                onFailed.Invoke();
                                if (clearAnswerOnFail)
                                {
                                        ClearAnswer();
                                }
                                return;
                        }
                        for (var i = 0; i < keys.Count; i++)
                        {
                                if (answer[i] != keys[i])
                                {
                                        onFailed.Invoke();
                                        if (clearAnswerOnFail)
                                        {
                                                ClearAnswer();
                                        }
                                        return;
                                }
                        }
                        answerIsTrue = true;
                        onSuccess.Invoke();
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool eventsFoldOut;
                [SerializeField, HideInInspector] public bool successFoldOut;
                [SerializeField, HideInInspector] public bool failedFoldOut;
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Returns Success if the answer entered matches the key code. Call EnterKey() and CheckAnswer().");
                        }

                        FoldOut.Box(1, color, extraHeight: 5, offsetY: -2);
                        {
                                parent.Field("Key Code", "keyCode");
                        }

                        if (FoldOut.FoldOutButton(parent.Get("eventsFoldOut")))
                        {
                                Fields.EventFoldOut(parent.Get("onSuccess"), parent.Get("successFoldOut"), "On Success", color: color);
                                Fields.EventFoldOut(parent.Get("onFailed"), parent.Get("failedFoldOut"), "On Failed", color: color);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
