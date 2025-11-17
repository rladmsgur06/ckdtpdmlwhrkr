using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu ("")]
        public abstract class Composite : Node
        {
                [SerializeField] public InterruptType canInterrupt;
                [SerializeField] public OnInterruptType onInterrupt;
                [SerializeField] public List<Node> children = new List<Node> ( );
                [SerializeField] public string defaultSignal = "Signal";
                [SerializeField] public bool useSignal = false;
                [HideInInspector] public int currentChildIndex = 0;

                public bool canInterruptSelf => canInterrupt == InterruptType.ThisNode || canInterrupt == InterruptType.ThisAndLowerPriorityNodes;
                public bool canInterruptLowerNodes => canInterrupt == InterruptType.LowerPriorityNodes || canInterrupt == InterruptType.ThisAndLowerPriorityNodes;

                public void InterruptCheck (Root root)
                {
                        for (int i = 0; i < currentChildIndex; i++)
                        {
                                if (!children[i].isInterruptType) continue;
                                Composite composite = children[i] as Composite;
                                if (composite.canInterruptLowerNodes && composite.InterruptLogic (root, selfAbort : false))
                                {
                                        root.foundInterrupt = root.interruptIndex;
                                        break;
                                }
                        }
                        if (currentChildIndex > 0 && canInterruptSelf && !InterruptLogic (root, selfAbort : true)) // interrupt on condition failure, not success
                        {
                                Reset ( );
                        }
                }

                public virtual bool InterruptLogic (Root root, bool selfAbort = false)
                {
                        // for a sequence (AND), we only check the first node since the first node is either 
                        // conditional (allowing entry into the sequence) or another composite (which might contain other conditionals)

                        if (children.Count == 0) return selfAbort ? true : false; // failed, not a true fail, for self aborts we return true, since we are looking for a false to entre the state

                        Node child = children[0];

                        #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                        #if UNITY_EDITOR
                        child.interruptCheck = true;
                        child.interruptCounter = 0;
                        interruptCheck = true;
                        interruptCounter = 0;
                        #endif
                        #endregion

                        if (child is Conditional)
                        {
                                if (child.RunNodeLogic (root) == NodeState.Success) return true;
                        }
                        else if (child.isInterruptType) // composite 
                        {
                                if ((child as Composite).InterruptLogic (root, selfAbort)) return true;
                        }
                        else if (selfAbort) // if  not a conditional or interrupt, we encountered an action thus we exit if in self abort
                        {
                                return true; // failed, not a true fail
                        }
                        return false; //      a true fail
                }

                public override void OnReset (bool skip = false, bool enteredState = false)
                {
                        currentChildIndex = 0;
                        for (int i = 0; i < children.Count; i++)
                        {
                                children[i].Reset ( );
                        }
                }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                public override List<Node> Children ( ) { return children; }
                #pragma warning restore 0414
                #endif
                #endregion
        }

        public enum InterruptType
        {
                None,
                ThisNode,
                LowerPriorityNodes,
                ThisAndLowerPriorityNodes
        }

        public enum OnInterruptType
        {
                TerminateImmediately,
                CancelInterruptAndComplete
        }
}