using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TwoBitMachines.FlareEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TwoBitMachines.MapSystem
{
        [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
        public class Map : MonoBehaviour
        {
                [SerializeField] public MapSO map;
                [SerializeField] public Transform player;
                [SerializeField] public GameObject mapObject;
                [SerializeField] public GameObject playerIcon;

                [SerializeField] public UnityEvent onOpen;
                [SerializeField] public UnityEvent onClose;
                [SerializeField] public UnityEventVector3 onRoomUnlocked;

                [SerializeField] public Mesh mesh;
                [SerializeField] public MeshFilter meshFilter;
                [SerializeField] public MeshRenderer meshRenderer;
                [SerializeField] public List<ZoneEvents> eventList = new List<ZoneEvents>();

                [System.NonSerialized] private string activeZone;
                [System.NonSerialized] private bool initialized = false;
                [System.NonSerialized] private int currentRoomIndex = -1;

#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] public bool eventFoldOut;
                [SerializeField] public bool openFoldOut;
                [SerializeField] public bool closeFoldOut;
                [SerializeField] public bool roomUnlockedFoldOut;
#pragma warning restore 0414
#endif

                private void Awake ()
                {
                        mesh = mesh == null ? new Mesh() : mesh;
                        mesh.Clear();
                        meshFilter.mesh = null;  // clear initial mesh so it doesn't show on first frame
                        if (playerIcon != null)
                        {
                                playerIcon.SetActive(false);
                        }
                        if (WorldManager.get.initialized)
                        {
                                Initialized();
                        }
                }

                private void Start ()
                {
                        if (!initialized)
                        {
                                Initialized();
                        }
                }

                private void Initialized ()
                {
                        if (map == null)
                                return;

                        GetZone();
                        mesh = mesh == null ? new Mesh() : mesh;
                        map.Restore();
                        UpdateMesh(true);
                        InitializeInput();
                        initialized = true;
                }

                private void GetZone ()
                {
                        string currentSceneName = SceneManager.GetActiveScene().name;
                        for (int i = 0; i < map.zoneList.Count; i++)
                        {
                                if (map.zoneList[i].sceneName == currentSceneName)
                                {
                                        activeZone = map.zoneList[i].name;
                                        return;
                                }
                        }
                }

                public void InitializeInput ()
                {
                        WorldManager.get.update -= RunMap;
                        WorldManager.get.update += RunMap;
                }

                private void OnEnable ()
                {
                        if (map != null && map.pauseGame == MapPause.PauseGame)
                        {
                                WorldManager.get.PauseNoInvoke();
                        }
                        onOpen.Invoke();
                }

                private void OnDisable ()
                {
                        if (map != null && map.pauseGame == MapPause.PauseGame)
                        {
                                WorldManager.get.UnpauseNoInvoke();
                        }
                        onClose.Invoke();
                }

                public void RunMap (bool gameReset = false)
                {
                        if (map != null && map.openInput != null && mapObject != null && map.openInput.Released())
                        {
                                mapObject.SetActive(!mapObject.activeInHierarchy);
                        }
                        UnlockRoom();
#if UNITY_EDITOR
                        if (!EditorApplication.isPlayingOrWillChangePlaymode)
                        {
                                for (int i = 0; i < map.zoneList.Count; i++)
                                {
                                        map.zoneList[i].RestoreOriginalData();
                                }
                        }
#endif
                }

                public void SetMeshRef ()
                {
                        mesh = mesh == null ? new Mesh() : mesh;
                        meshFilter = meshFilter == null ? gameObject.GetComponent<MeshFilter>() : meshFilter;
                        meshRenderer = meshRenderer == null ? gameObject.GetComponent<MeshRenderer>() : meshRenderer;
                }

                public void UpdateMesh (bool openRoomLists)
                {
                        SetMeshRef();

                        if (map != null)
                        {
                                map.UpdateMesh(mesh, meshFilter, meshRenderer, eventList, openRoomLists);
                        }
                }

                private void UnlockRoom ()
                {
                        if (player == null)
                                return;

                        Vector2 target = player.position + Vector3.up * 0.1f;
                        for (int z = 0; z < map.zoneList.Count; z++)
                        {
                                Zone zone = map.zoneList[z];
                                if (zone.name != activeZone)
                                {
                                        continue;
                                }
                                for (int r = 0; r < zone.roomList.Count; r++)
                                {
                                        if (MapSO.IsPointInPoly(zone.roomList[r].pointList, target))
                                        {
                                                if (map.trackPlayerRealTime)
                                                {
                                                        playerIcon.transform.position = player.position + (Vector3) zone.roomList[r].offset + new Vector3(0, map.playerOffsetY, 0);
                                                        if (playerIcon != null && !playerIcon.activeInHierarchy)
                                                        {
                                                                playerIcon.SetActive(true);
                                                        }
                                                }
                                                if (currentRoomIndex != r)
                                                {
                                                        UnlockRoom(zone.roomList[r], z, r);
                                                        currentRoomIndex = r;
                                                        return;
                                                }

                                        }
                                }
                        }
                }

                public void UnlockRoom (string roomID)
                {
                        string[] parts = roomID.Split('_');
                        if (parts.Length != 2 || map == null)
                        {
                                return;
                        }

                        string zoneName = parts[0];
                        int roomNumber = int.Parse(parts[1]);

                        for (int i = 0; i < map.zoneList.Count; i++)
                        {
                                if (map.zoneList[i].name == zoneName && roomNumber >= 0 && roomNumber < map.zoneList[i].roomList.Count)
                                {
                                        UnlockRoom(map.zoneList[i].roomList[roomNumber], i, roomNumber);
                                        return;
                                }
                        }
                }

                public void UnlockRoom (Room room, int zoneIndex, int roomIndex)
                {
                        bool unlocked = room.visible;
                        room.visible = true;
                        if (playerIcon != null && !map.trackPlayerRealTime)
                        {
                                playerIcon.transform.position = room.playerIcon + room.offset;
                                if (playerIcon != null && !playerIcon.activeInHierarchy)
                                {
                                        playerIcon.SetActive(true);
                                }
                        }
                        if (!unlocked)
                        {
                                map.Save();
                                UpdateMesh(false);
                                if (zoneIndex < eventList.Count && roomIndex < eventList[zoneIndex].list.Count)
                                {
                                        eventList[zoneIndex].list[roomIndex].Open(room);
                                }
                        }
                        onRoomUnlocked.Invoke(room.playerIcon + room.offset);
                        return;
                }
        }

        [System.Serializable]
        public class SaveZoneList
        {
                public List<SaveZone> list = new List<SaveZone>();
        }

        [System.Serializable]
        public struct SaveZone
        {
                public string name;
                public List<bool> list;
        }

        [System.Serializable]
        public class ZoneEvents
        {
                public List<RoomEvents> list = new List<RoomEvents>();
        }

        [System.Serializable]
        public class RoomEvents
        {
                public List<GameObject> list = new List<GameObject>();

                public void Open (Room room)
                {
                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i] != null)
                                {
                                        list[i].SetActive(true);
                                        list[i].transform.position += (Vector3) room.offset;
                                }
                        }
                }
        }

        public enum MapPause
        {
                PauseGame,
                LeaveAsIs
        }
}
