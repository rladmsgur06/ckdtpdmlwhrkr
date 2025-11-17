#region 
#if UNITY_EDITOR
using UnityEditor;
#endif
#endregion
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class Node : MonoBehaviour
        {
                [SerializeField] public NodeSetup nodeSetup;
                [SerializeField] public NodeState nodeState;
                [SerializeField] public NodeType nodeType;
                [SerializeField] public Node nodeParent;
                [SerializeField] public bool completed;
                public bool isInterruptType => (this is Composite) && (!(this is Decorator) || this is Inverter);

                public virtual bool InterruptSearch(Root root, List<Node> children)
                {
                        return false;
                }

                public virtual NodeState RunNodeLogic(Root root)
                {
                        return NodeState.Failure;
                }

                public virtual void OnReset(bool skip = false, bool enteredState = false)
                {

                }

                public virtual bool HardReset()
                {
                        return false;
                }

                public NodeState RunChild(Root root)
                {
                        nodeState = RunNodeLogic(root); //                           runs the custom logic
                        nodeSetup = NodeSetup.CurrentlyRunning;
                        if (nodeState != NodeState.Running) //                        reset if not running
                        {
                                Reset(); //                                          reset setup and reset custom logic
                        }
                        else if (root.foundInterrupt > -1 && nodeType == NodeType.Action)
                        {
                                if (nodeParent != null && nodeParent is Composite) // what if it's a decorator?
                                {
                                        if ((nodeParent as Composite).onInterrupt == OnInterruptType.CancelInterruptAndComplete)
                                        {
                                                root.foundInterrupt = -1; // -1 is off
                                        }
                                }
                        }

                        #region
#if UNITY_EDITOR
                        if (nodeState != NodeState.Failure)
                        {
                                active = true;
                                selectCounter = 0;
                        }
                        if (nodeState == NodeState.Failure)
                        {
                                failed = true;
                                failedCounter = 0;
                        }
#endif
                        #endregion
                        return nodeState;
                }

                public void Reset(bool skip = false, bool enteredState = false)
                {
                        nodeSetup = NodeSetup.NeedToInitialize;
                        OnReset(skip, enteredState);
                }

                public virtual Node GetParent() { return nodeParent; }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀ 
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] public string nameType = "Root";
                [SerializeField] public string fsmChild = "";
                [SerializeField] public int fsmIndex = 0;
                [SerializeField] public float selectCounter = 10f;
                [SerializeField] public float failedCounter = 10f;
                [SerializeField] public float interruptCounter = 10f;
                [SerializeField] public Vector2 rectSize;
                [SerializeField] public Vector2 rectPosition;
                [SerializeField] public bool add;
                [SerializeField] public bool active;
                [SerializeField] public bool delete;
                [SerializeField] public bool deleteAsk;
                [SerializeField] public bool showInfo;
                [SerializeField] public bool failed;
                [SerializeField] public bool foldOut;
                [SerializeField] public bool inspect;
                [SerializeField] public bool comments;
                [SerializeField] public bool isDragged;
                [SerializeField] public bool wasDragged;
                [SerializeField] public bool rectActive;
                [SerializeField] public bool copySelected;
                [SerializeField] public bool hideChildren;
                [SerializeField] public bool interruptCheck;
                [SerializeField] public bool inspectSelected;
                [SerializeField] public bool lookingForChild;
                [SerializeField] public bool childActive;
                [SerializeField] public bool openNext;
                [SerializeField] public bool canHaveChildren = true;
                [SerializeField] public List<string> refName = new List<string>();
                [SerializeField] public List<NodeMessage> message = new List<NodeMessage>();

                public Rect GetRect(Vector2 offset = default(Vector2))
                {
                        return new Rect(rectPosition.x + offset.x, rectPosition.y + offset.y, rectSize.x, rectSize.y);
                }

                public Vector2 RectCenter()
                {
                        return new Vector2(rectPosition.x + rectSize.x * 0.5f, rectPosition.y + rectSize.y * 0.5f);
                }

                public void SetParent(Node node) { nodeParent = node; }

                public virtual List<Node> Children() { return null; }

                public bool CanHaveChildren() { return nodeType == NodeType.Composite; }

                public virtual void OnSceneGUI(UnityEditor.Editor editor) { }

                public virtual bool OnInspector(AIBase ai, SerializedObject element, Color color, bool onEnable)
                {
                        return false;
                }

                public virtual bool HasNextState()
                {
                        return true;
                }
                public virtual void OnDrawGizmos()
                {
                        hideFlags = HideFlags.HideInInspector;
                }

#pragma warning restore 0414
#endif
                #endregion
        }

#if UNITY_EDITOR
        [System.Serializable]
        public class NodeMessage
        {
                public Vector2 position;
                public Vector2 size;
                public string message;
                public bool isDragged;

                public NodeMessage(Vector2 position)
                {
                        position.y += 30f;
                        this.position = position;
                        this.size = new Vector2(80f, 30f);
                        this.message = "Node Message";
                }

                public Rect GetRect(Vector2 offset)
                {
                        return new Rect(position.x + offset.x, position.y + offset.y, size.x, size.y);
                }
        }
#endif

        public enum NodeState
        {
                Failure,
                Running,
                Success
        }

        public enum NodeSetup
        {
                NeedToInitialize,
                CurrentlyRunning
        }

        public enum NodeType
        {
                Composite,
                Action,
                Decorator,
                Conditional
        }
}