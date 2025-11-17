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
        public class PlayerInputAny : Conditional
        {
                public override NodeState RunNodeLogic (Root root)
                {
                        for (int i = 0; i < WorldManager.inputs.Count; i++)
                        {
                                if (WorldManager.inputs[i].Any())
                                        return NodeState.Success;
                        }
                        return NodeState.Failure;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai , SerializedObject parent , Color color , bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(35 , "Returns success if any input is active.");
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

}
