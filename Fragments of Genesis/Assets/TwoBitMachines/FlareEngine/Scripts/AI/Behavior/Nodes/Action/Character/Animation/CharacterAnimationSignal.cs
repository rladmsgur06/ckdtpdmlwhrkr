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
        public class CharacterAnimationSignal : Action
        {
                [SerializeField] public CollisionStatusType cType;
                [SerializeField] public Character reference;
                [SerializeField] public string signalName;

                public override NodeState RunNodeLogic (Root root)
                {
                        bool set = false;
                        if (cType == CollisionStatusType.OtherCharacter)
                        {
                                if (reference == null)
                                        return NodeState.Failure;
                                reference.signals.Set(signalName);
                                set = true;
                        }
                        else
                        {
                                for (int i = 0; i < ThePlayer.Player.players.Count; i++)
                                {
                                        ThePlayer.Player.players[i].signals.Set(signalName);
                                        set = true;
                                }
                        }
                        return set ? NodeState.Running : NodeState.Failure;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Set the following animation signal true for the specified character" +
                                            "\n \nReturns Running, Failure");
                        }
                        int cType = parent.Enum("cType");
                        FoldOut.Box(cType == 1 ? 3 : 2, color, offsetY: -2);
                        parent.Field("For", "cType");
                        parent.Field("World Collision", "reference", execute: cType == 1);
                        parent.Field("Signal Name", "signalName");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
