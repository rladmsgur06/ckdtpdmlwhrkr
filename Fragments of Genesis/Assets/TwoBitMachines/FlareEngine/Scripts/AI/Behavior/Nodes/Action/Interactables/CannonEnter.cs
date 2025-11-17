#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.ThePlayer;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class CannonEnter : Conditional
        {
                [SerializeField] public Player player;

                [SerializeField] private SpriteRenderer spriteRenderer;
                [SerializeField] private Collider2D cannonCollider;
                [SerializeField] private bool needToClear = false;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (player == null || player.world.boxCollider == null)
                        {
                                return NodeState.Failure;
                        }
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                if (cannonCollider == null)
                                {
                                        cannonCollider = this.transform.GetComponent<Collider2D>();
                                }
                        }
                        if (needToClear)
                        {
                                if (!cannonCollider.IsTouching(player.world.boxCollider))
                                {
                                        needToClear = false;
                                }
                                if (needToClear)
                                {
                                        return NodeState.Running;
                                }
                        }
                        if (cannonCollider == null)
                        {
                                return NodeState.Failure;
                        }
                        if (cannonCollider.IsTouching(player.world.boxCollider))
                        {
                                return SetPlayerState() ? NodeState.Success : NodeState.Failure;
                        }
                        return NodeState.Running;
                }

                public bool SetPlayerState ()
                {
                        Cannon cannon = player.GetComponent<Cannon>();
                        if (cannon == null)
                        {
                                return false;
                        }
                        needToClear = true;
                        cannon.Follow(this.transform);
                        SetToCannonPosition(player.transform);
                        Health health = player.transform.GetComponent<Health>();
                        spriteRenderer = player.transform.GetComponent<SpriteRenderer>();
                        player.BlockInput(true);

                        if (health != null)
                        {
                                health.CanTakeDamage(false);
                        }
                        if (spriteRenderer != null)
                        {
                                spriteRenderer.enabled = false;
                        }
                        return true;
                }

                private void SetToCannonPosition (Transform target)
                {
                        Vector3 position = this.transform.position;
                        target.position = new Vector3(position.x, position.y, target.position.z);
                }

                public override bool HardReset ()
                {
                        needToClear = true;
                        if (spriteRenderer != null)
                        {
                                spriteRenderer.enabled = true;
                        }
                        return true;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(65, "Enter a cannon. Place a Collider2D on the AI to detect players on the specified layer. Set it to IsTrigger. " +
                                        "\n \nReturns Running, Failure, Success");
                        }

                        FoldOut.Box(1, color, offsetY: -2);
                        parent.Field("Player", "player");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
