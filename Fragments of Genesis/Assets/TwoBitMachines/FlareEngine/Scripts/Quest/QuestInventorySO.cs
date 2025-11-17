using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [CreateAssetMenu (menuName = "FlareEngine/QuestInventorySO")]
        public class QuestInventorySO : JournalInventory
        {
                [SerializeField] public List<QuestSO> questSO = new List<QuestSO> ( );
                [SerializeField, HideInInspector] public bool foldOut;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField, HideInInspector] private int signalIndex;
                [SerializeField, HideInInspector] private bool active;
                #endif
                #endregion

                public override List<QuestSO> QuestList ( )
                {
                        return questSO;
                }
        }
}