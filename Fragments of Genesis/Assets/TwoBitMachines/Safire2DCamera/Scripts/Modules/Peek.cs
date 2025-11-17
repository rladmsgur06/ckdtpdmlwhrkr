#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace TwoBitMachines.Safire2DCamera
{
        [System.Serializable]
        public class Peek
        {
                [SerializeField] public bool enable;
                [SerializeField] public bool ignoreClamps;
                [SerializeField] public float time = 0.25f;
                [SerializeField] public float distance = 5f;

                [SerializeField] public KeyCode buttonA = KeyCode.W;
                [SerializeField] public KeyCode buttonB = KeyCode.S;
                [SerializeField] public Vector2 directionA = Vector2.up;
                [SerializeField] public Vector2 directionB = Vector2.down;

                [System.NonSerialized] private float distanceLeft;
                [System.NonSerialized] private float distanceRight;
                [System.NonSerialized] private float distanceUp;
                [System.NonSerialized] private float distanceDown;
                [System.NonSerialized] private bool xActive;
                [System.NonSerialized] private bool yActive;

                [System.NonSerialized] private bool goLeft, goRight, goUp, goDown;

                public enum MousePeekType
                {
                        Button,
                        Automatic
                }

                public void Reset ()
                {
                        distanceLeft = 0;
                        distanceRight = 0;
                        distanceUp = 0;
                        distanceDown = 0;
                        xActive = false;
                        yActive = false;

                        goLeft = false;
                        goRight = false;
                        goUp = false;
                        goDown = false;
                }

                public void Velocity (Camera camera, Follow follow)
                {
                        if (!enable || !ignoreClamps || follow.isUser || follow.highlightTarget.active || follow.usingAutoRail)
                                return;

                        Vector3 appliedPeek = Move(follow);
                        follow.SetCameraPosition(camera.transform.position + appliedPeek);
                }

                public Vector3 Velocity (Follow follow, bool isUser)
                {
                        if (!enable || ignoreClamps || isUser || follow.usingAutoRail)
                                return Vector3.zero;

                        return Move(follow);
                }

                private Vector3 Move (Follow follow)
                {
                        float signLeft, signRight, signUp, signDown;
                        float speedUp = time == 0 ? 0 : Mathf.Abs((directionA.y * distance) / time);
                        float speedDown = time == 0 ? 0 : Mathf.Abs((directionB.y * distance) / time);
                        float speedLeft = time == 0 ? 0 : Mathf.Abs((directionA.x * distance) / time);
                        float speedRight = time == 0 ? 0 : Mathf.Abs((directionB.x * distance) / time);

                        if (directionA.x < 0)
                        {
                                signLeft = Input.GetKey(buttonA) || goLeft ? -1 : 1 * 1.5f;
                                distanceLeft = Mathf.Clamp(distanceLeft + Time.deltaTime * speedLeft * signLeft, directionA.x * distance, 0);
                        }
                        else if (directionA.x > 0)
                        {
                                signLeft = Input.GetKey(buttonA) || goLeft ? 1 : -1 * 1.5f;
                                distanceLeft = Mathf.Clamp(distanceLeft + Time.deltaTime * speedLeft * signLeft, 0, directionA.x * distance);
                        }

                        if (directionB.x < 0)
                        {
                                signRight = Input.GetKey(buttonB) || goRight ? -1 : 1 * 1.5f;
                                distanceRight = Mathf.Clamp(distanceRight + Time.deltaTime * speedRight * signRight, directionB.x * distance, 0);
                        }
                        else if (directionB.x > 0)
                        {
                                signRight = Input.GetKey(buttonB) || goRight ? 1 : -1 * 1.5f;
                                distanceRight = Mathf.Clamp(distanceRight + Time.deltaTime * speedRight * signRight, 0, directionB.x * distance);
                        }

                        if (directionA.y > 0)
                        {
                                signUp = Input.GetKey(buttonA) || goUp ? 1 : -1 * 1.5f;
                                distanceUp = Mathf.Clamp(distanceUp + Time.deltaTime * speedUp * signUp, 0, directionA.y * distance);
                        }
                        else if (directionA.y < 0)
                        {
                                signUp = Input.GetKey(buttonA) || goUp ? -1 : 1 * 1.5f;
                                distanceUp = Mathf.Clamp(distanceUp + Time.deltaTime * speedUp * signUp, directionA.y * distance, 0);
                        }

                        if (directionB.y < 0)
                        {
                                signDown = Input.GetKey(buttonB) || goDown ? -1 : 1 * 1.5f;
                                distanceDown = Mathf.Clamp(distanceDown + Time.deltaTime * speedDown * signDown, directionB.y * distance, 0);
                        }
                        else if (directionB.y > 0)
                        {
                                signDown = Input.GetKey(buttonB) || goDown ? 1 : -1 * 1.5f;
                                distanceDown = Mathf.Clamp(distanceDown + Time.deltaTime * speedDown * signDown, 0, directionB.y * distance);
                        }

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

                        goLeft = goRight = goUp = goDown = false;

                        return appliedPeek;
                }

                public void PeekUp ()
                {
                        goUp = true;
                }

                public void PeekDown ()
                {
                        goDown = true;
                }

                public void PeekRight ()
                {
                        goRight = true;
                }

                public void PeekLeft ()
                {
                        goLeft = true;
                }

                public void PeekDirection (Vector2 direction)
                {
                        goLeft = direction.x < 0;
                        goRight = direction.x > 0;
                        goUp = direction.y > 0;
                        goDown = direction.y < 0;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool close;
                [SerializeField, HideInInspector] public bool edit;
                [SerializeField, HideInInspector] public bool add;

                public static void CustomInspector (SerializedProperty parent, Color barColor, Color labelColor)
                {
                        if (!parent.Bool("edit"))
                                return;

                        if (Follow.Open(parent, "Peek", barColor, labelColor))
                        {
                                GUI.enabled = parent.Bool("enable");
                                FoldOut.Box(3, Tint.Box);
                                parent.Field("Distance", "distance");
                                parent.Field("Easing Time", "time");
                                parent.Field("Ignore Clamps", "ignoreClamps");
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(2, Tint.Box);
                                parent.FieldDouble("Direction A", "buttonA", "directionA");
                                parent.FieldDouble("Direction B", "buttonB", "directionB");
                                Layout.VerticalSpacing(5);
                                GUI.enabled = true;
                        };
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
