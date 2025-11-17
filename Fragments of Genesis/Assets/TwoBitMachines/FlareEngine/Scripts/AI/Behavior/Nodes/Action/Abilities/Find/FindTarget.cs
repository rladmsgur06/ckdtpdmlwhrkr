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
        public class FindTarget : Conditional
        {
                [SerializeField] public Blackboard target;
                [SerializeField] public AISearchType findType;
                [SerializeField] public float radius;
                [SerializeField] public bool mirror;
                [SerializeField] public Vector2 offset;
                [SerializeField] public Vector2 distance;
                [SerializeField] public Blackboard territory;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (target == null)
                                return NodeState.Failure;

                        return Search(root.position, target.GetTarget(), root) ? NodeState.Success : NodeState.Failure;
                }

                private bool Search (Vector2 position, Vector2 target, Root root)
                {
                        if (this.target.hasNoTargets)
                        {
                                return false;
                        }
                        if (findType == AISearchType.Radius)
                        {
                                Vector2 offset = this.offset;
                                offset.x = !mirror ? offset.x : Mathf.Abs(offset.x) * Mathf.Sign(root.direction);
                                if ((target - (position + offset)).sqrMagnitude < radius * radius)
                                {
                                        return true;
                                }
                        }
                        else if (findType == AISearchType.Distance)
                        {
                                Vector2 offset = this.offset;
                                offset.x = !mirror ? offset.x : Mathf.Abs(offset.x) * Mathf.Sign(root.direction);
                                if (distance.x != 0 && (target.x > (position.x + offset.x + distance.x * 0.5f) || target.x < (position.x + offset.x - distance.x * 0.5f)))
                                {
                                        return false;
                                }
                                if (distance.y != 0 && (target.y > (position.y + offset.y + distance.y * 0.5f) || target.y < (position.y + offset.y - distance.y * 0.5f)))
                                {
                                        return false;
                                }
                                return true;
                        }
                        else if (territory != null)
                        {
                                return territory.Contains(target);
                        }
                        return false;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(35, "Find the specified Target in relation to the AI or a Territory.");
                        }
                        int type = parent.Enum("findType");
                        int height = type < 2 ? 2 : 0;
                        FoldOut.Box(3 + height, color, offsetY: -2);
                        AIBase.SetRef(ai.data, parent.Get("target"), 0);
                        parent.Field("Type", "findType");
                        parent.Field("Radius", "radius", execute: type == 0);
                        parent.Field("Distance", "distance", execute: type == 1);
                        if (type == 2)
                        {
                                AIBase.SetRef(ai.data, parent.Get("territory"), 1);
                        }
                        else
                        {
                                parent.Field("Offset", "offset");
                                parent.FieldToggle("Mirror Offset", "mirror");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }

                public override void OnSceneGUI (UnityEditor.Editor editor)
                {
                        Vector2 offset = this.offset;
                        if (Application.isPlaying)
                        {
                                AIBase ai = editor.target as AIBase;
                                offset.x = !mirror ? offset.x : Mathf.Abs(offset.x) * Mathf.Sign(ai.signals.characterDirection);
                        }
                        if (findType == AISearchType.Radius)
                        {
                                Draw.GLCircleInit(transform.position + (Vector3) offset, radius, Color.blue);
                        }
                        else if (findType == AISearchType.Distance)
                        {
                                Draw.SquareCenter(transform.position + (Vector3) offset, distance, Color.blue);
                        }
                }

#pragma warning restore 0414
#endif
                #endregion

        }

        public enum AISearchType
        {
                Radius,
                Distance,
                Territory
        }

}
