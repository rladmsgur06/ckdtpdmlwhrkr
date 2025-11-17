using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class JournalInventory : ScriptableObject
        {
                public virtual List<QuestSO> QuestList ( )
                {
                        return null;
                }

                public virtual List<ItemSO> ItemList ( )
                {
                        return null;
                }
        }
}