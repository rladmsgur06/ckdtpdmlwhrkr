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
        public class GrabPlayer : Action
        {
                [SerializeField] public Player player;

                [SerializeField] public string grabSignal;
                [SerializeField] public float grabTime = 0.5f;
                [SerializeField] public float grabRadius = 1.5f;

                [SerializeField] public float holdTime = 4f;
                [SerializeField] public float damageRate = 0.2f;
                [SerializeField] public string holdSignal;
                [SerializeField] public string playerHoldSignal;
                [SerializeField] public Transform holdPoint;

                [System.NonSerialized] private int direction;
                [System.NonSerialized] private float counterHold;
                [System.NonSerialized] private float counterGrab;
                [System.NonSerialized] private float lerp;
                [System.NonSerialized] private bool foundPlayer;
                [System.NonSerialized] private State state;
                [System.NonSerialized] private Health health;
                [System.NonSerialized] private Vector2 startPosition;

                public enum State { Grabbing, Hold }
                private Vector2 playerPosition => player.transform.position;

                public void Reset ()
                {
                        state = 0;
                        lerp = 0;
                        counterHold = 0;
                        counterGrab = 0;
                }

                public override void OnReset (bool skip = false, bool enteredState = false)
                {
                        ResetInput();
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (player == null)
                        {
                                return NodeState.Failure;
                        }
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                Reset();
                                foundPlayer = false;
                                direction = root.direction;
                                Compute.FlipLocalPositionX(holdPoint, direction);
                                if (health == null)
                                {
                                        health = player.gameObject.GetComponent<Health>();
                                }
                        }
                        if (state == State.Grabbing)
                        {
                                if (TwoBitMachines.Clock.Timer(ref counterGrab, grabTime))
                                {
                                        if (!foundPlayer)
                                        {
                                                ResetInput();
                                                return NodeState.Failure;
                                        }
                                        state = State.Hold;
                                }
                                if (!foundPlayer && (playerPosition - root.position).sqrMagnitude <= grabRadius * grabRadius)
                                {
                                        foundPlayer = true;
                                        startPosition = player.transform.position;
                                }
                                root.signals.Set(grabSignal);
                                HoldPlayer(root);

                        }
                        if (state == State.Hold)
                        {
                                if (TwoBitMachines.Clock.Timer(ref counterHold, holdTime))
                                {
                                        ResetInput();
                                        return NodeState.Success;
                                }
                                root.signals.Set(holdSignal);
                                HoldPlayer(root);
                        }
                        return NodeState.Running;
                }

                public void HoldPlayer (Root root)
                {
                        if (foundPlayer)
                        {
                                lerp += Root.deltaTime;
                                Vector3 position = player.transform.position;
                                Vector3 newPosition = Vector2.Lerp(startPosition, holdPoint.position, lerp / 0.075f);
                                player.transform.position = new Vector3(newPosition.x, newPosition.y, position.z);
                                player.BlockInput(true);
                                player.signals.Set(playerHoldSignal);
                                player.signals.ForceDirection(-direction);
                                player.abilities.velocity = Vector2.zero; // clears gravity
                                if (health != null)
                                        health.Increment(Root.deltaTime * -damageRate);
                        }
                }

                public void ResetInput ()
                {
                        if (foundPlayer && player != null)
                        {
                                player.BlockInput(false);
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool beginFoldOut;
                [SerializeField, HideInInspector] public bool beginTransitionFoldOut;

                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(45, "Grab and hold the player." +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(3, color, offsetY: -2);
                        {
                                parent.Field("Player", "player");
                                parent.FieldDouble("Grab", "grabTime", "grabRadius");
                                Labels.FieldDoubleText("Time", "Radius");
                                parent.Field("Signal", "grabSignal");
                        }
                        Layout.VerticalSpacing(3);
                        FoldOut.Box(4, color);
                        {
                                parent.FieldDouble("Hold", "holdTime", "damageRate");
                                Labels.FieldDoubleText("Time", "Damage");
                                parent.Field("Hold Point", "holdPoint");
                                parent.Field("Hold Signal", "holdSignal");
                                parent.Field("Player Hold Signal", "playerHoldSignal");
                        }
                        Layout.VerticalSpacing(5);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
