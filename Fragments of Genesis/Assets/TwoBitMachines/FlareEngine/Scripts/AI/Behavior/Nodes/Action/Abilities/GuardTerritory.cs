#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class GuardTerritory : Action
        {
                [SerializeField] public Blackboard target;
                [SerializeField] public Blackboard guardPoint;
                [SerializeField] public Blackboard territory;
                [SerializeField] public float speed = 5f;
                [SerializeField] public float findDistance;
                [SerializeField] private float xVelRef;
                [SerializeField] public UnityEvent onEnter;
                [SerializeField] public UnityEvent onExit;

                [System.NonSerialized] private bool wasFollowing = false;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (target == null || guardPoint == null || territory == null)
                        {
                                return NodeState.Failure;
                        }
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                xVelRef = 0;
                                wasFollowing = false;
                        }
                        if (Root.deltaTime == 0)
                        {
                                return NodeState.Running;
                        }

                        Vector2 followTarget = target.GetTarget();
                        bool isFollowingTarget = territory.Contains(followTarget);
                        Vector2 actualTarget = isFollowingTarget ? followTarget : guardPoint.GetTarget();

                        if (this.target.hasNoTargets)
                        {
                                return NodeState.Failure;
                        }

                        if (!wasFollowing && isFollowingTarget)
                        {
                                onEnter.Invoke();
                        }
                        if (wasFollowing && !isFollowingTarget)
                        {
                                onExit.Invoke();
                        }
                        wasFollowing = isFollowingTarget;

                        float newP = Mathf.MoveTowards(root.position.x, actualTarget.x, speed * Root.deltaTime);
                        root.velocity.x += (newP - root.position.x) / Root.deltaTime;
                        if (!root.onGround)
                        {
                                root.velocity.x = xVelRef; // If jumping, retain old direction.
                        }
                        else if (isFollowingTarget && (Mathf.Abs(actualTarget.x - root.position.x) <= findDistance))
                        {
                                root.velocity.x = 0;
                                return NodeState.Success;
                        }
                        else if (!isFollowingTarget && (Mathf.Abs(actualTarget.x - root.position.x) <= 0))
                        {
                                root.velocity.x = 0;
                        }
                        xVelRef = root.velocity.x;
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool eventsFoldOut;
                [SerializeField, HideInInspector] public bool enterFoldOut;
                [SerializeField, HideInInspector] public bool exitFoldOut;

                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(80, "The AI will follow the target if the target is inside the territory.Else, the AI moves to the guard point. This returns Success when the target is within the stop distance." +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(5, color, extraHeight: 5, offsetY: -2);
                        {
                                AIBase.SetRef(ai.data, parent.Get("target"), 0);
                                AIBase.SetRef(ai.data, parent.Get("guardPoint"), 1);
                                AIBase.SetRef(ai.data, parent.Get("territory"), 2);
                                parent.Field("Speed", "speed");
                                parent.Field("Stop Distance", "findDistance");
                        }
                        if (FoldOut.FoldOutButton(parent.Get("eventsFoldOut")))
                        {
                                Fields.EventFoldOut(parent.Get("onEnter"), parent.Get("enterFoldOut"), "On Enter", color: color);
                                Fields.EventFoldOut(parent.Get("onExit"), parent.Get("exitFoldOut"), "On Exit", color: color);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
