#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class FollowPlatformWalls : Action
        {
                [SerializeField] public float speed;
                [SerializeField] public float probability = 1f;

                [System.NonSerialized] private Vector2 oldNormal = Vector2.zero;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (root.world.onGround && root.world.onWall)
                        {
                                Vector2 corner = root.world.box.topCenter;
                                RaycastHit2D hit = Physics2D.Raycast(corner, root.world.box.right * root.direction, root.world.box.size.x, WorldManager.collisionMask);
                                Debug.DrawRay(corner, root.world.box.right * root.direction * root.world.box.size.x, Color.red);
                                if (ChangeDirection(hit, root, root.direction))
                                {
                                        return NodeState.Success;
                                }
                        }
                        if (root.world.onGround && root.world.missedAVertical)
                        {
                                Vector2 corner = root.world.box.BottomCorner(root.direction) - root.world.box.skinY * 2f;
                                RaycastHit2D hit = Physics2D.Raycast(corner, -root.world.box.right * root.direction, root.world.box.size.x, WorldManager.collisionMask);
                                Debug.DrawRay(corner, -root.world.box.right * root.direction * root.world.box.size.x, Color.red);
                                if (ChangeDirection(hit, root, -root.direction))
                                {
                                        return NodeState.Success;
                                }
                        }
                        root.velocity.x += speed;
                        return NodeState.Running;
                }

                public override void OnReset (bool skip = false, bool enteredState = false)
                {
                        oldNormal = Vector2.zero;
                }

                private bool ChangeDirection (RaycastHit2D hit, Root root, int direction)
                {
                        if (hit && hit.distance > 0 && (oldNormal - hit.normal).magnitude > 0.1f)
                        {
                                oldNormal = hit.normal;
                                if (probability < 1f && probability >= 0 && Random.Range(0, 1f) >= probability)
                                {
                                        speed *= -1f;
                                        return true;
                                }
                                else
                                {
                                        transform.localEulerAngles += Vector3.forward * 90f * direction;
                                        transform.position = hit.point;
                                        root.world.box.Update();
                                        return true;
                                }
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
                                Labels.InfoBoxTop(90,
                                        "The AI will walk around platforms or walls, cutting corners. A probability of 1 " +
                                        "will ensure it always moves to the next corner or it might change direction. " +
                                        " Sprite must be set to bottom pivot." +
                                        "\n \nReturns Running, Success");
                        }
                        FoldOut.Box(2, color, offsetY: -2);
                        parent.Field("Speed", "speed");
                        parent.Field("Probability", "probability");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
