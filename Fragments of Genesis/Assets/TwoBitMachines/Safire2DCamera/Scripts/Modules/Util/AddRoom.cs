using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        public class AddRoom : MonoBehaviour
        {
                [SerializeField] public Room room = new Room();
                [SerializeField] public Safire2DCamera safireCamera;

                private void Start ()
                {
                        safireCamera = safireCamera == null ? Safire2DCamera.mainCamera : safireCamera;

                        if (safireCamera == null)
                        {
                                return;
                        }

                        room.bounds.Initialize();
                        room.multipleTargets.Initialize(safireCamera.zoom, room.bounds);
                        safireCamera.rooms.rooms.Add(room);
                        int index = safireCamera.rooms.rooms.Count - 1;

                        if (!safireCamera.rooms.enable)
                        {
                                return;
                        }

                        Vector2 targetPosition = safireCamera.follow.TargetPosition();
                        if (room.bounds.DetectBounds(targetPosition))
                        {
                                safireCamera.rooms.EnterRoom(room, targetPosition, index);
                        }
                }
        }
}
