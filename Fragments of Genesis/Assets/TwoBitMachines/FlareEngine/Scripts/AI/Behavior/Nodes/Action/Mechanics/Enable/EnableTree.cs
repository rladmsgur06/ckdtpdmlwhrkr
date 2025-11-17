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
        public class EnableTree : Action
        {
                [SerializeField] public bool pause;
                [SerializeField] public bool remove;
                [SerializeField] public bool enableObject = true;
                [System.NonSerialized] public float counter;

                public override NodeState RunNodeLogic (Root root)
                {
                        root.pause = pause;
                        if (remove)
                        {
                                Character character = gameObject.GetComponent<Character>();
                                character?.RemoveAI();
                        }
                        gameObject.SetActive(enableObject);
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
                                Labels.InfoBoxTop(75, "Pause or remove the BehaviorTree from executing. Also specify the active state of the gameobject that belongs to the BehaviorTree." +
                                        "\n \nReturns Success");
                        }
                        FoldOut.Box(3, color, offsetY: -2);
                        {
                                parent.Field("Pause", "pause");
                                parent.Field("Remove", "remove");
                                parent.Field("Enable Object", "enableObject");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
