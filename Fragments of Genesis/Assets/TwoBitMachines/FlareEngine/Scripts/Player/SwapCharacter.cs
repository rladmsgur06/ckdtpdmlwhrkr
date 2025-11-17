using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine
{
        public class SwapCharacter : MonoBehaviour
        {
                [SerializeField] public InputButtonSO buttonSwap;
                [SerializeField] public bool resetToFirst;
                [SerializeField] public string instantDeathTag;
                [SerializeField] public string swapWE;

                [SerializeField] public UnityEventEffect onSwap;
                [SerializeField] public UnityEvent onFailedToSwap; // all players probably dead
                [SerializeField] public CharacterSwapItems item;

                [System.NonSerialized] private bool onSwapEventActive = false;
                private static CharacterSwapItems temp = new CharacterSwapItems();

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private int signalIndex;
                [SerializeField, HideInInspector] private bool active;
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private bool swapFoldOut;
                [SerializeField, HideInInspector] private bool failedFoldOut;
#pragma warning restore 0414
#endif
                #endregion

                private void Start ()
                {
                        Load();
                        item.Initialize();
                        if (resetToFirst)
                        {
                                item.index = 0;
                        }
                        Swap(item.index);
                        onSwapEventActive = true;
                }

                private void OnDisable ()
                {
                        Save();
                }

                public void Update ()
                {
                        if (buttonSwap != null && buttonSwap.Pressed())
                        {
                                MoveToNextCharacter();
                        }
                }

                public void MoveToNextCharacter (ImpactPacket impact)
                {
                        if (impact.attacker != null && impact.attacker.gameObject.CompareTag(instantDeathTag))
                        {
                                onFailedToSwap.Invoke();
                                return;
                        }
                        MoveToNextCharacter();
                }

                public void MoveToNextCharacter ()
                {
                        for (int i = item.index + 1; i < item.list.Count; i++)
                        {
                                if (!item.list[i].locked && item.list[i].notDead)
                                {
                                        item.index = i;
                                        Swap(item.index);
                                        return;
                                }

                        }
                        for (int i = 0; i < item.index; i++)
                        {
                                if (i >= item.list.Count)
                                {
                                        return;
                                }

                                if (!item.list[i].locked && item.list[i].notDead)
                                {
                                        item.index = i;
                                        Swap(item.index);
                                        return;
                                }
                        }

                        onFailedToSwap.Invoke();
                }

                public void Swap (int newIndex)
                {
                        for (int i = 0; i < item.list.Count; i++)
                        {
                                if (item.list[i].character == null || !item.list[i].character.gameObject.activeInHierarchy)
                                {
                                        continue;
                                }

                                for (int j = 0; j < item.list.Count; j++)
                                {
                                        if (i == j || j != newIndex || item.list[j].character == null || item.list[j].locked)
                                        {
                                                continue;
                                        }
                                        if (item.list[i].character != item.list[j].character)
                                        {
                                                Swap(i, j);
                                        }
                                        return;

                                }
                        }
                }

                public void Swap (string name)
                {
                        for (int i = 0; i < item.list.Count; i++)
                        {
                                if (item.list[i].character == null || !item.list[i].character.gameObject.activeInHierarchy)
                                {
                                        continue;
                                }

                                for (int j = 0; j < item.list.Count; j++)
                                {
                                        if (i == j || item.list[j].name != name || item.list[j].character == null || item.list[j].locked)
                                        {
                                                continue;
                                        }
                                        if (item.list[i].character != item.list[j].character)
                                        {
                                                Swap(i, j);
                                        }
                                        return;
                                }
                        }
                }

                public void Swap (int i, int j)
                {
                        item.index = j;
                        Character oldChar = item.list[i].character;
                        Character newChar = item.list[j].character;
                        newChar.transform.position = oldChar.transform.position;
                        oldChar.gameObject.SetActive(false);
                        newChar.gameObject.SetActive(true);
                        item.list[i].onDeactivate.Invoke();
                        item.list[j].onActivate.Invoke();
                        if (onSwapEventActive)
                        {
                                onSwap.Invoke(ImpactPacket.impact.Set(swapWE, newChar.transform.position, Vector2.down));
                        }
                        ThePlayer.Player oldPlayer = oldChar.GetComponent<ThePlayer.Player>();
                        ThePlayer.Player player = newChar.GetComponent<ThePlayer.Player>();
                        if (player != null)
                        {
                                ThePlayer.Player.mainPlayer = player;
                        }
                        if (player != null && oldPlayer != null) /// try to keep same air state
                        {
                                player.abilities.velocity.y = oldPlayer.abilities.velocity.y;
                                player.abilities.world.onGround = oldPlayer.world.onGround;
                                player.abilities.world.onCeiling = oldPlayer.world.onCeiling;
                                player.abilities.onSurface = oldPlayer.abilities.onSurface;
                        }
                        if (Safire2DCamera.Safire2DCamera.mainCamera != null)
                        {
                                Safire2DCamera.Safire2DCamera.mainCamera.ChangeTargetTransform(newChar.transform);
                        }
                }

                public void Unlock (string name)
                {
                        for (int i = 0; i < item.list.Count; i++)
                        {
                                if (item.list[i].name == name)
                                {
                                        item.list[i].locked = false;
                                        item.list[i].LockedEvents();
                                        Save();
                                        return;
                                }
                        }
                }

                public void UnlockAndSwap (string name)
                {
                        for (int i = 0; i < item.list.Count; i++)
                        {
                                if (item.list[i].name == name)
                                {
                                        item.list[i].locked = false;
                                        item.list[i].LockedEvents();
                                        Swap(name);
                                        Save();
                                        return;
                                }
                        }
                }

                public void Load ()
                {
                        // swapping item order in inspector will break the save order
                        temp = Storage.Load<CharacterSwapItems>(temp, WorldManager.saveFolder, "CharacterSwap");
                        item.index = temp.index;
                        for (int i = 0; i < temp.list.Count; i++)
                        {
                                if (i < item.list.Count)
                                {
                                        item.list[i].locked = temp.list[i].locked;
                                }
                        }
                }

                public void Save ()
                {
                        Storage.Save(item, WorldManager.saveFolder, "CharacterSwap");
                }
        }

        [System.Serializable]
        public class CharacterSwapItems
        {
                [SerializeField] public int index;
                [SerializeField] public List<CharacterSwapItem> list = new List<CharacterSwapItem>();

                public void Initialize ()
                {
                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i].character != null)
                                {
                                        list[i].health = list[i].character.GetComponent<Health>();
                                }
                                list[i].LockedEvents();
                        }
                }
        }

        [System.Serializable]
        public class CharacterSwapItem
        {
                [SerializeField] public Character character;
                [SerializeField] public string name;
                [SerializeField] public bool locked;
                [SerializeField] public UnityEvent onActivate;
                [SerializeField] public UnityEvent onDeactivate;
                [SerializeField] public UnityEvent isLocked;
                [SerializeField] public UnityEvent isUnlocked;

                [System.NonSerialized] public Health health;
                public bool notDead => health == null || health.GetValue() > 0;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private bool activateFoldOut;
                [SerializeField, HideInInspector] private bool deactivateFoldOut;
                [SerializeField, HideInInspector] private bool lockedFoldOut;
                [SerializeField, HideInInspector] private bool unlockedFoldOut;
#pragma warning restore 0414
#endif
                #endregion

                public void LockedEvents ()
                {
                        if (locked)
                                isLocked.Invoke();
                        if (!locked)
                                isUnlocked.Invoke();
                }
        }
}
