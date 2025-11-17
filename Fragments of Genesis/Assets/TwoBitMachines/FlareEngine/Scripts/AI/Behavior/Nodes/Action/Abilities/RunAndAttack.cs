#region 
#if UNITY_EDITOR
using System;
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class RunAndAttack : Action
        {
                [SerializeField] public Blackboard target;
                [SerializeField] public Collider2D collider2DRef;
                [SerializeField] public float speed = 12f;
                [SerializeField] public float smooth = 0.85f;
                [SerializeField] public float radius = 4f;
                [SerializeField] public float attempts = 5f;
                [SerializeField] public string signal;
                [SerializeField] public float holdTime = 0.5f;

                [System.NonSerialized] private float velocityX;
                [System.NonSerialized] private float oldDirectionX;
                [System.NonSerialized] private float attempted = 0;
                [System.NonSerialized] private float counter;
                [System.NonSerialized] private bool foundTarget;
                [System.NonSerialized] private bool hold;

                public void Reset ()
                {
                        velocityX = 0;
                        counter = 0;
                        foundTarget = false;
                        hold = false;
                        if (collider2DRef != null)
                                collider2DRef.gameObject.SetActive(false);
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (target == null)
                        {
                                return NodeState.Failure;
                        }
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                Reset();
                                attempted = 0;
                                oldDirectionX = -1;
                        }

                        Vector2 t = target.GetTarget();

                        if (this.target.hasNoTargets)
                        {
                                return NodeState.Failure;
                        }

                        if (hold)
                        {
                                root.signals.Set(signal);
                                if (TwoBitMachines.Clock.Timer(ref counter, holdTime))
                                {
                                        Reset();
                                        return NodeState.Success;
                                }
                        }
                        else if (foundTarget)
                        {
                                root.signals.Set(signal);
                                root.velocity.x = velocityX = Compute.Lerp(velocityX, 0, smooth);
                                if (Mathf.Abs(velocityX) < 0.25f)
                                {
                                        Reset();
                                        hold = true;
                                }
                        }
                        else
                        {

                                float directionX = t.x < root.position.x ? -1f : 1f;
                                root.velocity.x = velocityX = Compute.Lerp(velocityX, directionX * speed, smooth);

                                if ((t - root.position).sqrMagnitude <= radius * radius)
                                {
                                        if (collider2DRef != null)
                                                collider2DRef.gameObject.SetActive(true);
                                        foundTarget = true;
                                }
                                else if (oldDirectionX != Mathf.Sign(velocityX))
                                {
                                        oldDirectionX = Mathf.Sign(velocityX);
                                        attempted++;
                                        if (attempted >= attempts)
                                        {
                                                return NodeState.Success;
                                        }
                                }
                        }

                        if (collider2DRef != null)
                        {
                                Vector3 v = collider2DRef.transform.localPosition;
                                collider2DRef.transform.localPosition = new Vector3(Mathf.Sign(velocityX) > 0 ? Mathf.Abs(v.x) : -Mathf.Abs(v.x), v.y, v.z);
                        }
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Follow a target in the x direction, and attack if within radius." +
                                        "\n \nReturns Success, Failure");
                        }

                        FoldOut.Box(6, color, offsetY: -2);
                        {
                                AIBase.SetRef(ai.data, parent.Get("target"), 0);
                                parent.FieldDouble("Speed", "speed", "smooth");
                                Labels.FieldText("Smooth");
                                parent.FieldDouble("Attack Radius", "radius", "collider2DRef");
                                Labels.FieldDoubleText("Radius", "");
                                parent.Field("Attack Signal", "signal");
                                parent.Field("Hold Time", "holdTime");
                                parent.Field("Attempts", "attempts");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

}
