using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class SaveOptions
        {
                [SerializeField] public List<SaveSlot> slot = new List<SaveSlot>();
                [SerializeField] public int currentSlot = 0;
                [SerializeField] public string gameName = "";
                [SerializeField] public bool navigate = false;// encrypt
                [SerializeField] public bool marked = false;
                [SerializeField] public bool delete = false;
                [SerializeField] public int sceneDoor = 0;
                [SerializeField] public int sceneDoorPlayerDirection = 0;

                public const string folder = "Static";
                public const string key = "Misc";

                public void Save ()
                {
                        Storage.Save(this, folder, key); // general save, while using in editor
                }

                public void Save (int levelNumber, float playTime, bool isSaveMenu)
                {
                        if (!isSaveMenu)
                        {
                                for (int i = 0; i < slot.Count; i++)
                                {
                                        if (i == currentSlot)
                                        {
                                                slot[i].UpdateSettings(levelNumber, playTime);
                                                break;
                                        }
                                }
                        }
                        Storage.Save(this, folder, key);
                }

                public void DeleteSlotData (int slotIndex)
                {
                        for (int i = 0; i < slot.Count; i++)
                        {
                                if (i == slotIndex)
                                {
                                        slot[i].ClearSettings();
                                        Storage.DeleteAll("Slot" + i.ToString());
                                }
                        }
                        Storage.Save(this, folder, key); // general save, while using in editor
                }

                public void DeleteAllSlotsData ()
                {
                        for (int i = 0; i < slot.Count; i++)
                        {
                                slot[i].ClearSettings();
                                Storage.DeleteAll("Slot" + i.ToString());
                        }
                        currentSlot = 0;
                        Storage.Save(this, folder, key);
                }

                public string RetrieveSaveFolder ()
                {
                        if (slot.Count == 0 || currentSlot < 0 || currentSlot >= slot.Count)
                        {
                                return gameName;
                        }
                        for (int i = 0; i < slot.Count; i++)
                        {
                                if (i == currentSlot)
                                {
                                        return "Slot" + i.ToString(); // we make sure folder does indeed exist
                                }
                        }
                        return gameName;
                }

                public static void Load (ref SaveOptions save)
                {
                        save = Storage.Load<SaveOptions>(save, folder, key);
                }
        }

        [System.Serializable]
        public class SaveSlot
        {
                [SerializeField] public bool initialized = false;
                [SerializeField] public float totalTime = 0;
                [SerializeField] public float level = 0f;

                public void UpdateSettings (int levelNumber, float playTime)
                {
                        if (levelNumber > level)
                        {
                                level = levelNumber;
                        }
                        totalTime += playTime;
                }

                public void ClearSettings ()
                {
                        level = 0;
                        totalTime = 0;
                        initialized = false;
                }
        }
}
