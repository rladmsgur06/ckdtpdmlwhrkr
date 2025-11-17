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
        public class Regions
        {
                [SerializeField] public bool enable;
                [SerializeField] public List<Region> regions = new List<Region>();

                public void Reset ()
                {
                        for (int i = 0; i < regions.Count; i++)
                                regions[i].inside = false;
                }

                public Vector3 Offset (Vector3 target, Follow follow, bool isUser)
                {
                        if (!enable || regions.Count == 0 || isUser)
                                return Vector3.zero;

                        Vector3 totalOffset = Vector2.zero;
                        for (int i = 0; i < regions.Count; i++)
                        {
                                if (regions[i].Execute(target + totalOffset, follow))
                                {
                                        follow.ForceTargetClamp();
                                        totalOffset += (regions[i].center - target) * regions[i].pull;
                                }
                        }
                        return totalOffset;
                }

                [System.Serializable]
                public class Region
                {
                        [SerializeField] public RegionType type;
                        [SerializeField] public float zoom = 0f;
                        [SerializeField] public float smooth = 0.5f;
                        [SerializeField] public float outerRegion = 10f;
                        [SerializeField] public float innerRegion = 5f;
                        [SerializeField] public Transform transform;
                        [SerializeField] public Vector3 position;
                        [SerializeField] public UnityEvent onExit;
                        [SerializeField] public UnityEvent onEnter;

                        [System.NonSerialized] public Vector3 center;
                        [System.NonSerialized] public bool inside;
                        [System.NonSerialized] public float pull;
                        [System.NonSerialized] private float originalScale = 1;

                        private float outerRegionSqr => outerRegion * outerRegion;
                        private float innerRegionSqr => innerRegion * innerRegion;

                        #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                        [SerializeField, HideInInspector] public int select = -1;
                        [SerializeField, HideInInspector] public bool eventsFoldOut;
                        [SerializeField, HideInInspector] public bool enterFoldOut;
                        [SerializeField, HideInInspector] public bool exitFoldOut;
                        [SerializeField, HideInInspector] public bool open;
#pragma warning restore 0414
#endif
                        #endregion

                        public bool Execute (Vector3 target, Follow follow)
                        {
                                center = type == RegionType.Transform ? transform != null ? transform.position : target : position;
                                Vector2 distance = target - center;

                                if (distance.sqrMagnitude <= outerRegionSqr)
                                {
                                        if (!inside)
                                        {
                                                inside = true;
                                                onEnter.Invoke();
                                                originalScale = follow.zoom.scale > 0 ? follow.zoom.scale : 1;
                                        }
                                        float magnitude = distance.sqrMagnitude <= innerRegionSqr ? 0 : distance.magnitude - innerRegion;
                                        pull = 1f - (magnitude / (outerRegion - innerRegion));
                                        if (zoom > 0)
                                        {
                                                follow.zoom.Set(scale: originalScale + (zoom - originalScale) * pull, speed: smooth == 0 ? 0.1f : smooth, isTween: false);
                                        }
                                }
                                else if (inside) // no longer inside, exit
                                {
                                        inside = false;
                                        onExit.Invoke();
                                        if (follow.zoom.scale != 1 && zoom > 0)
                                        {
                                                follow.zoom.Set(scale: 1f, duration: 2f);
                                        }
                                }
                                return inside;
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool add;
                [SerializeField, HideInInspector] public bool edit;
                [SerializeField, HideInInspector] public bool close;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool view = true;
                public static void CustomInspector (SerializedProperty parent, Color barColor, Color labelColor)
                {
                        if (!parent.Bool("edit"))
                                return;

                        if (Follow.Open(parent, "Regions", barColor, labelColor, true, canView: true))
                        {
                                SerializedProperty array = parent.Get("regions");

                                if (parent.ReadBool("add"))
                                {
                                        array.arraySize++;
                                        EditorTools.ClearProperty(array.LastElement());
                                        array.LastElement().Get("position").vector3Value = SceneTools.SceneCenter(Vector2.zero);
                                        array.LastElement().Get("outerRegion").floatValue = 10f;
                                        array.LastElement().Get("innerRegion").floatValue = 5f;
                                }

                                if (array.arraySize == 0)
                                        Layout.VerticalSpacing(5);

                                GUI.enabled = parent.Bool("enable");
                                for (int i = 0; i < array.arraySize; i++)
                                {
                                        SerializedProperty element = array.Element(i);
                                        int type = element.Get("type").enumValueIndex;

                                        FoldOut.BoxSingle(1, Tint.Orange);
                                        {
                                                Fields.ConstructField();
                                                Fields.ConstructSpace(S.LW);
                                                element.ConstructField("type", S.CW - S.B3);
                                                if (Fields.ConstructButton("Delete"))
                                                { array.DeleteArrayElement(i); break; }
                                                if (Fields.ConstructButton("Target"))
                                                { Select(array, i); }
                                                if (Fields.ConstructButton("Reopen"))
                                                { element.Toggle("open"); }

                                        }
                                        Layout.VerticalSpacing(2);

                                        if (!element.Bool("open"))
                                                continue;

                                        FoldOut.Box(type == 1 ? 4 : 3, color: FoldOut.boxColor, extraHeight: 3, offsetY: -2);
                                        {
                                                element.Field("Transform", "transform", execute: type == 1);
                                                element.FieldDouble("Radius", "outerRegion", "innerRegion");
                                                Labels.FieldDoubleText("Outer", "Inner", rightSpacing: 3);
                                                element.Field("Zoom", "zoom");
                                                element.Slider("Smooth", "smooth");
                                        }

                                        bool eventOpen = FoldOut.FoldOutButton(element.Get("eventsFoldOut"), offsetY: -2);
                                        Fields.EventFoldOut(element.Get("onEnter"), element.Get("enterFoldOut"), "On Enter", execute: eventOpen);
                                        Fields.EventFoldOut(element.Get("onExit"), element.Get("exitFoldOut"), "On Exit", execute: eventOpen);
                                        ;
                                }
                                GUI.enabled = true;
                        }
                }
                public static void DrawTrigger (Safire2DCamera main, UnityEditor.Editor editor)
                {
                        if (!main.follow.regions.view || !main.follow.regions.enable)
                                return;

                        for (int i = 0; i < main.follow.regions.regions.Count; i++)
                        {
                                Region element = main.follow.regions.regions[i];
                                bool isTransform = element.type == RegionType.Transform && element.transform != null;
                                Color color = element.select == i ? Tint.PastelGreen : Color.white;
                                // move Center
                                if (!isTransform)
                                {
                                        element.position = SceneTools.MovePositionCircleHandle(element.position, editor: editor);
                                }
                                // Set positions
                                Vector3 center = isTransform ? element.transform.position : element.position;
                                if (isTransform)
                                        element.position = element.transform.position;
                                if (!isTransform && element.transform != null)
                                        element.transform.position = element.position;

                                SceneTools.Circle(center, element.outerRegion, color, element.outerRegion * 2f);
                                SceneTools.Circle(center, element.innerRegion, color, element.innerRegion * 2f);

                                if (Mouse.down && ((Vector2) Mouse.position - (Vector2) center).magnitude < element.outerRegion)
                                {
                                        for (int j = 0; j < main.follow.regions.regions.Count; j++)
                                        {
                                                main.follow.regions.regions[j].select = -1;
                                                main.follow.regions.regions[j].open = false;
                                        }
                                        element.select = i;
                                        element.open = true;
                                }
                        }
                }
                public static void Select (SerializedProperty array, int index)
                {
                        for (int i = 0; i < array.arraySize; i++)
                        {
                                array.Element(i).Get("select").intValue = -1;
                                if (index == i)
                                {
                                        array.Element(i).Get("select").intValue = index;
                                        SceneTools.MoveSceneCamera(array.Element(i).Get("position").vector3Value);
                                }
                        }
                }
#pragma warning restore 0414
#endif
                #endregion

        }

        public enum RegionType
        {
                Position,
                Transform
        }

}
