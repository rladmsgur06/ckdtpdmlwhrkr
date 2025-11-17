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
        public class NullifyGravity : Action
        {
                private bool pause;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (!pause)
                        {
                                root.velocity.y -= root.gravity.gravityEffect;
                        }
                        return NodeState.Running;
                }

                public void Pause (bool value)
                {
                        pause = value;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai , SerializedObject parent , Color color , bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55 , "Undo the effect of gravity" +
                                        "\n \nReturns Running");
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
