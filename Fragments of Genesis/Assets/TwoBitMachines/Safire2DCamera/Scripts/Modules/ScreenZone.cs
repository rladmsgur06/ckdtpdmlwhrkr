using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        [System.Serializable]
        public class ScreenZone
        {
                [SerializeField] public Vector2 size;
                [System.NonSerialized] public Vector2 origin;
                public bool surpassedZoneX { get; private set; }
                public bool surpassedZoneY { get; private set; }

                public void Reset ( )
                {
                        surpassedZoneX = false;
                        surpassedZoneY = false;
                }

                public Vector3 Velocity (Vector2 target, Camera camera, bool isUser)
                {
                        if (this.size == Vector2.zero || isUser) return Vector3.zero;

                        Reset ( );
                        Vector2 velocityClamp = Vector3.zero;
                        Vector2 cameraPosition = camera.transform.position;
                        Vector2 zone = new Vector2 (camera.Width ( ) * size.x, camera.Height ( ) * size.y);

                        if (zone.x > 0 && target.x < (cameraPosition.x - zone.x)) { surpassedZoneX = true; velocityClamp.x = target.x - (cameraPosition.x - zone.x); }
                        if (zone.x > 0 && target.x > (cameraPosition.x + zone.x)) { surpassedZoneX = true; velocityClamp.x = target.x - (cameraPosition.x + zone.x); }
                        if (zone.y > 0 && target.y < (cameraPosition.y - zone.y)) { surpassedZoneY = true; velocityClamp.y = target.y - (cameraPosition.y - zone.y); }
                        if (zone.y > 0 && target.y > (cameraPosition.y + zone.y)) { surpassedZoneY = true; velocityClamp.y = target.y - (cameraPosition.y + zone.y); }
                        return velocityClamp;
                }

                // public void Clamp (ref Vector2 target, ref Vector2 position, Camera camera)
                // {
                //         Vector2 cameraPosition = camera.transform.position;
                //         Vector2 zone = new Vector2 (camera.Width ( ) * size.x, camera.Height ( ) * size.y);
                //         if (zone.x > 0 && target.x < (cameraPosition.x - zone.x)) position.x = (cameraPosition.x - zone.x);
                //         if (zone.x > 0 && target.x > (cameraPosition.x + zone.x)) position.x = (cameraPosition.x + zone.x);
                //         if (zone.y > 0 && target.y < (cameraPosition.y - zone.y)) position.y = (cameraPosition.y - zone.y);
                //         if (zone.y > 0 && target.y > (cameraPosition.y + zone.y)) position.y = (cameraPosition.y + zone.y);
                // }
        }
}