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
        public class InputGetKeyAndSignalList : Action
        {
                [SerializeField] public List<KeySignal> list = new List<KeySignal>();
                [SerializeField] public Character character;
                private int index = -1;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                index = -1;
                        }
                        if (Input.anyKeyDown)
                        {
                                for (int i = 0; i < list.Count; i++)
                                {
                                        if (Input.GetKeyDown(list[i].key))
                                        {
                                                index = i;
                                                break;
                                        }
                                }
                        }
                        if (index >= 0 && index < list.Count && character != null)
                        {
                                character.signals.Set(list[index].signal);
                        }
                        return NodeState.Running;
                }

                [System.Serializable]
                public struct KeySignal
                {
                        public KeyCode key;
                        public string signal;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414

                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(85, "Will set a character signal true according the key pressed in this list.");
                        }

                        SerializedProperty array = parent.Get("list");
                        if (array.arraySize == 0)
                                array.arraySize++;

                        FoldOut.Box(1 + array.arraySize, color, extraHeight: 6, offsetY: -2);
                        {
                                parent.Field("Character", "character");
                        }
                        Block.BoxArray(array, color, 21, false, 0, "", (height, index) =>
                        {
                                Block.Header(array.Element(index)).BoxRect(Color.clear, leftSpace: 5, height: height)
                                                   .Field("key", weight: 0.5f)
                                                  .Field("signal", weight: 0.5f)
                                                  .ArrayButtons()
                                                  .BuildGet()
                                                  .ReadArrayButtons(array, index);
                        });

                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

}
