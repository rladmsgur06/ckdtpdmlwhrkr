#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class LaunchCharacter : Action
        {
                [SerializeField] public Blackboard target;
                [SerializeField] public bool withinRange;
                [SerializeField] public float range = 1f;
                [SerializeField] public Vector2 launchVelocity;
                [System.NonSerialized] public Root root;

                public override NodeState RunNodeLogic (Root root)
                {
                        this.root = root;
                        if (!root.movingPlatform.hasPassengers) //character == null || character.world.mp.mp != root.movingPlatform)
                        {
                                return NodeState.Failure;
                        }
                        if (!withinRange)
                        {
                                root.movingPlatform.canBoostLaunch = true;
                                root.movingPlatform.launchVelocity = launchVelocity;
                        }
                        else if (target != null && (target.GetTarget() - root.position).sqrMagnitude <= (range * range) && !target.hasNoTargets)
                        {
                                root.movingPlatform.canBoostLaunch = true;
                                root.movingPlatform.launchVelocity = launchVelocity;
                        }
                        return NodeState.Running;
                }

                public override void OnReset (bool skip = false, bool enteredState = false)
                {
                        if (root != null)
                        {
                                root.movingPlatform.canBoostLaunch = false;
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414

                public override bool HasNextState ()
                {
                        return false;
                }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(35, "Launch a character if character moves off this moving platform.");
                        }

                        bool withinRange = parent.Bool("withinRange");
                        FoldOut.Box(2 + (withinRange ? 1 : 0), color, offsetY: -2);
                        {
                                parent.Field("Launch Velocity", "launchVelocity");
                                parent.FieldAndEnable("Within Target Range", "range", "withinRange");
                                if (withinRange)
                                {
                                        AIBase.SetRef(ai.data, parent.Get("target"), 0);
                                }
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
