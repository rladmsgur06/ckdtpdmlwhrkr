using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine
{
        public class OverworldNode : MonoBehaviour
        {
                [SerializeField] public OverworldNodeType type;
                [SerializeField] public string sceneName = "SceneName";
                [SerializeField] public string blockSaveKey;
                [SerializeField] public string signal = "";
                [SerializeField] public string unlockKey = "";
                [SerializeField] public string nodeName = "";
                [SerializeField] public float signalTime = 1f;
                [SerializeField] public SetNextNodeType setNextNodeType;
                [SerializeField] public Sprite imageLocked;
                [SerializeField] public Sprite imageUnlocked;

                [SerializeField] public UnityEvent onUnlock;
                [SerializeField] public UnityEvent onTeleport;
                [SerializeField] public UnityEvent isBlocked;
                [SerializeField] public UnityEvent isUnblocked;
                [SerializeField] public UnityEvent signalComplete;
                [SerializeField] public UnityEventOverworldNode onEnterNode;
                [SerializeField] public UnityEventOverworldNode onExitNode;

                [SerializeField] public OverworldNode teleportToNode;
                [SerializeField] public OverworldNode nextNode;

                [SerializeField] public bool unlocked = false;
                [SerializeField] private SpriteRenderer rendererRef;
                [SerializeField] private SaveBool saveBool = new SaveBool();
                [SerializeField] public List<OverworldNode> path = new List<OverworldNode>();
                [SerializeField] public List<OverworldNode> blockPath = new List<OverworldNode>();

                public bool isLevel => type == OverworldNodeType.Level;
                public bool isBlock => type == OverworldNodeType.Block;
                public bool isTeleport => type == OverworldNodeType.Teleport;
                public bool canBeLocked => isLevel || isBlock;
                public Vector2 position => transform.position;
                public static List<Texture2D> icon;

                public float G { get; set; }
                public float H { get; set; }
                public float F { get { return G + H; } }
                public OverworldNode Parent { get; set; }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] public bool foldOut;
                [SerializeField] public bool enterFoldOut;
                [SerializeField] public bool onEnterFoldOut;
                [SerializeField] public bool onExitFoldOut;
                [SerializeField] public bool unlockFoldOut;
                [SerializeField] public bool isBlockedFoldOut;
                [SerializeField] public bool isUnblockedFoldOut;
                [SerializeField] public bool signalFoldOut;
                [SerializeField] public bool teleportFoldOut;
                [SerializeField] public float timeStamp = 2f;

                public void OnDrawGizmos ()
                {
                        if (icon == null || icon.Count == 0) // || Application.isPlaying)
                        {
                                return;
                        }
                        Texture2D iconTexture = null;//

                        if (isLevel)
                        {
                                iconTexture = icon.GetIcon("LockRed");
                        }
                        else if (isBlock)
                        {
                                iconTexture = icon.GetIcon("Stop");
                        }
                        else if (isTeleport)
                        {
                                iconTexture = icon.GetIcon("TeleportYellow");
                        }
                        else
                        {
                                return;
                        }

                        float iconSize = 1f;
                        Vector2 position = this.position + Vector2.up * -1f;
                        Rect iconRect = new Rect(position.x, position.y, iconSize, -iconSize);
                        Gizmos.DrawGUITexture(iconRect, iconTexture);
                }
#pragma warning restore 0414
#endif
                #endregion

                public void Awake ()
                {
                        if (rendererRef == null)
                        {
                                rendererRef = this.gameObject.GetComponent<SpriteRenderer>();
                        }
                }

                public void Start ()
                {
                        if (canBeLocked)
                        {
                                if (isLevel)
                                {
                                        saveBool.value = false;
                                        unlocked = Storage.Load<SaveBool>(saveBool, WorldManager.saveFolder, sceneName).value;
                                }
                                if (isBlock)
                                {
                                        saveBool.value = false;
                                        unlocked = Storage.Load<SaveBool>(saveBool, WorldManager.saveFolder, blockSaveKey).value;
                                        if (unlocked)
                                        {
                                                isUnblocked.Invoke();
                                        }
                                        else
                                        {

                                                isBlocked.Invoke();
                                        }
                                }
                                SetImage();
                        }
                }

                public void UnlockBlock ()
                {
                        if (isBlock)
                        {
                                unlocked = true;
                                saveBool.value = true;
                                Storage.Save(saveBool, WorldManager.saveFolder, blockSaveKey);
                                onUnlock.Invoke();
                                SetImage();
                        }
                }

                private void SetImage ()
                {
                        if (rendererRef != null && canBeLocked)
                        {
                                rendererRef.sprite = IsLocked() ? imageLocked : imageUnlocked;
                        }
                }

                public bool IsLocked ()
                {
                        return canBeLocked ? !unlocked : false;
                }

                public bool PathBlocked (OverworldNode otherNode)
                {
                        return otherNode.IsLocked() || blockPath.Contains(otherNode);
                }
        }

        public enum OverworldNodeType
        {
                Basic,
                Level,
                Block,
                Teleport,
                HasItem,
                Start
        }

        public enum SetNextNodeType
        {
                No,
                TeleportTo,
                MoveTo
        }

        [System.Serializable]
        public class UnityEventOverworldNode : UnityEvent<OverworldNode> { }
}
