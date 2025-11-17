#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.Safire2DCamera
{
        [System.Serializable]
        public class Rails
        {
                [SerializeField] public bool enable = false;
                [SerializeField] public List<Rail> rails = new List<Rail>();
                [System.NonSerialized] private int activeRailIndex = -1;
                [System.NonSerialized] private Vector2 startPoint;

                public void Initialize ()
                {
                        for (int i = 0; i < rails.Count; i++)
                        {
                                rails[i].enterLeft.Initialize();
                                rails[i].enterRight.Initialize();
                        }
                }

                public void Reset ()
                {
                        activeRailIndex = -1;
                }

                public Vector2 Position (Vector2 target, Vector2 cameraPosition, Follow follow)
                {
                        if (!enable)
                        {
                                return target;
                        }

                        Vector2 realTargetPosition = follow.TargetPosition();

                        if (activeRailIndex != -1) // inside a different rail, exit out of current rail
                        {
                                for (int i = 0; i < rails.Count; i++)
                                {
                                        if ((activeRailIndex != i || rails[i].isExit) && rails[i].Contains(realTargetPosition))
                                        {
                                                activeRailIndex = -1;
                                        }
                                }
                        }

                        for (int i = 0; i < rails.Count; i++)
                        {
                                if (activeRailIndex == i || rails[i].Contains(realTargetPosition))
                                {
                                        if (rails[i].auto)
                                        {
                                                return rails[i].AutoFollow(target, cameraPosition, follow, i, ref activeRailIndex);
                                        }
                                        if (rails[i].horizontal)
                                        {
                                                return rails[i].FollowX(target, cameraPosition, follow, i, ref activeRailIndex);
                                        }
                                        if (rails[i].vertical)
                                        {
                                                return rails[i].FollowY(target, cameraPosition, follow, i, ref activeRailIndex);
                                        }
                                }
                        }
                        return target;
                }

                public void RailPause (bool value)
                {
                        for (int i = 0; i < rails.Count; i++)
                        {
                                if (activeRailIndex == i)
                                {
                                        if (rails[i].auto)
                                                rails[i].pauseAuto = value;
                                        return;
                                }
                        }
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

                public static void CustomInspector (SerializedProperty parent, Color barColor, Color labelColor, Safire2DCamera main)
                {
                        if (!parent.Bool("edit"))
                                return;

                        if (Follow.Open(parent, "Rails", barColor, labelColor, true, canView: true))
                        {
                                GUI.enabled = parent.Bool("enable");
                                SerializedProperty array = parent.Get("rails");

                                if (parent.ReadBool("add"))
                                {
                                        array.arraySize++;
                                        EditorTools.ClearProperty(array.LastElement());
                                        SerializedProperty element = array.LastElement();
                                        SerializedProperty targets = element.Get("targets");
                                        element.Get("autoSpeed").floatValue = 5f;
                                        element.SetTrue("open");
                                        element.Get("enterLeft").Get("position").vector2Value = SceneTools.SceneCenter(Vector2.zero) + Vector2.left * 5f;
                                        element.Get("enterRight").Get("position").vector2Value = SceneTools.SceneCenter(Vector2.zero) + Vector2.right * 5f;
                                        element.Get("enterLeft").Get("size").vector2Value = new Vector2(5f, 5f);
                                        element.Get("enterRight").Get("size").vector2Value = new Vector2(5f, 5f);
                                        targets.arraySize++;
                                        targets.arraySize++;
                                        targets.Element(0).vector2Value = element.Get("enterLeft").Get("position").vector2Value;
                                        targets.Element(1).vector2Value = element.Get("enterRight").Get("position").vector2Value;
                                }

                                if (array.arraySize == 0)
                                        Layout.VerticalSpacing(5);

                                for (int i = 0; i < array.arraySize; i++)
                                {
                                        SerializedProperty element = array.Element(i);
                                        SerializedProperty targets = element.Get("targets");

                                        int type = element.Get("railType").enumValueIndex;
                                        int autoHeight = type == 2 ? 1 : 0;

                                        Color color = type == 0 ? Tint.Brown : type == 1 ? Tint.Blue : type == 2 ? Tint.Orange : Tint.Delete;

                                        FoldOut.BoxSingle(1 + autoHeight, color);
                                        {
                                                Fields.ConstructField();
                                                element.ConstructField("name", S.LW - 4f, 4f);
                                                element.ConstructField("railType", S.CW - S.B3);
                                                if (Fields.ConstructButton("Delete"))
                                                { array.DeleteArrayElement(i); break; }
                                                if (Fields.ConstructButton("Target"))
                                                { Follow.Select(array, i); }
                                                if (Fields.ConstructButton("Reopen"))
                                                { element.Toggle("open"); }
                                                if (autoHeight > 0)
                                                        element.Field("Speed", "autoSpeed");
                                        }
                                        Layout.VerticalSpacing(2);

                                        if (!element.Bool("open"))
                                        {
                                                continue;
                                        }

                                        if (type != 3)
                                        {
                                                int selectIndex = element.Int("selectIndex");
                                                FoldOut.Box(targets.arraySize, color: FoldOut.boxColor, offsetY: -2);
                                                {
                                                        for (int j = 0; j < targets.arraySize; j++)
                                                        {
                                                                if (j == selectIndex)
                                                                {
                                                                        float offset = j == 0 ? 5 : Layout.rectFieldHeight;
                                                                        Layout.GetLastRectDraw(Layout.longInfoWidth, Layout.rectFieldHeight, -11, offset, color: Tint.WhiteOpacity100);
                                                                }
                                                                Fields.ConstructField();
                                                                targets.Element(j).ConstructField(S.FW - S.B2);
                                                                if (Fields.ConstructButton("Add"))
                                                                { targets.InsertArrayElement(j); break; }
                                                                if (Fields.ConstructButton("Minus") && targets.arraySize > 2)
                                                                { targets.DeleteArrayElement(j); break; }
                                                        }
                                                }
                                                Layout.VerticalSpacing(autoHeight > 0 ? 1 : 3);
                                        }
                                        else
                                        {
                                                FoldOut.BoxSingle(1, color: FoldOut.boxColor, offsetY: -2);
                                                {
                                                        Labels.Display(element.Get("enterLeft").Get("position").vector2Value.ToString());
                                                }
                                                Layout.VerticalSpacing(1);
                                        }

                                        if (autoHeight > 0)
                                        {
                                                Fields.EventFoldOut(element.Get("onComplete"), element.Get("completeFoldOut"), "On Complete");
                                        }

                                }
                                GUI.enabled = true;
                        }
                }

                public static void DrawRails (Safire2DCamera main, UnityEditor.Editor editor)
                {
                        if (!main.follow.rails.view || !main.follow.rails.enable)
                                return;

                        List<Rail> rails = main.follow.rails.rails;

                        for (int i = 0; i < rails.Count; i++)
                        {
                                Color previousColor = Handles.color;
                                Rail rail = rails[i];
                                bool autoRail = rail.railType == RailType.Auto;
                                Color color = rail.railType == RailType.Horizontal ? Tint.Brown : rail.railType == RailType.Vertical ? Tint.Blue : rail.railType == RailType.Exit ? Tint.Delete : Tint.PastelGreen;
                                Handles.color = color;

                                SimpleBounds boundsLeft = rail.enterLeft;
                                SimpleBounds boundsRight = rail.enterRight;

                                if (rail.railType != RailType.Exit)
                                {
                                        List<Vector2> targets = rail.targets;
                                        for (int j = 0; j < targets.Count; j++)
                                        {
                                                targets[j] = SceneTools.MovePositionCircleHandle(targets[j], snap: 0.25f, editor: editor);
                                                if (Mouse.down && ((Vector2) Mouse.position - targets[j]).sqrMagnitude < 1f)
                                                {
                                                        rail.selectIndex = j;
                                                }
                                                if (main.cameraRef != null && rail.select == i && rail.selectIndex == j)
                                                {
                                                        SceneTools.SquareCenterBroken(targets[j], main.cameraRef.CameraSize(), Tint.WhiteOpacity100, 8f);
                                                }
                                        }
                                        for (int j = 0; j < targets.Count - 1; j++)
                                        {
                                                SceneTools.Line(targets[j], targets[j + 1]);
                                        }

                                        Vector2 newPositionLeft = SceneTools.MovePositionDotHandle(boundsLeft.position, Vector2.one, handleSize: 0.5f, snap: 0.25f);
                                        if (newPositionLeft != boundsLeft.position)
                                        {
                                                rail.offsetLeft += (newPositionLeft - boundsLeft.position);
                                        }
                                        Vector2 previousSizeLeft = boundsLeft.size;

                                        SceneTools.DrawAndModifyBounds(ref boundsLeft.position, ref boundsLeft.size, rail.select == i ? Tint.PastelGreen : color, 0.5f, move: false);
                                        if (previousSizeLeft != boundsLeft.size)
                                        {
                                                rail.offsetLeft += (boundsLeft.size - previousSizeLeft) * 0.5f;
                                        }

                                        Vector2 newPositionRight = SceneTools.MovePositionDotHandle(boundsRight.position, Vector2.one, handleSize: 0.5f, snap: 0.25f);

                                        if (newPositionRight != boundsRight.position)
                                        {
                                                rail.offsetRight += (newPositionRight - boundsRight.position);
                                        }
                                        if (!autoRail)
                                        {
                                                Vector2 previousSizeRight = boundsRight.size;
                                                SceneTools.DrawAndModifyBounds(ref boundsRight.position, ref boundsRight.size, rail.select == i ? Tint.PastelGreen : color, 0.5f, move: false);
                                                if (previousSizeRight != boundsRight.size)
                                                {
                                                        rail.offsetRight += (boundsRight.size - previousSizeRight) * 0.5f;
                                                }
                                        }

                                        boundsLeft.position = targets[0] - boundsLeft.size * 0.5f + rail.offsetLeft;
                                        boundsRight.position = targets[targets.Count - 1] - boundsRight.size * 0.5f + rail.offsetRight;
                                        SceneTools.Label(boundsLeft.position + Vector2.up * (boundsLeft.size.y + 1f), "Rail Left", Tint.WarmWhite);
                                        SceneTools.Label(boundsRight.position + Vector2.up * (boundsRight.size.y + 1f), "Rail Right", Tint.WarmWhite);
                                }

                                if (Mouse.down && (boundsLeft.DetectRaw(Mouse.position) || boundsRight.DetectRaw(Mouse.position)))
                                {
                                        for (int j = 0; j < rails.Count; j++)
                                        {
                                                rails[j].select = -1;
                                                rails[j].open = false;
                                        }
                                        rail.select = i;
                                        rail.open = true;
                                }
                                Handles.color = previousColor;

                                if (rail.railType == RailType.Exit)
                                {
                                        SceneTools.DrawAndModifyBounds(ref boundsLeft.position, ref boundsLeft.size, Tint.Delete, 0.5f);
                                        SceneTools.Label(boundsLeft.position + Vector2.up * (boundsLeft.size.y + 1f), "Rail Exit", Tint.WarmWhite);
                                        boundsRight.position = boundsLeft.position;
                                        boundsRight.size = boundsLeft.size;
                                }
                        }
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        [System.Serializable]
        public class Rail
        {
                [SerializeField] public RailType railType;
                [SerializeField] public string name = "Name";
                [SerializeField] public float autoSpeed = 2f;
                [SerializeField] public Vector2 offsetLeft;
                [SerializeField] public Vector2 offsetRight;
                [SerializeField] public UnityEvent onComplete;
                [SerializeField] public List<Vector2> targets = new List<Vector2>();
                [SerializeField, HideInInspector] public SimpleBounds enterRight = new SimpleBounds();
                [SerializeField, HideInInspector] public SimpleBounds enterLeft = new SimpleBounds();

                [System.NonSerialized] public bool pauseAuto;
                [System.NonSerialized] public float smooth;
                [System.NonSerialized] private int followIndex;
                [System.NonSerialized] Vector2 startPoint;

                public bool horizontal => railType == RailType.Horizontal;
                public bool vertical => railType == RailType.Vertical;
                public bool auto => railType == RailType.Auto;
                public bool isExit => railType == RailType.Exit;
                public static Vector2 h = Vector2.right * 1000f;
                public static Vector2 v = Vector2.up * 1000f;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public int select = -1;
                [SerializeField, HideInInspector] public int selectIndex = -1;
                [SerializeField, HideInInspector] public bool open = false;
                [SerializeField, HideInInspector] public bool completeFoldOut;
#pragma warning restore 0414
#endif
                #endregion

                public bool Contains (Vector2 followTarget)
                {
                        smooth = followIndex = 0;
                        return auto ? enterLeft.Contains(followTarget) : enterLeft.Contains(followTarget) || enterRight.Contains(followTarget);
                }

                public Vector2 FollowX (Vector2 followTarget, Vector2 cameraPosition, Follow follow, int index, ref int activeRailIndex)
                {
                        for (int i = 0; i < targets.Count - 1; i++)
                        {
                                if (Compute.LineIntersection(followTarget + v, followTarget - v, targets[i], targets[i + 1], out Vector2 intersectionX))
                                {
                                        activeRailIndex = index;
                                        follow.ForceTargetSmooth(x: false); // needed so when it completes it doesn't snap
                                        startPoint.y = smooth == 0 ? cameraPosition.y : startPoint.y;
                                        return new Vector2(followTarget.x, Compute.Lerp(startPoint.y, intersectionX.y, 0.3f, ref smooth));
                                }
                        }
                        smooth = 0;
                        activeRailIndex = -1;
                        return followTarget;
                }

                public Vector2 FollowY (Vector2 followTarget, Vector2 cameraPosition, Follow follow, int index, ref int activeRailIndex)
                {
                        for (int i = 0; i < targets.Count - 1; i++)
                        {
                                if (Compute.LineIntersection(followTarget - h, followTarget + h, targets[i], targets[i + 1], out Vector2 intersectionY))
                                {
                                        activeRailIndex = index;
                                        follow.ForceTargetSmooth(y: false);
                                        startPoint.x = smooth == 0 ? cameraPosition.x : startPoint.x;
                                        return new Vector2(Compute.Lerp(startPoint.x, intersectionY.x, 0.3f, ref smooth), followTarget.y);
                                }
                        }
                        smooth = 0;
                        activeRailIndex = -1;
                        return followTarget;
                }

                public Vector2 AutoFollow (Vector2 followTarget, Vector2 cameraPosition, Follow follow, int index, ref int activeRailIndex)
                {
                        activeRailIndex = index;
                        for (int i = followIndex; i < targets.Count;)
                        {
                                follow.ForceTargetSmooth();
                                follow.usingAutoRail = true;
                                Vector2 railTarget = Vector2.MoveTowards(cameraPosition, targets[i], Time.deltaTime * (pauseAuto ? 0 : autoSpeed));
                                if ((railTarget - targets[i]).magnitude < 0.01f)
                                        followIndex++;
                                if (followIndex >= targets.Count)
                                {
                                        onComplete.Invoke();
                                        break;
                                }
                                return railTarget;
                        }
                        smooth = 0;
                        activeRailIndex = -1;
                        return followTarget;
                }

        }

        public enum RailType
        {
                Horizontal,
                Vertical,
                Auto,
                Exit
        }

}
