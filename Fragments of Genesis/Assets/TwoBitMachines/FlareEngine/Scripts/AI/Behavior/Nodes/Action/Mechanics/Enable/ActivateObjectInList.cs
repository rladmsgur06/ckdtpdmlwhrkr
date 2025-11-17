#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class ActivateObjectInList : Conditional
        {
                [SerializeField] public WorldFloat index;
                [SerializeField] public List<GameObject> list = new List<GameObject>();

                public override NodeState RunNodeLogic (Root root)
                {
                        if (index == null)
                                return NodeState.Failure;
                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i] != null)
                                        list[i].gameObject.SetActive(false);
                        }
                        for (int i = 0; i < list.Count; i++)
                        {
                                if (i == (int) index.GetValue() && list[i] != null)
                                        list[i].gameObject.SetActive(true);
                        }
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(35, "Activate a gameobject from a list according to an index.");
                        }

                        FoldOut.Box(1, color, offsetY: -2);
                        parent.Field("World Float", "index");
                        Layout.VerticalSpacing(3);
                        SerializedProperty array = parent.Get("list");
                        if (array.arraySize == 0)
                                array.arraySize++;

                        FoldOut.Box(array.arraySize, color, offsetY: -2);
                        array.FieldProperty("GameObject");
                        Layout.VerticalSpacing(1);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
