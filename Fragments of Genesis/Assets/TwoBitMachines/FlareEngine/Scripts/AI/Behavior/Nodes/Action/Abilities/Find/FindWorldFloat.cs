#region 
#if UNITY_EDITOR
using System.ComponentModel;
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class FindWorldFloat : Conditional
        {
                [SerializeField] public LayerMask layerMask;
                [SerializeField] public float radius = 2f;
                [SerializeField] public FloatLogicType logic;
                [SerializeField] public CompareTo compareTo;
                [SerializeField] public float compareFloat;
                [SerializeField] public WorldFloat compareVariable;

                private static Collider2D[] results = new Collider2D[25];
                private ContactFilter2D filter2d = new ContactFilter2D();

                public override NodeState RunNodeLogic (Root root)
                {
                        if (!SearchWorldFloat(out WorldFloat worldFloat))
                        {
                                return NodeState.Failure;
                        }

                        float compareValue = compareTo == CompareTo.Value ? compareFloat : compareTo == CompareTo.OtherVariable && compareVariable != null ? compareVariable.GetValue() : 0;
                        return WorldFloatLogic.Compare(logic, worldFloat.GetValue(), compareValue);
                }

                private bool SearchWorldFloat (out WorldFloat worldFloat)
                {
                        worldFloat = null;
                        bool found = false;
                        float distance = Mathf.Infinity;
                        filter2d.useTriggers = true;
                        filter2d.useLayerMask = true;
                        filter2d.layerMask = layerMask;
                        int length = Physics2D.OverlapCircle(transform.position, radius, filter2d, results);

                        for (int i = 0; i < length; i++)
                        {
                                float distanceSqr = (transform.position - results[i].transform.position).sqrMagnitude;
                                if (distanceSqr < distance)
                                {
                                        WorldFloat newWorldFloat = results[i].GetComponent<WorldFloat>();
                                        if (newWorldFloat != null)
                                        {
                                                distance = distanceSqr;
                                                worldFloat = newWorldFloat;
                                                found = true;
                                        }
                                }
                        }
                        return found;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(40, "Find the nearest World Float and test its value. This will work with Health.");
                        }

                        int logic = parent.Enum("logic");
                        int height = logic <= 4 ? 1 : 0;
                        int type = parent.Enum("compareTo");
                        FoldOut.Box(3 + height, color, offsetY: -2);
                        {
                                parent.Field("Layer", "layerMask");
                                parent.Field("Radius", "radius");
                                parent.Field("Logic", "logic");
                                parent.FieldDouble("Compare To", "compareTo", "compareFloat", execute: type == 0 && height == 1);
                                parent.FieldDouble("Compare To", "compareTo", "compareVariable", execute: type == 1 && height == 1);
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }

                public override void OnSceneGUI (UnityEditor.Editor editor)
                {
                        Draw.GLCircleInit(transform.position, radius, Color.blue);
                }

#pragma warning restore 0414
#endif
                #endregion

        }

}
