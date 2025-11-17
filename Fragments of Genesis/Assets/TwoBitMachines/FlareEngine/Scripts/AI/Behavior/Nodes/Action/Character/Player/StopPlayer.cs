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
        public class StopPlayer : Action
        {
                [SerializeField] public Player player;

                [SerializeField] public string stopSignal;
                [SerializeField] public float stopTime = 0.5f;

                private Vector3 holdPosition;
                private float counter;

                public void Reset ()
                {
                        counter = 0;
                }

                public override void OnReset (bool skip = false, bool enteredState = false)
                {
                        ResetInput();
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (player == null)
                        {
                                return NodeState.Success;
                        }
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                Reset();
                                counter = 0;
                                holdPosition = player.transform.position;
                                if (player != null)
                                {
                                        player.BlockInput(true);
                                }
                        }

                        if (TwoBitMachines.Clock.Timer(ref counter, stopTime))
                        {
                                ResetInput();
                                return NodeState.Success;
                        }

                        Vector3 p = player.transform.position;
                        player.transform.position = new Vector3(holdPosition.x, p.y, p.z);
                        player.signals.Set(stopSignal);

                        return NodeState.Running;
                }

                public void ResetInput ()
                {
                        if (player != null)
                        {
                                player.BlockInput(false);
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Stop the player from moving for the specified time." +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        {
                                parent.Field("Player", "player");
                                parent.FieldDouble("Stop Time", "stopTime", "stopSignal");
                                Labels.FieldText("Signal");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
