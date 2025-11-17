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

        public class TeleportNextToTarget : Action
        {
                [SerializeField] public Blackboard target;
                [SerializeField] public TeleportType type;
                [SerializeField] public Vector2 offset;
                [SerializeField] public float distance;
                [SerializeField] public bool exitOnWall;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (target == null)
                        {
                                return NodeState.Failure;
                        }

                        return type == TeleportType.Instant ? Instant(root) : Distance(root);
                }

                public NodeState Distance (Root root)
                {
                        BoxInfo box = root.world.box;
                        float tempDistance = target.GetTarget().x - root.position.x;
                        if (this.target.hasNoTargets)
                        {
                                return NodeState.Failure;
                        }

                        float sign = Mathf.Sign(tempDistance);
                        float magnitude = distance + Mathf.Abs(tempDistance) + box.skin.x * 2f;
                        Vector2 corner = sign > 0 ? box.bottomRight - box.skinX : box.bottomLeft + box.skinX;

                        for (int i = 0; i < box.rays.x; i++)
                        {
                                Vector2 origin = corner + box.up * box.spacing.y * i;
                                RaycastHit2D hit = Physics2D.Raycast(origin, box.right * sign, magnitude, WorldManager.collisionMask);
                                if (hit)
                                {
                                        if (exitOnWall)
                                                return NodeState.Failure;
                                        if (hit.distance > 0)
                                                magnitude = hit.distance - box.skin.x * 2f;
                                }
                        }
                        this.transform.position += Vector3.right * sign * magnitude; // teleport
                        return NodeState.Success;
                }


                public NodeState Instant (Root root)
                {
                        Vector3 position = target.GetTarget();
                        if (this.target.hasNoTargets)
                        {
                                return NodeState.Failure;
                        }
                        float direction = position.x >= root.position.x ? -1f : 1f;
                        position.x += direction * offset.x;
                        position.y += offset.y;
                        position.z = this.transform.position.z;
                        this.transform.position = position;
                        return NodeState.Success;
                }

                public enum TeleportType
                {
                        DistanceAndWallCheck,
                        Instant
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject so, Color color, bool onEnable)
                {
                        if (so.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Teleport next to the target by the distance specified. Exit teleportation if there is a wall in the way. Or teleport instantly." +
                                        "\n \nReturns Success, Failure");
                        }
                        int type = so.Enum("type");
                        int height = type == 0 ? 1 : 0;
                        FoldOut.Box(3 + height, color, offsetY: -2);
                        {
                                AIBase.SetRef(ai.data, so.Get("target"), 0);
                                so.Field("Type", "type");
                                so.Field("Distance", "distance", execute: type == 0);
                                so.Field("Exit on Wall", "exitOnWall", execute: type == 0);
                                so.Field("Offset", "offset", execute: type == 1);
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
