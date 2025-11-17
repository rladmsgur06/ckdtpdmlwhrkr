#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        [System.Serializable]
        public class PeekPlus
        {
                [SerializeField] public bool enable;
                [SerializeField] public bool ignoreClamps;
                [SerializeField] public float speed = 10f;
                [SerializeField] public Vector2 distance = new Vector2(5f, 5f);
                [SerializeField] public ControllerType controllerTypeX;
                [SerializeField] public ControllerType controllerTypeY;

                [SerializeField] public InputButtonSO inputLeft;
                [SerializeField] public InputButtonSO inputRight;
                [SerializeField] public InputButtonSO inputUp;
                [SerializeField] public InputButtonSO inputDown;
                [SerializeField] public InputButtonSO horizontalStick;
                [SerializeField] public InputButtonSO verticalStick;

                [System.NonSerialized] private float distanceLeft;
                [System.NonSerialized] private float distanceRight;
                [System.NonSerialized] private float distanceUp;
                [System.NonSerialized] private float distanceDown;
                [System.NonSerialized] private bool xActive;
                [System.NonSerialized] private bool yActive;

                public enum ControllerType { Buttons, Stick }

                public void Reset ()
                {
                        distanceLeft = 0;
                        distanceRight = 0;
                        distanceUp = 0;
                        distanceDown = 0;
                        xActive = false;
                        yActive = false;
                }

                public Vector3 Velocity (Follow follow, bool isUser)
                {
                        if (!enable || ignoreClamps || isUser || follow.usingAutoRail)
                                return Vector3.zero;

                        return Move(follow);
                }

                public void Velocity (Camera camera, Follow follow)
                {
                        if (!enable || !ignoreClamps || follow.isUser || follow.highlightTarget.active || follow.usingAutoRail)
                                return;

                        Vector3 appliedPeek = Move(follow);
                        follow.SetCameraPosition(camera.transform.position + appliedPeek);
                }

                private Vector3 Move (Follow follow)
                {
                        float signLeft, signRight, signUp, signDown;

                        if (controllerTypeX == ControllerType.Buttons)
                        {
                                signLeft = inputLeft != null && inputLeft.Holding() ? -1 : 1 * 1.5f;
                                signRight = inputRight != null && inputRight.Holding() ? 1 : -1 * 1.5f;
                        }
                        else
                        {
                                Vector2 directionX = horizontalStick != null && horizontalStick.Holding() ? horizontalStick.valueV2 : Vector2.zero;
                                signLeft = directionX.x < 0 ? -1 : 1 * 1.5f;
                                signRight = directionX.x > 0 ? 1 : -1 * 1.5f;
                        }

                        if (controllerTypeY == ControllerType.Buttons)
                        {
                                signUp = inputUp != null && inputUp.Holding() ? 1 : -1 * 1.5f;
                                signDown = inputDown != null && inputDown.Holding() ? -1 : 1 * 1.5f;
                        }
                        else
                        {
                                Vector2 directionY = verticalStick != null && verticalStick.Holding() ? verticalStick.valueV2 : Vector2.zero;
                                signUp = directionY.y > 0 ? 1 : -1 * 1.5f;
                                signDown = directionY.y < 0 ? -1 : 1 * 1.5f;
                        }

                        distanceLeft = Mathf.Clamp(distanceLeft + Time.deltaTime * speed * signLeft, -distance.x, 0);
                        distanceRight = Mathf.Clamp(distanceRight + Time.deltaTime * speed * signRight, 0, distance.x);
                        distanceUp = Mathf.Clamp(distanceUp + Time.deltaTime * speed * signUp, 0, distance.y);
                        distanceDown = Mathf.Clamp(distanceDown + Time.deltaTime * speed * signDown, -distance.y, 0);

                        Vector2 appliedPeek = new Vector2(distanceLeft + distanceRight, distanceUp + distanceDown);
                        if (appliedPeek.x != 0)
                                xActive = true;
                        if (appliedPeek.y != 0)
                                yActive = true;

                        follow.ForceTargetClamp(appliedPeek.x != 0, appliedPeek.y != 0);

                        if (xActive && appliedPeek.x == 0)
                        {
                                xActive = false;
                                follow.ForceTargetSmooth(y: false);
                        }
                        if (yActive && appliedPeek.y == 0)
                        {
                                yActive = false;
                                follow.ForceTargetSmooth(y: false);
                        }
                        return appliedPeek;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
                #pragma warning disable format, 0414
                #if UNITY_EDITOR
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool close;
                [SerializeField, HideInInspector] public bool edit;
                [SerializeField, HideInInspector] public bool add;

                public static void CustomInspector (SerializedProperty parent, Color barColor, Color labelColor)
                {
                        if (!parent.Bool("edit"))
                                return;

                        if (Follow.Open(parent, "Peek Plus", barColor, labelColor))
                        {
                                GUI.enabled = parent.Bool("enable");
                                ControllerType typeX = (ControllerType)parent.Enum("controllerTypeX");
                                ControllerType typeY = (ControllerType) parent.Enum("controllerTypeY");

                                FoldOut.Box(3, Tint.Box);
                                parent.Field("Distance", "distance");
                                parent.Field("Speed", "speed");
                                parent.Field("Ignore Clamps", "ignoreClamps");
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(4, Tint.Box);
                                
                                parent.Field("Input X","controllerTypeX");
                                if(typeX == ControllerType.Buttons)
                                        parent.FieldDouble("Left, Right", "inputLeft", "inputRight");
                                else
                                        parent.Field("Horizontal Stick", "horizontalStick");

                                parent.Field("Input Y", "controllerTypeY");
                                if (typeY == ControllerType.Buttons)
                                        parent.FieldDouble("Up, Down","inputUp", "inputDown");
                                else
                                        parent.Field("Vertical Stick", "verticalStick");

                                Layout.VerticalSpacing(5);
                                GUI.enabled = true;
                        };
                }
                #pragma warning restore format, 0414
                #endif
                #endregion
        }
}
