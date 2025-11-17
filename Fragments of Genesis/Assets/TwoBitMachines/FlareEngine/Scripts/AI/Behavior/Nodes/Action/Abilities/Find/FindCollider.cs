#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class FindCollider : Action
        {
                [SerializeField] public LayerMask colliderLayer;
                [SerializeField] private bool found;
                [SerializeField] private bool beginSearch;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                found = false;
                                beginSearch = true;
                        }
                        return found ? NodeState.Success : NodeState.Running;
                }

                public override void OnReset (bool skip = false, bool enteredState = false)
                {
                        found = false;
                        beginSearch = false;
                }

                public void OnTriggerEnter2D (Collider2D collider)
                {
                        if (beginSearch && Compute.ContainsLayer(colliderLayer, collider.gameObject.layer))
                        {
                                found = true;
                        }
                }

                public void OnTriggerStay2D (Collider2D collider)
                {
                        if (beginSearch && Compute.ContainsLayer(colliderLayer, collider.gameObject.layer))
                        {
                                found = true;
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(70, "Use OnTriggerEnter2D and OnTriggerStay2D to detect a collider. Set the collider" +
                                        " on this AI to IsTrigger." + "\n \nReturns Success if found, otherwise returns Running."
                                );
                        }

                        FoldOut.Box(1, color, offsetY: -2);
                        {
                                parent.Field("Collider Layer", "colliderLayer");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }

#pragma warning restore 0414
#endif
                #endregion

        }

}
