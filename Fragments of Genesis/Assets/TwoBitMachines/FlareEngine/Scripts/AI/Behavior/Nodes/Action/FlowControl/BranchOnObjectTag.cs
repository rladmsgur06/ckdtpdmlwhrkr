#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class BranchOnObjectTag : Action
        {
                [SerializeField] public List<BranchTo> branch = new List<BranchTo>();
                [SerializeField] public UnityEventEffect onBranch;

                [System.NonSerialized] private AIFSM parent;
                [System.NonSerialized] private Transform id;

                public void Start ()
                {
                        parent = GetComponent<AIFSM>();
                }

                public void ActivateBranch (ImpactPacket packet)
                {
                        if (packet.attacker == null || parent == null)
                                return;

                        string tag = packet.attacker.tag;
                        for (int i = 0; i < branch.Count; i++)
                        {
                                if (branch[i].tag == tag)
                                {
                                        parent.ChangeState(branch[i].state);
                                        onBranch.Invoke(packet);
                                        return;
                                }
                        }
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool eventFoldout;
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(95, "This will read an object's tag and jump to the specified state. Create as many options as necessary. To activate, call its Activate Branch method. This is typically used by the Health component. Only works with AIFSM." +
                                        "\n \nReturns  Success");
                        }

                        SerializedProperty array = parent.Get("branch");
                        if (array.arraySize == 0)
                                array.arraySize++;

                        FoldOut.Box(array.arraySize, color, offsetY: -2);
                        for (int i = 0; i < array.arraySize; i++)
                        {
                                Fields.ArrayPropertyFieldDouble(array, i, "Tag, State", "tag", "state");
                        }
                        Layout.VerticalSpacing(3);
                        Fields.EventFoldOut(parent.Get("onBranch"), parent.Get("eventFoldout"), "On Branch", color: color, offsetY: -2);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        [System.Serializable]
        public class BranchTo
        {
                public string tag = "";
                public string state = "";
        }
}
