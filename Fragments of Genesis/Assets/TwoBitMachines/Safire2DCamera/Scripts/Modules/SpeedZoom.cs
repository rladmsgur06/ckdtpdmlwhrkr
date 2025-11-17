#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.Safire2DCamera
{
        [System.Serializable]
        public class SpeedZoom
        {
                [SerializeField] public bool enable;
                [SerializeField] public List<SpeedZoomPacket> list = new List<SpeedZoomPacket>();

                [System.NonSerialized] private Vector3 previousPosition;
                [System.NonSerialized] private Follow follow;
                [System.NonSerialized] private Zoom zoom;

                public void Initialize (Follow follow, Zoom zoom)
                {
                        this.follow = follow;
                        this.zoom = zoom;
                }

                public void Reset ()
                {
                        previousPosition = follow.TargetPosition();
                }

                public void Execute ()
                {
                        if (enable == false || Time.deltaTime == 0)
                        {
                                previousPosition = follow.TargetPosition();
                                return;
                        }

                        Vector2 speed = (follow.TargetPosition() - previousPosition) / Time.deltaTime;
                        previousPosition = follow.TargetPosition();

                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                                if (list[i].Check(speed, zoom))
                                        return;
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public int signalIndex;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool active;
                [SerializeField, HideInInspector] public bool close;
                [SerializeField, HideInInspector] public bool edit;
                [SerializeField, HideInInspector] public bool add;
                public static void CustomInspector (SerializedProperty parent, Color barColor, Color labelColor)
                {
                        if (!parent.Bool("edit"))
                                return;

                        if (Follow.Open(parent, "Speed Zoom", barColor, labelColor, true))
                        {
                                SerializedProperty array = parent.Get("list");
                                if (parent.ReadBool("add"))
                                {
                                        array.arraySize++;
                                }

                                if (array.arraySize == 0)
                                        Layout.VerticalSpacing(5);

                                GUI.enabled = parent.Bool("enable");
                                for (int i = 0; i < array.arraySize; i++)
                                {
                                        SerializedProperty element = array.Element(i);
                                        FoldOut.BoxSingle(1, color: FoldOut.boxColor);
                                        {
                                                Fields.ConstructField();
                                                Fields.ConstructSpace(Layout.labelWidth);
                                                element.ConstructField("axis", S.CW - S.B);
                                                if (Fields.ConstructButton("Delete"))
                                                { array.DeleteArrayElement(i); break; }
                                                ListReorder.Grip(parent, array, Layout.GetLastRect(20, 20), i, Tint.WarmWhite);
                                        }
                                        Layout.VerticalSpacing(2);

                                        FoldOut.Box(3, color: FoldOut.boxColor, offsetY: -2);
                                        {
                                                element.Field("Threshold", "threshold");
                                                element.Slider("Zoom", "scale", 0.1f, 5f);
                                                element.Slider("Smooth", "smooth", 0.01f, 1f);
                                        }
                                        Layout.VerticalSpacing(3);
                                }
                                GUI.enabled = true;
                        }
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        [System.Serializable]
        public class SpeedZoomPacket
        {
                [SerializeField] public AxisAll axis;
                [SerializeField] public float scale = 1f;
                [SerializeField] public float smooth = 0.75f;
                [SerializeField] public float threshold = 0f;
                [SerializeField, HideInInspector] public bool open;

                public bool Check (Vector2 speed, Zoom zoom)
                {
                        if (axis == AxisAll.Horizontal & Mathf.Abs(speed.x) >= threshold)
                        {
                                zoom.Set(scale: scale, speed: smooth, isTween: false);
                                return true;
                        }
                        if (axis == AxisAll.Vertical && Mathf.Abs(speed.y) >= threshold)
                        {
                                zoom.Set(scale: scale, speed: smooth, isTween: false);
                                return true;
                        }
                        if (axis == AxisAll.Both && speed.sqrMagnitude >= (threshold * threshold))
                        {
                                zoom.Set(scale: scale, speed: smooth, isTween: false);
                                return true;
                        }
                        return false;
                }
        }

        public enum AxisAll
        {
                Horizontal,
                Vertical,
                Both
        }
}
