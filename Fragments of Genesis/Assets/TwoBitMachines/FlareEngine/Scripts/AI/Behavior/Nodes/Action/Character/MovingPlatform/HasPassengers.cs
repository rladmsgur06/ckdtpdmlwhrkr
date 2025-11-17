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
        public class HasPassengers : Conditional
        {
                public override NodeState RunNodeLogic (Root root)
                {
                        return root.movingPlatform.hasPassengers ? NodeState.Success : NodeState.Failure;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(35, "Does this moving platform have passengers?");
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
