#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.ThePlayer;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class JumpInObject : Conditional
        {
                [SerializeField] public Player player;
                [SerializeField] public float top = 1.5f;
                [SerializeField] public float width = 1.5f;
                [SerializeField] public UnityEvent onJumpedIn;

                [SerializeField] private State state;
                [SerializeField] private Health playerHealth;
                [SerializeField] private SpriteRenderer playerRenderer;
                [SerializeField] private SpriteRenderer objectRenderer;
                [SerializeField] private int playerOrder;
                [SerializeField] private string playerLayer;
                [SerializeField] private float moveCounter = 0;
                private float topPoint => transform.position.y + top;

                public enum State
                {
                        Waiting,
                        OverTop,
                        GoingInside,
                        HardReset
                }

                private void Awake ()
                {
                        playerRenderer = player.transform.GetComponent<SpriteRenderer>();
                        if (playerRenderer != null)
                        {
                                playerLayer = playerRenderer.sortingLayerName;
                                playerOrder = playerRenderer.sortingOrder;
                        }
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (player == null)
                        {
                                return NodeState.Failure;
                        }

                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                state = State.Waiting;
                                if (playerRenderer == null)
                                {
                                        playerRenderer = player.transform.GetComponent<SpriteRenderer>();
                                }
                                if (objectRenderer == null)
                                {
                                        objectRenderer = this.gameObject.GetComponent<SpriteRenderer>();
                                }
                                if (playerHealth == null)
                                {
                                        playerHealth = player.transform.GetComponent<Health>();
                                }
                        }

                        if (state == State.Waiting)
                        {
                                if (InXRange() && player.transform.position.y >= topPoint)
                                {
                                        state = State.OverTop;
                                }
                        }
                        if (state == State.OverTop)
                        {
                                if (!InXRange())
                                {
                                        state = State.Waiting;
                                }
                                else if (player.transform.position.y < topPoint)
                                {
                                        state = State.GoingInside;
                                        moveCounter = 0;
                                        player.world.isHidingExternal = true;
                                        player?.BlockInput(true);
                                        playerHealth?.CanTakeDamage(false);
                                        if (playerRenderer != null && objectRenderer != null)
                                        {
                                                playerRenderer.sortingLayerID = objectRenderer.sortingLayerID;
                                                playerRenderer.sortingOrder = -1;
                                        }
                                }
                        }
                        if (state == State.GoingInside)
                        {
                                float velX = (this.transform.position.x - player.transform.position.x) * 0.25f;
                                player.transform.position += new Vector3(velX, 0, 0);

                                if (TwoBitMachines.Clock.Timer(ref moveCounter, 0.10f))
                                {
                                        onJumpedIn.Invoke();
                                        if (playerRenderer != null)
                                        {
                                                playerRenderer.enabled = false;
                                        }
                                        return NodeState.Success;
                                }
                        }
                        return NodeState.Running;
                }

                public override bool HardReset ()
                {
                        state = State.Waiting;
                        player.world.isHidingExternal = false;
                        if (playerRenderer != null)
                        {
                                playerRenderer.enabled = true;
                                playerRenderer.sortingLayerName = playerLayer;
                                playerRenderer.sortingOrder = playerOrder;
                        }
                        return true;
                }

                private bool InXRange ()
                {
                        float targetX = player.transform.position.x;
                        float positionX = transform.position.x;
                        return targetX >= (positionX - width * 0.5f) && targetX <= (positionX + width * 0.5f);
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool eventFoldout;
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(65, "Player must first jump over the top point to enter this object. In this state, the player's hide flag goes true and can't take damage." +
                                        "\n \nReturns Running, Failure, Success");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        parent.Field("Player", "player");
                        parent.FieldDouble("Object", "top", "width");
                        Labels.FieldDoubleText("Top", "Width");
                        Layout.VerticalSpacing(1);
                        Fields.EventFoldOut(parent.Get("onJumpedIn"), parent.Get("eventFoldout"), "On Jumped In", color: color);
                        return true;
                }

                public override void OnSceneGUI (UnityEditor.Editor editor)
                {
                        Vector3 p = this.transform.position;
                        Debug.DrawLine(p + Vector3.up * top - Vector3.right * width * 0.5f, p + Vector3.up * top + Vector3.right * width * 0.5f, Color.red);
                }

#pragma warning restore 0414
#endif
                #endregion

        }
}
