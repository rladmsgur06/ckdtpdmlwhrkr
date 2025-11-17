#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        [System.Serializable]
        public class LookAhead
        {
                [SerializeField] public bool enable;
                [SerializeField] public ForwardFocusType type;
                [SerializeField] public ZoneShape zoneType;
                [SerializeField] public Vector2 threshold;
                [SerializeField] public float smooth = 0.5f;
                [SerializeField] public float distanceX;
                [SerializeField] public float distanceY;
                [SerializeField] public float radius;

                [System.NonSerialized] public Follow follow;
                [System.NonSerialized] private Vector2 newTarget;
                [System.NonSerialized] private Vector2 lookAhead;
                [System.NonSerialized] private Vector2 direction;
                [System.NonSerialized] private Vector2 previousTarget;

                public bool isCircle => zoneType == ZoneShape.Circle;
                public bool radiusCircle => radius > 0 && isCircle;
                public Vector2 mouseViewPort { get { return follow.camera.ScreenToViewportPoint(Input.mousePosition); } }

                public void Initialize (Follow follow)
                {
                        this.follow = follow;
                        previousTarget = follow.TargetPosition();
                }

                public void Reset ()
                {
                        lookAhead = direction = newTarget = Vector2.zero;
                        previousTarget = follow.TargetPosition();
                }

                public void RoomExit ()
                {
                        lookAhead = follow.cameraTransform.position - follow.TargetPosition();
                }

                public Vector2 Position (Vector2 target, Camera camera, bool isUser)
                {
                        if (!enable || isUser)
                                return previousTarget = target;

                        Vector2 desiredMovement = target - previousTarget;
                        if (desiredMovement.magnitude >= 2f) /// 2 represent the amount of distance the player instantly skipped. if so, force a smooth follow.
                        {
                                if (radiusCircle)
                                {
                                        follow.ForceTargetSmooth(x: false);
                                        follow.ForceTargetSmooth(y: false);
                                }
                                if (distanceX > 0)
                                {
                                        follow.ForceTargetSmooth(y: false);
                                }
                                if (distanceY > 0)
                                {
                                        follow.ForceTargetSmooth(x: false);
                                }
                        }

                        Vector2 velocity = Time.deltaTime <= 0 ? Vector2.zero : (target - previousTarget) / Time.deltaTime;
                        newTarget = Vector2.zero;
                        previousTarget = target;

                        bool movingX = Mathf.Abs(velocity.x) > 0.001f;
                        bool movingY = Mathf.Abs(velocity.y) > 0.001f;
                        bool moving = movingX || movingY;
                        if (movingX)
                                direction.x = Mathf.Sign(velocity.x);
                        if (movingY)
                                direction.y = Mathf.Sign(velocity.y);

                        if (type == ForwardFocusType.Mouse)
                        {
                                newTarget = MouseTarget(camera.Size());
                        }
                        else if (isCircle)
                        {
                                if (moving)
                                        direction = velocity.normalized; // this will remember previous direction instead of resetting it to zero.
                                newTarget = Look(camera.ShortestLength() * direction * radius, velocity, threshold, lookAhead, moving);
                        }
                        else
                        {
                                newTarget.x = Look(camera.Width() * distanceX * direction.x, velocity.x, threshold.x, lookAhead.x, movingX);
                                newTarget.y = Look(camera.Height() * distanceY * direction.y, velocity.y, threshold.y, lookAhead.y, movingY);
                        }
                        if (radiusCircle)
                        {
                                // follow.ForceTargetClamp();
                                follow.ForceTargetClampConditional(x: false);
                                follow.ForceTargetClampConditional(y: false);
                        }
                        if (distanceX > 0)
                        {
                                follow.ForceTargetClampConditional(y: false);
                        }
                        if (distanceY > 0)
                        {
                                follow.ForceTargetClampConditional(x: false);
                        }
                        lookAhead = Compute.Lerp(lookAhead, newTarget, smooth);

                        return target + new Vector2(distanceX > 0 || radiusCircle ? lookAhead.x : 0, distanceY > 0 || radiusCircle ? lookAhead.y : 0);
                }

                private Vector2 MouseTarget (Vector2 cameraSize)
                {
                        Vector2 mouse = (mouseViewPort * 2f) - Vector2.one; // will make the center of the screen the relative point
                        mouse = new Vector2(Mathf.Clamp(mouse.x, -1f, 1f), Mathf.Clamp(mouse.y, -1f, 1f));

                        if (isCircle)
                        {
                                float shortestLength = cameraSize.y < cameraSize.x ? cameraSize.y : cameraSize.x;
                                return mouse.normalized * radius * Mathf.Clamp(mouse.magnitude * shortestLength, -shortestLength, shortestLength);
                        }
                        else
                        {
                                mouse.x = Mathf.Clamp(mouse.x, -distanceX, distanceX);
                                mouse.y = Mathf.Clamp(mouse.y, -distanceY, distanceY);
                                return new Vector2(mouse.x * cameraSize.x, mouse.y * cameraSize.y);
                        }
                }

                public float Look (float newTarget, float velocity, float threshold, float lookAhead, bool moving)
                {
                        if (type == ForwardFocusType.Recenter)
                        {
                                newTarget = moving ? newTarget : 0;
                        }
                        else if (type == ForwardFocusType.Inverted)
                        {
                                newTarget = moving ? 0 : newTarget;
                        }
                        else if (type == ForwardFocusType.Incremental)
                        {
                                newTarget = moving ? newTarget : lookAhead;
                        }
                        else if (type == ForwardFocusType.Threshold)
                        {
                                newTarget = Mathf.Abs(velocity) >= threshold ? newTarget : 0;
                        }
                        return newTarget;
                }

                public Vector2 Look (Vector2 newTarget, Vector2 velocity, Vector2 threshold, Vector2 lookAhead, bool moving)
                {
                        if (type == ForwardFocusType.Recenter)
                        {
                                newTarget = moving ? newTarget : Vector2.zero;
                        }
                        else if (type == ForwardFocusType.Inverted)
                        {
                                newTarget = moving ? Vector2.zero : newTarget;
                        }
                        else if (type == ForwardFocusType.Incremental)
                        {
                                newTarget = moving ? newTarget : lookAhead;
                        }
                        else if (type == ForwardFocusType.Threshold)
                        {
                                newTarget = velocity.magnitude >= threshold.magnitude ? newTarget : Vector2.zero;
                        }
                        return newTarget;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool edit;
                [SerializeField, HideInInspector] public bool close;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool view = true;
                public static void CustomInspector (SerializedProperty parent, Color barColor, Color labelColor)
                {
                        if (!parent.Bool("edit"))
                                return;

                        if (Follow.Open(parent, "Look Ahead", barColor, labelColor, canView: true))
                        {
                                GUI.enabled = parent.Bool("enable");

                                int shape = parent.Enum("zoneType");
                                int type = parent.Enum("type");

                                FoldOut.Box(type == 3 ? 3 : 2, Tint.Box);
                                parent.FieldDouble("Type", "type", "zoneType");
                                parent.Slider("Smooth", "smooth", 0, 1f);
                                parent.Field("Threshold", "threshold", execute: type == 3);
                                Layout.VerticalSpacing(5);

                                FoldOut.Box(shape == 0 ? 2 : 1, Tint.Box);
                                if (shape == 0)
                                        parent.Slider("Horizontal", "distanceX");
                                if (shape == 0)
                                        parent.Slider("Vertical", "distanceY");
                                if (shape == 1)
                                        parent.Slider("Radius", "radius");
                                Layout.VerticalSpacing(5);

                                GUI.enabled = true;
                        }
                }

#pragma warning restore 0414
#endif
                #endregion

        }

        public enum ForwardFocusType
        {
                Normal,
                Recenter,
                Inverted,
                Incremental,
                Threshold,
                Mouse
        }

        public enum ZoneShape
        {
                Square,
                Circle
        }

}
