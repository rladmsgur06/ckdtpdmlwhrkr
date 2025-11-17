using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        [System.Serializable]
        public class PushZone
        {
                [SerializeField] public float zoneX;
                [SerializeField] public float zoneY;
                [SerializeField] public PushHorizontal horizontal;
                [SerializeField] public PushVertical vertical;

                [System.NonSerialized] private float counterX = 0;
                [System.NonSerialized] private float counterY = 0;
                [System.NonSerialized] private const float timeLimit = 1.25f;

                public bool right => horizontal == PushHorizontal.PushRight;
                public bool left => horizontal == PushHorizontal.PushLeft;
                public bool down => vertical == PushVertical.PushDown;
                public bool up => vertical == PushVertical.PushUp;

                public bool enabledX => horizontal != PushHorizontal.DontPush;
                public bool enabledY => vertical != PushVertical.DontPush;

                public Vector2 Velocity (Vector2 target, Vector2 cameraVelocity, Camera camera, bool isUser)
                {
                        if (!enabledX && !enabledY)
                        {
                                return cameraVelocity;
                        }

                        Vector3 pushVelocity = new Vector3 (enabledX ? 0 : cameraVelocity.x, enabledY ? 0 : cameraVelocity.y);
                        Vector2 cameraPosition = camera.transform.position;
                        Vector2 size = new Vector2 (camera.Width ( ) * zoneX, camera.Height ( ) * zoneY);

                        if (isUser)
                        {
                                if ((right && cameraVelocity.x > 0) || (left && cameraVelocity.x < 0)) pushVelocity.x = cameraVelocity.x;
                                if ((up && cameraVelocity.y > 0) || (down && cameraVelocity.y < 0)) pushVelocity.y = cameraVelocity.y;
                                return pushVelocity;
                        }

                        if (right && target.x >= (cameraPosition.x + size.x))
                        {
                                TwoBitMachines.Clock.TimerExpired (ref counterX, timeLimit);
                                pushVelocity.x = (target.x - (cameraPosition.x + size.x)) * (counterX / timeLimit); // ease into 
                        }
                        else if (left && target.x <= (cameraPosition.x - size.x))
                        {
                                TwoBitMachines.Clock.TimerExpired (ref counterX, timeLimit);
                                pushVelocity.x = (target.x - (cameraPosition.x - size.x)) * (counterX / timeLimit);
                        }
                        else
                                counterX = 0;

                        if (up && target.y >= (cameraPosition.y + size.y))
                        {
                                TwoBitMachines.Clock.TimerExpired (ref counterY, timeLimit);
                                pushVelocity.y = (target.y - (cameraPosition.y + size.y)) * (counterY / timeLimit);
                        }
                        else if (down && target.y <= (cameraPosition.y - size.y))
                        {
                                TwoBitMachines.Clock.TimerExpired (ref counterY, timeLimit);
                                pushVelocity.y = (target.y - (cameraPosition.y - size.y)) * (counterY / timeLimit);
                        }
                        else
                                counterY = 0;

                        return pushVelocity;
                }
        }

        public enum PushHorizontal
        {
                DontPush,
                PushLeft,
                PushRight
        }

        public enum PushVertical
        {
                DontPush,
                PushUp,
                PushDown
        }
}