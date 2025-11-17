using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("Flare Engine/一WorldEvents/WorldEvent")]
        public class WorldEventTrigger : MonoBehaviour
        {
                public WorldEventSO worldEvent;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] private bool foldOut = false;
                [SerializeField] private string eventName = "";
#pragma warning restore 0414
#endif
                #endregion

                public void TriggerEvent () // Call this to trigger the world event, 
                {
                        if (worldEvent != null)
                                worldEvent.TriggerEvent();
                }
        }
}

// #if UNITY_EDITOR
// using TwoBitMachines.Editors;
// using UnityEditor;
// #endif
// using UnityEngine;
// using UnityEngine.Events;

// namespace TwoBitMachines.FlareEngine.AI
// {
//         [AddComponentMenu("")]
//         public class WorldEventTrigger : Action
//         {
//                 [SerializeField] public WorldEventSO worldEvent;

//                 public override NodeState RunNodeLogic (Root root)
//                 {
//                         if (worldEvent == null)
//                         {
//                                 return NodeState.Failure;
//                         }

//                         worldEvent.TriggerEvent();
//                         return NodeState.Success;
//                 }

//                 #region ▀▄▀▄▀▄ Custom Inspector▄▀▄▀▄▀
// #if UNITY_EDITOR
// #pragma warning disable 0414
//                 [SerializeField, HideInInspector] public bool eventsFoldOut;
//                 [SerializeField, HideInInspector] public bool beginFoldOut;
//                 [SerializeField, HideInInspector] public bool finishFoldOut;

//                 public override bool OnInspector (AIBase ai,
//                  SerializedObject parent, Color color, bool onEnable)
//                 {
//                         if (parent.Bool("showInfo"))
//                         {
//                                 Labels.InfoBoxTop(
//                                     40,
//                                     "Trigger a World Event"
//                                 );
//                         }

//                         FoldOut.Box(1, color, offsetY: -2);
//                         {
//                                 parent.Field("World Event", "worldEvent");
//                         }

//                         return true;
//                 }
// #pragma warning restore 0414
// #endif
//                 #endregion
//         }
// }
