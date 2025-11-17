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
        public class SetWorldFloat : Action
        {
                [SerializeField] public float value;
                [SerializeField] public WorldFloat worldFloat;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (worldFloat == null)
                        {
                                worldFloat = gameObject.GetComponent<WorldFloat>();
                        }
                        worldFloat?.SetValue(value);
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Set the WorldFloat value. If the reference is empty, the WorldFloat on this object will be used." +
                                            "\n \nReturns Success");
                        }
                        FoldOut.Box(1, color, offsetY: -2);
                        {
                                parent.FieldDouble("World Float", "worldFloat", "value");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
