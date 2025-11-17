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
        public class SetWorldBool : Action
        {
                [SerializeField] public bool value;
                [SerializeField] public WorldBool worldBool;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (worldBool == null)
                        {
                                worldBool = gameObject.GetComponent<WorldBool>();
                        }
                        worldBool?.SetValue(value);
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
                                Labels.InfoBoxTop(55, "Set the WorldBool value. If the reference is empty, the WorldBool on this object will be used." +
                                            "\n \nReturns Success");
                        }
                        FoldOut.Box(1, color, offsetY: -2);
                        {
                                parent.FieldAndEnableRaw("World Bool", "worldBool", "value");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
