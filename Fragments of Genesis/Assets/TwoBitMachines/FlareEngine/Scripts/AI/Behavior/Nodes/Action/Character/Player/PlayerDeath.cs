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
        public class PlayerDeath : Action
        {
                [SerializeField] public Player player;
                [SerializeField] public string signalName = "death";
                [SerializeField] public float time = 3f;
                [SerializeField] public float transitionTime = 1f;

                [SerializeField] public UnityEvent onBegin;
                [SerializeField] public UnityEvent onTransitionBegin;

                [System.NonSerialized] public float counter;
                [System.NonSerialized] public BoxCollider2D boxCollider2D;
                [System.NonSerialized] private DeathState state;

                public enum DeathState
                {
                        Wait,
                        Active,
                        Transition,
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (player == null)
                        {
                                return NodeState.Failure;
                        }
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                if (boxCollider2D == null)
                                {
                                        boxCollider2D = player.gameObject.GetComponent<BoxCollider2D>();
                                }
                                if (boxCollider2D != null)
                                {
                                        boxCollider2D.enabled = false;
                                }
                                counter = 0;
                                state = DeathState.Active;
                                onBegin.Invoke();
                        }

                        player.BlockInput(true);
                        player.signals.Set(signalName);

                        switch (state)
                        {
                                case DeathState.Active:
                                        if (TwoBitMachines.Clock.Timer(ref counter, time))
                                        {
                                                if (transitionTime == 0)
                                                {
                                                        ResetSettings();
                                                        return NodeState.Success;
                                                }
                                                else
                                                {
                                                        counter = 0;
                                                        state = DeathState.Transition;
                                                        onTransitionBegin.Invoke();
                                                }
                                        }
                                        break;
                                case DeathState.Transition:
                                        if (TwoBitMachines.Clock.Timer(ref counter, transitionTime))
                                        {
                                                ResetSettings();
                                                return NodeState.Success;
                                        }
                                        break;
                        }
                        return NodeState.Running;
                }

                private void ResetSettings ()
                {
                        state = DeathState.Wait;
                        player.BlockInput(false);
                        if (boxCollider2D != null)
                        {
                                boxCollider2D.enabled = true;
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
                                Labels.InfoBoxTop(120, "Once the player is killed, set the animation signal for the specified time. Transition time will continue to hold the player in the death state in case you want to activate a game transition once the regular time period is over. This will block player input and disable its collider." +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(4, color, offsetY: -2);
                        parent.Field("Player", "player");
                        parent.Field("Signal Name", "signalName");
                        parent.Field("Time", "time");
                        parent.Field("Transition Time", "transitionTime");
                        Layout.VerticalSpacing(3);
                        Fields.EventFoldOut(parent.Get("onBegin"), parent.Get("beginFoldOut"), "On Begin", color: color, offsetY: -2);
                        Fields.EventFoldOut(parent.Get("onTransitionBegin"), parent.Get("beginTransitionFoldOut"), "On Transition Begin", color: color, offsetY: -2);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
