using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TwoBitMachines.FlareEngine;

namespace TwoBitMachines.MapSystem
{
        [CreateAssetMenu(menuName = "FlareEngine/MapSO")]
        public class MapSO : ScriptableObject
        {
                [SerializeField] public string mapName;
                [SerializeField] public MapPause pauseGame;
                [SerializeField] public InputButtonSO openInput;

                [SerializeField] public MeshData meshData = new MeshData();
                [SerializeField] public List<Zone> zoneList = new List<Zone>();
                [SerializeField] private SaveZoneList saveZones = new SaveZoneList();

                [SerializeField] public MapEditMode editMode;
                [SerializeField] public float playerOffsetY;
                [SerializeField] public bool trackPlayerRealTime;
                [SerializeField] public float snapSize = 0.25f;
                [SerializeField] public float gizmoSize = 0.5f;
                [SerializeField] public int zoneIndex;

                public string key => "Flare Map " + mapName;
                public static List<string> zoneNames = new List<string>();

#if UNITY_EDITOR
                private void OnValidate ()
                {
                        for (int i = 0; i < zoneList.Count; i++)
                        {
                                zoneList[i].SaveOriginalData();
                        }
                }

                public void UpdateMeshEditor (Mesh mesh, MeshFilter meshFilter, MeshRenderer meshRenderer)
                {
                        meshData.Clear();
                        for (int i = 0; i < zoneList.Count; i++)
                        {
                                Zone zone = zoneList[i];
                                if (i != zoneIndex)
                                {
                                        continue;
                                }
                                for (int j = 0; j < zone.roomList.Count; j++)
                                {
                                        Room room = zone.roomList[j];
                                        if (room.opacity)
                                        {
                                                continue;
                                        }
                                        meshData.Add(room.roomMesh);
                                }
                        }

                        for (int i = 0; i < zoneList.Count; i++)
                        {
                                Zone zone = zoneList[i];
                                if (i != zoneIndex)
                                {
                                        continue;
                                }
                                for (int j = 0; j < zone.roomList.Count; j++)
                                {
                                        Room room = zone.roomList[j];
                                        if (room.opacity)
                                        {
                                                continue;
                                        }
                                        meshData.Add(room.outlineMesh);
                                }
                        }

                        meshData.Set(mesh, meshFilter); // this sets the mesh filter
                        mesh.RecalculateBounds();
                }

#endif

                public void UpdateMeshEditorShowAll (Mesh mesh, MeshFilter meshFilter, MeshRenderer meshRenderer)
                {
                        meshData.Clear();
                        for (int i = 0; i < zoneList.Count; i++)
                        {
                                Zone zone = zoneList[i];
                                for (int j = 0; j < zone.roomList.Count; j++)
                                {
                                        Room room = zone.roomList[j];
                                        meshData.Add(room.roomMesh, room.offset);
                                }
                        }

                        for (int i = 0; i < zoneList.Count; i++)
                        {
                                Zone zone = zoneList[i];
                                for (int j = 0; j < zone.roomList.Count; j++)
                                {
                                        Room room = zone.roomList[j];
                                        meshData.Add(room.outlineMesh, room.offset);
                                }
                        }

                        meshData.Set(mesh, meshFilter); // this sets the mesh filter
                        mesh.RecalculateBounds();
                }

                public void UpdateMesh (Mesh mesh, MeshFilter meshFilter, MeshRenderer meshRenderer, List<ZoneEvents> eventList, bool openRoomLists)
                {
                        meshData.Clear();
                        for (int i = 0; i < zoneList.Count; i++)
                        {
                                Zone zone = zoneList[i];
                                for (int j = 0; j < zone.roomList.Count; j++)
                                {
                                        Room room = zone.roomList[j];
                                        if (room.visible)
                                        {
                                                meshData.Add(room.roomMesh, room.offset);
                                                if (openRoomLists && i < eventList.Count && j < eventList[i].list.Count)
                                                {
                                                        eventList[i].list[j].Open(room);
                                                }
                                        }
                                }
                        }

                        for (int i = 0; i < zoneList.Count; i++)
                        {
                                Zone zone = zoneList[i];
                                for (int j = 0; j < zone.roomList.Count; j++)
                                {
                                        Room room = zone.roomList[j];
                                        if (room.visible)
                                        {
                                                meshData.Add(room.outlineMesh, room.offset);
                                        }
                                }
                        }

                        meshData.Set(mesh, meshFilter); // this sets the mesh filter
                        mesh.RecalculateBounds();
                }

                public void Save ()
                {
                        saveZones.list.Clear();
                        for (int i = 0; i < zoneList.Count; i++)
                        {
                                Zone zone = zoneList[i];
                                SaveZone saveZone = new SaveZone();
                                saveZone.name = zone.name;
                                saveZone.list = new List<bool>();
                                saveZones.list.Add(saveZone);

                                for (int j = 0; j < zone.roomList.Count; j++)
                                {
                                        saveZone.list.Add(zone.roomList[j].visible);
                                }
                        }

                        Storage.Save(saveZones, WorldManager.saveFolder, key);
                }

                public void Restore ()
                {
                        saveZones.list.Clear();
                        saveZones = Storage.Load<SaveZoneList>(saveZones, WorldManager.saveFolder, key);

                        for (int i = 0; i < saveZones.list.Count; i++)
                        {
                                SaveZone saveZone = saveZones.list[i];

                                for (int j = 0; j < zoneList.Count; j++)
                                {
                                        if (zoneList[j].name == saveZone.name)
                                        {
                                                for (int k = 0; k < zoneList[j].roomList.Count && k < saveZone.list.Count; k++)
                                                {
                                                        zoneList[j].roomList[k].visible = saveZone.list[k];
                                                }
                                        }
                                }
                        }
                }

                public List<string> ZoneNames ()
                {
                        zoneNames.Clear();
                        for (int i = 0; i < zoneList.Count; i++)
                                zoneNames.Add(zoneList[i].name);
                        return zoneNames;
                }

                public static bool IsPointInPoly (List<Point> points, Vector2 point)
                {
                        bool isInside = false;
                        int polyCorners = points.Count;
                        int j = polyCorners - 1;

                        for (int i = 0; i < polyCorners; i++)
                        {
                                Vector2 a = points[j].position;
                                Vector2 b = points[i].position;

                                if (((a.y > point.y) != (b.y > point.y)) && (point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x))
                                {
                                        isInside = !isInside;
                                }
                                j = i;
                        }
                        return isInside;
                }

                public static bool IsPointInPoly (List<Point> points, Vector2 offset, Vector2 point)
                {
                        bool isInside = false;
                        int polyCorners = points.Count;
                        int j = polyCorners - 1;

                        for (int i = 0; i < polyCorners; i++)
                        {
                                Vector2 a = points[j].position + offset;
                                Vector2 b = points[i].position + offset;

                                if (((a.y > point.y) != (b.y > point.y)) && (point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x))
                                {
                                        isInside = !isInside;
                                }
                                j = i;
                        }
                        return isInside;
                }
        }

        public enum MapEditMode
        {
                EditZones,
                EditMap
        }
}
