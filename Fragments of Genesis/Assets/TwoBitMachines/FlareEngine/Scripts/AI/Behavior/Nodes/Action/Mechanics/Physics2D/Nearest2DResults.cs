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
        public class Nearest2DResults : Action
        {
                [SerializeField] public Blackboard setTarget;
                [SerializeField] public SetResultAs setTargetAs;
                [SerializeField] public ResultsIs check;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (setTarget == null)
                        {
                                return NodeState.Failure;
                        }

                        if (check == ResultsIs.Single2DResult)
                        {
                                if (Root.collider2DRef != null)
                                {
                                        SetValue(Root.collider2DRef);
                                        return NodeState.Success;
                                }
                        }
                        else if (FindNearest(root))
                        {
                                return NodeState.Success;
                        }

                        return NodeState.Failure;
                }

                private bool FindNearest (Root root)
                {
                        float distance = float.MaxValue;
                        int index = -1;
                        for (int i = 0; i < Root.colliderResults.Count; i++)
                        {
                                if (Root.colliderResults[i] == null)
                                        continue;
                                float sqrMagnitude = (root.position - (Vector2) Root.colliderResults[i].transform.position).sqrMagnitude;
                                if (sqrMagnitude < distance)
                                {
                                        index = i;
                                        distance = sqrMagnitude;
                                }
                        }

                        if (index > -1)
                        {
                                SetValue(Root.colliderResults[index]);
                                return true;
                        }

                        return false;
                }

                private void SetValue (Collider2D collider2D)
                {
                        if (setTargetAs == SetResultAs.Transform)
                        {
                                setTarget.Set(collider2D.transform);
                        }
                        else if (setTargetAs == SetResultAs.Collider2D)
                        {
                                setTarget.Set(collider2D);
                        }
                        else if (setTargetAs == SetResultAs.GameObject)
                        {
                                setTarget.Set(collider2D.gameObject);
                        }
                        else if (setTargetAs == SetResultAs.Vector2)
                        {
                                setTarget.Set((Vector2) collider2D.transform.position);
                        }
                        else if (setTargetAs == SetResultAs.Vector3)
                        {
                                setTarget.Set(collider2D.transform.position);
                        }

                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(65, "Get the nearest target to the AI from a list of physics results and set the reference to this target." +
                                        "\n \nReturns Success, Failure");
                        }

                        FoldOut.Box(3, color, offsetY: -2);
                        parent.Field("Check", "check");
                        AIBase.SetRef(ai.data, parent.Get("setTarget"), 0);
                        parent.Field("Set Target As", "setTargetAs");
                        Layout.VerticalSpacing(3);

                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        public enum SetResultAs
        {
                Transform,
                Collider2D,
                GameObject,
                Vector2,
                Vector3,
        }

        public enum ResultsIs
        {
                List2DResults,
                Single2DResult
        }
}
