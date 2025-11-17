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
        public class WorldEffect : Action
        {
                [SerializeField] public WorldEffects worldEffects;
                [SerializeField] public string effectName;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (worldEffects == null)
                        {
                                worldEffects = WorldEffects.get;
                        }
                        if (worldEffects == null)
                        {
                                return NodeState.Failure;
                        }
                        ImpactPacket effect = ImpactPacket.impact.Set(effectName, root.position, Vector2.zero);
                        worldEffects.Activate(effect);
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Call a world effect at the AI's position." +
                                        "\n \nReturns Running, Failure");
                        }

                        FoldOut.Box(2, color, offsetY: -2);
                        parent.Field("World Effect", "worldEffects");
                        parent.Field("Effect Name", "effectName");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
