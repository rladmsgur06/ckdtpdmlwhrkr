#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;
using System.Collections.Generic;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class InputGetKeyList : Action
        {
                [SerializeField] public List<KeyCode> keys = new List<KeyCode>();
                [SerializeField] public UnityEventInt onPressed;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (Input.anyKeyDown)
                        {
                                for (int i = 0; i < keys.Count; i++)
                                {
                                        if (Input.GetKeyDown(keys[i]))
                                        {
                                                onPressed.Invoke(i);
                                                return NodeState.Success;
                                        }
                                }
                        }
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool pressedFoldOut;
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(85, "Will return Success if a key in the list was pressed. An event with the key index pressed is invoked.");
                        }

                        SerializedProperty array = parent.Get("keys");
                        if (array.arraySize == 0)
                                array.arraySize++;

                        FoldOut.Box(array.arraySize, color, extraHeight: 6, offsetY: -2);
                        {
                        }
                        Block.BoxArray(array, color, 21, false, 0, "", (height, index) =>
                        {
                                Block.Header(array.Element(index)).BoxRect(Color.clear, leftSpace: 5, height: height)
                                                  .Field(array.Element(index), weight: 1f)
                                                  .ArrayButtons()
                                                  .BuildGet()
                                                  .ReadArrayButtons(array, index);
                        });

                        Fields.EventFoldOut(parent.Get("onPressed"), parent.Get("pressedFoldOut"), "On Pressed", color: color);
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

}
