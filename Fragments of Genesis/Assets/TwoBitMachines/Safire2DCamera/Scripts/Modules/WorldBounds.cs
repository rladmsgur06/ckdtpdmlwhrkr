#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        [System.Serializable]
        public class WorldBounds
        {
                [SerializeField] public bool enable = false;
                [SerializeField] public List<WorldBound> bounds = new List<WorldBound>();
                [System.NonSerialized] private Vector2 previousTarget;

                public void Reset ()
                {
                        for (int i = 0; i < bounds.Count; i++)
                        {
                                bounds[i].canClamp = false;
                                bounds[i].clamping = false;
                                bounds[i].holding = false;
                        }
                }

                public void Pause (int index, bool value)
                {
                        for (int i = 0; i < bounds.Count; i++)
                        {
                                if (i == index)
                                {
                                        bounds[i].pause = value;
                                        return;
                                }
                        }
                }

                public void Exit (Follow follow)
                {
                        if (!enable || bounds.Count == 0)
                                return;

                        Vector2 target = follow.TargetPosition();

                        for (int i = 0; i < bounds.Count; i++)
                        {
                                if (bounds[i].pause)
                                {
                                        continue;
                                }
                                if (!bounds[i].InRange(target) && bounds[i].BreakClamp(target))
                                {
                                        if ((bounds[i].canClamp && bounds[i].clamping) || bounds[i].holding)
                                        {
                                                follow.ForceTargetSmooth(bounds[i].horizontal, bounds[i].vertical);
                                        }

                                        bounds[i].canClamp = false;
                                        bounds[i].clamping = false;
                                        bounds[i].holding = false;
                                }
                        }
                }

                public void Clamp (Camera camera, Vector3 previousCamera, Follow follow)
                {
                        if (!enable || bounds.Count == 0)
                                return;

                        Vector3 camCenter = camera.transform.position;
                        Vector2 target = follow.TargetPosition();

                        for (int i = 0; i < bounds.Count; i++)
                        {
                                if (bounds[i].pause)
                                {
                                        continue;
                                }
                                if (!bounds[i].InRange(target) && bounds[i].BreakClamp(target))
                                {
                                        continue;
                                }

                                float width = camera.Width();
                                float height = camera.Height();
                                camCenter = bounds[i].Hold(target, camCenter, previousCamera, width, height);

                                if (bounds[i].canClamp || bounds[i].CanClamp(camCenter, width, height))
                                {
                                        bounds[i].canClamp = true;
                                        bounds[i].holding = false;
                                        camCenter = bounds[i].Clamp(camCenter, previousCamera.x, width, height, target.x != previousTarget.x);
                                }
                        }
                        camera.transform.position = camCenter;
                        previousTarget = target;
                }

                #region ▀▄▀▄▀▄  Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool add;
                [SerializeField, HideInInspector] public bool edit;
                [SerializeField, HideInInspector] public bool close;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool view = true;
                [SerializeField, HideInInspector] public int select = -1;
                [SerializeField, HideInInspector] public int signalIndex;
                [SerializeField, HideInInspector] public bool active;

                public static void CustomInspector (SerializedProperty parent, Color barColor, Color labelColor)
                {
                        if (!parent.Bool("edit"))
                                return;

                        if (Follow.Open(parent, "World Bounds", barColor, labelColor, true, canView: true))
                        {
                                GUI.enabled = parent.Bool("enable");
                                SerializedProperty array = parent.Get("bounds");

                                if (parent.ReadBool("add"))
                                {
                                        array.arraySize++;
                                        EditorTools.ClearProperty(array.LastElement());
                                        SerializedProperty element = array.LastElement();
                                        element.Get("a").vector2Value = SceneTools.SceneCenter(Vector2.zero) + Vector2.up * 5f;
                                        element.Get("b").vector2Value = SceneTools.SceneCenter(Vector2.zero) + Vector2.down * 5f;
                                        parent.Get("select").intValue = array.arraySize - 1;
                                }

                                if (array.arraySize == 0)
                                {
                                        Layout.VerticalSpacing(5);
                                }

                                int select = parent.Int("select");
                                for (int i = 0; i < array.arraySize; i++)
                                {
                                        SerializedProperty element = array.Element(i);

                                        FoldOut.BoxSingle(1, FoldOut.boxColor);
                                        {
                                                if (i == select)
                                                {
                                                        Layout.GetLastRectDraw(Layout.longInfoWidth, Layout.rectFieldHeight + 4, -11, 0, color: Tint.Delete180);
                                                }
                                                if (element.FieldDoubleAndButton("", "boundsType", "clampType", "Delete"))
                                                {
                                                        array.DeleteArrayElement(i);
                                                        break;
                                                }
                                                if (ListReorder.Grip(parent, array, Layout.GetLastRect(20, 20), i, Tint.WarmWhite, yOffset: 2))
                                                {
                                                        parent.Get("select").intValue = ListReorder.dstItemIndex;
                                                }
                                                SerializedProperty pause = element.Get("pause");
                                                if (Buttons.Button(Layout.GetLastRect(20, 20, 10), Icon.Get("Pause"), pause.boolValue ? Tint.PastelGreen : Tint.Delete, center: true))
                                                {
                                                        pause.Toggle();
                                                }

                                                Rect block = Layout.GetLastRect(Layout.longInfoWidth, 18, -11, 0);
                                                if (block.ContainsMouseDown())
                                                {
                                                        parent.Get("select").intValue = i;
                                                }
                                        }
                                        Layout.VerticalSpacing(2);

                                }
                                GUI.enabled = true;
                        }
                }

                public static void DrawBounds (Safire2DCamera main, UnityEditor.Editor editor)
                {
                        if (!main.worldBounds.view || !main.worldBounds.enable)
                                return;

                        List<WorldBound> bounds = main.worldBounds.bounds;

                        for (int i = 0; i < bounds.Count; i++)
                        {
                                Color previousColor = Handles.color;
                                WorldBound bound = bounds[i];
                                Color color = Tint.Delete;
                                Handles.color = color;

                                Vector2 oldA = bound.a;
                                Vector2 oldB = bound.b;
                                bound.a = SceneTools.MovePositionCircleHandle(bound.a, snap: 0.25f, editor: editor);
                                bound.b = SceneTools.MovePositionCircleHandle(bound.b, snap: 0.25f, editor: editor);

                                bool aChanged = oldA != bound.a;
                                bool bChanged = oldB != bound.b;

                                if (bound.boundsType == BoundsType.Left || bound.boundsType == BoundsType.Right)
                                {
                                        if (aChanged)
                                                bound.b.x = bound.a.x;
                                        if (bChanged)
                                                bound.a.x = bound.b.x;
                                }
                                else
                                {
                                        if (aChanged)
                                                bound.b.y = bound.a.y;
                                        if (bChanged)
                                                bound.a.y = bound.b.y;
                                }

                                Vector2 center = (bound.a + bound.b) * 0.5f;
                                if (main.worldBounds.select == i)
                                {
                                        SceneTools.SolidDisc(center, Vector3.forward, 1.5f, bound.pause ? Tint.PastelGreen100 : Tint.Delete100);
                                }
                                if (bound.restrict)
                                {
                                        SceneTools.Line(bound.a, bound.b, bound.pause ? Tint.PastelGreen : Tint.Delete);
                                }
                                else
                                {
                                        SceneTools.DottedLine(bound.a, bound.b, bound.pause ? Tint.PastelGreen : Tint.Delete);
                                }
                                SceneTools.Line(center, center - (bound.Direction() * (bound.horizontal ? main.cameraRef.Width() * 2f : main.cameraRef.Height() * 2f)), Tint.WhiteOpacity100);

                                Handles.color = Tint.WarmWhite;
                                Handles.Label(center - Vector2.right * 1f + Vector2.up * 0.5f, bound.boundsType.ToString());

                                if (Mouse.down && (SceneTools.NearMouse(bound.a) || SceneTools.NearMouse(bound.b)))
                                {
                                        main.worldBounds.select = i;
                                }
                                Handles.color = previousColor;
                        }
                }
#pragma warning restore 0414
#endif
                #endregion

        }

        [System.Serializable]
        public class WorldBound
        {
                [SerializeField] public BoundsType boundsType;
                [SerializeField] public ClampType clampType;
                [SerializeField] public Vector2 a;
                [SerializeField] public Vector2 b;
                [SerializeField] public bool pause = false;

                [System.NonSerialized] public bool canClamp;
                [System.NonSerialized] public bool clamping;
                [System.NonSerialized] public bool holding;
                [System.NonSerialized] private float buffer = 2.5f;
                [System.NonSerialized] private float rate = 0.98f;
                [System.NonSerialized] private Vector2 hold;
                [System.NonSerialized] private Vector2 oldCenter;
                [System.NonSerialized] private float width;
                [System.NonSerialized] private float height;
                [System.NonSerialized] private float graceRate = 4f;
                [System.NonSerialized] private float graceDistance = 2f;

                public bool nearLeft => Mathf.Abs(oldCenter.x - (a.y + width)) <= graceDistance;
                public bool nearRight => Mathf.Abs(oldCenter.x - (a.y - width)) <= graceDistance;
                public bool nearTop => Mathf.Abs(oldCenter.y - (a.y - height)) <= graceDistance;
                public bool nearBottom => Mathf.Abs(oldCenter.y - (a.y + height)) <= graceDistance;

                public bool restrict => clampType == ClampType.ClampRestrict;
                public bool vertical => boundsType == BoundsType.Top || boundsType == BoundsType.Bottom;
                public bool horizontal => boundsType == BoundsType.Left || boundsType == BoundsType.Right;

                public bool InRange (Vector2 position)
                {
                        if (boundsType == BoundsType.Left)
                        {
                                return position.x >= a.x && InsideY(position.y);
                        }
                        if (boundsType == BoundsType.Right)
                        {
                                return position.x <= a.x && InsideY(position.y);
                        }
                        if (boundsType == BoundsType.Top)
                        {
                                return position.y <= a.y && InsideX(position.x);
                        }
                        if (boundsType == BoundsType.Bottom)
                        {
                                return position.y >= a.y && InsideX(position.x);
                        }
                        return false;
                }

                public bool SemiInRange (Vector2 position)
                {
                        if (boundsType == BoundsType.Left)
                        {
                                return position.x < a.x && InsideY(position.y);
                        }
                        if (boundsType == BoundsType.Right)
                        {
                                return position.x > a.x && InsideY(position.y);
                        }
                        if (boundsType == BoundsType.Top)
                        {
                                return position.y > a.y && InsideX(position.x);
                        }
                        if (boundsType == BoundsType.Bottom)
                        {
                                return position.y < a.y && InsideX(position.x);
                        }
                        return false;
                }

                public bool InsideY (float y)
                {
                        return (y <= a.y && y >= b.y) || (y >= a.y && y <= b.y);
                }

                public bool InsideX (float x)
                {
                        return (x <= a.x && x >= b.x) || (x >= a.x && x <= b.x);
                }

                public bool CanClamp (Vector2 center, float width, float height)
                {
                        if (boundsType == BoundsType.Left)
                        {
                                return center.x - width >= a.x - 0.035f;
                        }
                        if (boundsType == BoundsType.Right)
                        {
                                return center.x + width <= a.x + 0.035f;
                        }
                        if (boundsType == BoundsType.Top)
                        {
                                return center.y + height <= a.y + 0.035f;
                        }
                        if (boundsType == BoundsType.Bottom)
                        {
                                return center.y - height >= a.y - 0.035f;
                        }
                        return false;
                }

                public Vector3 Hold (Vector2 target, Vector3 center, Vector3 oldCenter, float width, float height)
                {
                        if (canClamp || clamping)
                        {
                                return center;
                        }

                        this.oldCenter = oldCenter;
                        this.width = width;
                        this.height = height;

                        if (boundsType == BoundsType.Left)
                        {
                                if (target.x >= a.x && center.x - width < a.x && (center.x < oldCenter.x || nearLeft))
                                {
                                        holding = true;
                                        if (nearLeft) // move towards bound if camera edge is near enough
                                        {
                                                oldCenter.x += ((a.x + width) - oldCenter.x) * Time.deltaTime * graceRate;
                                        }
                                        return new Vector3(oldCenter.x, center.y, center.z);
                                }
                        }
                        if (boundsType == BoundsType.Right)
                        {
                                if (target.x <= a.x && center.x + width > a.x && (center.x > oldCenter.x || nearRight))
                                {
                                        holding = true;
                                        if (nearRight)
                                        {
                                                oldCenter.x += ((a.x - width) - oldCenter.x) * Time.deltaTime * graceRate;
                                        }
                                        return new Vector3(oldCenter.x, center.y, center.z);
                                }
                        }
                        if (boundsType == BoundsType.Top)
                        {
                                if (target.y <= a.y && center.y + height > a.y && (center.y >= oldCenter.y || nearTop))
                                {
                                        holding = true;
                                        if (nearTop)
                                        {
                                                oldCenter.y += ((a.y - height) - oldCenter.y) * Time.deltaTime * graceRate;
                                        }
                                        return new Vector3(center.x, oldCenter.y, center.z);
                                }
                        }
                        if (boundsType == BoundsType.Bottom)
                        {
                                if (target.y >= a.y && center.y - height < a.y && (center.y < oldCenter.y || nearBottom))
                                {
                                        holding = true;
                                        if (nearBottom)
                                        {
                                                oldCenter.y += ((a.y + height) - oldCenter.y) * Time.deltaTime * graceRate;
                                        }
                                        return new Vector3(center.x, oldCenter.y, center.z);
                                }
                        }
                        return center;
                }

                public Vector3 Clamp (Vector3 center, float previousX, float width, float height, bool targetIsMoving)
                {
                        clamping = false;
                        if (boundsType == BoundsType.Left)
                        {
                                float vel = center.x - previousX;
                                if (targetIsMoving && vel < 0 && (center.x - width) < a.x + buffer)
                                {
                                        center.x = Compute.Lerp(center.x - vel, a.x + width, rate);
                                }

                                if (center.x - width <= a.x)
                                {
                                        clamping = true;
                                        return new Vector3(a.x + width, center.y, center.z);
                                }
                        }
                        if (boundsType == BoundsType.Right)
                        {
                                float vel = center.x - previousX;
                                if (targetIsMoving && vel > 0 && (center.x + width) > a.x - buffer)
                                {
                                        center.x = Compute.Lerp(center.x - vel, a.x - width, rate);
                                }

                                if (center.x + width >= a.x)
                                {
                                        clamping = true;
                                        return new Vector3(a.x - width, center.y, center.z);
                                }
                        }
                        if (boundsType == BoundsType.Top)
                        {
                                if (center.y + height >= a.y)
                                {
                                        clamping = true;
                                        return new Vector3(center.x, a.y - height, center.z);
                                }
                        }
                        if (boundsType == BoundsType.Bottom)
                        {
                                if (center.y - height <= a.y)
                                {
                                        clamping = true;
                                        return new Vector3(center.x, a.y + height, center.z);
                                }
                        }
                        return center;
                }

                public Vector2 Direction ()
                {
                        if (boundsType == BoundsType.Left)
                        {
                                return Vector2.left;
                        }
                        if (boundsType == BoundsType.Right)
                        {
                                return Vector2.right;
                        }
                        if (boundsType == BoundsType.Top)
                        {
                                return Vector2.up;
                        }
                        if (boundsType == BoundsType.Bottom)
                        {
                                return Vector2.down;
                        }
                        return Vector2.one;
                }

                public bool BreakClamp (Vector2 target)
                {
                        return !(restrict && SemiInRange(target));
                }
        }

        public enum BoundsType
        {
                Left,
                Right,
                Top,
                Bottom
        }

        public enum ClampType
        {
                CanBreakClamp,
                ClampRestrict
        }
}
