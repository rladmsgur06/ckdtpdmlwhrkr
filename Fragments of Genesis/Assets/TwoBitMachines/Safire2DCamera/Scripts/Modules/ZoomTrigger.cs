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
        public class ZoomTrigger
        {
                [SerializeField] public bool enable;
                [SerializeField] public List<ZoomPacket> zooms = new List<ZoomPacket>();
                [System.NonSerialized] private Zoom zoomController;

                public void Initialize (Zoom zoomController)
                {
                        this.zoomController = zoomController;
                        for (int i = 0; i < zooms.Count; i++)
                                zooms[i].bounds.Initialize();
                }

                public void Execute (Follow follow)
                {
                        if (!enable)
                                return;
                        Vector3 target = follow.TargetPosition();
                        for (int i = 0; i < zooms.Count; i++)
                        {
                                zooms[i].Execute(target , zoomController);
                        }
                }

                [System.Serializable]
                public class ZoomPacket
                {
                        [SerializeField] public SimpleBounds bounds = new SimpleBounds();
                        [SerializeField] public OnExit revert;
                        [SerializeField] public float scale = 1;
                        [SerializeField] public float smooth = 0.5f;
                        [SerializeField] public UnityEvent onEnter;
                        [SerializeField] public UnityEvent onExit;

                        [System.NonSerialized] private bool active = false;
                        [System.NonSerialized] private float originalZoomLevel = 1;

                        #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                        [SerializeField, HideInInspector] public int select = -1;
                        [SerializeField, HideInInspector] public bool eventsFoldOut;
                        [SerializeField, HideInInspector] public bool enterFoldOut;
                        [SerializeField, HideInInspector] public bool exitFoldOut;
#pragma warning restore 0414
#endif
                        #endregion

                        public void Reset ()
                        {
                                active = false;
                                originalZoomLevel = 1;
                        }

                        public void Execute (Vector3 position , Zoom zoom)
                        {
                                bool insideTrigger = bounds.Contains(position);

                                if (insideTrigger && !active)
                                {
                                        active = true;
                                        onEnter.Invoke();
                                        originalZoomLevel = zoom.scale != 0 ? zoom.scale : 1;
                                }
                                if (active)
                                {
                                        zoom.Set(scale: scale , speed: smooth , isTween: false);
                                }
                                if (active && !insideTrigger)
                                {
                                        active = false;
                                        onExit.Invoke();
                                        if (revert == OnExit.RevertOnExit)
                                                zoom.Set(scale: originalZoomLevel , duration: 1f);
                                }
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
                public static void CustomInspector (SerializedProperty parent , Color barColor , Color labelColor)
                {
                        if (!parent.Bool("edit"))
                                return;

                        if (Follow.Open(parent , "Zoom Trigger" , barColor , labelColor , true , canView: true))
                        {
                                GUI.enabled = parent.Bool("enable");
                                SerializedProperty array = parent.Get("zooms");

                                if (parent.ReadBool("add"))
                                {
                                        array.arraySize++;
                                        EditorTools.ClearProperty(array.LastElement());
                                        array.LastElement().Get("bounds").Get("position").vector2Value = SceneTools.SceneCenter(Vector2.zero);
                                        array.LastElement().Get("bounds").Get("size").vector2Value = new Vector2(5f , 5f);
                                        array.LastElement().Get("scale").floatValue = 1f;
                                        array.LastElement().Get("smooth").floatValue = 0.5f;
                                }

                                if (array.arraySize == 0)
                                        Layout.VerticalSpacing(5);

                                for (int i = 0; i < array.arraySize; i++)
                                {
                                        SerializedProperty zoom = array.Element(i);

                                        Color color = FoldOut.boxColor;
                                        FoldOut.Box(2 , color , extraHeight: 3);
                                        {
                                                Fields.ConstructField();
                                                Fields.ConstructString("" , S.LW);
                                                zoom.ConstructField("revert" , S.CW - S.B2);
                                                if (Fields.ConstructButton("Target"))
                                                { Follow.Select(array , i); }
                                                if (Fields.ConstructButton("Delete"))
                                                { array.DeleteArrayElement(i); break; }

                                                zoom.FieldDouble("Settings" , "scale" , "smooth");
                                                Labels.FieldDoubleText("Scale" , "Smooth" , rightSpacing: 3);
                                                zoom.Clamp("smooth");

                                                bool eventOpen = FoldOut.FoldOutButton(zoom.Get("eventsFoldOut"));
                                                Fields.EventFoldOut(zoom.Get("onEnter") , zoom.Get("enterFoldOut") , "On Enter" , color: color , execute: eventOpen);
                                                Fields.EventFoldOut(zoom.Get("onExit") , zoom.Get("exitFoldOut") , "On Exit" , color: color , execute: eventOpen);
                                        }
                                }
                                GUI.enabled = true;
                        }
                }

                public static void DrawTrigger (Safire2DCamera main)
                {
                        if (!main.zoomTrigger.view || !main.zoomTrigger.enable)
                                return;

                        for (int i = 0; i < main.zoomTrigger.zooms.Count; i++)
                        {
                                Color previousColor = Handles.color;
                                ZoomPacket element = main.zoomTrigger.zooms[i];
                                Handles.color = Tint.Purple;
                                SimpleBounds bounds = element.bounds;
                                SceneTools.DrawAndModifyBounds(ref bounds.position , ref bounds.size , element.select == i ? Tint.PastelGreen : Handles.color , 0.5f);

                                if (Mouse.down && bounds.DetectRaw(Mouse.position))
                                {
                                        for (int j = 0; j < main.zoomTrigger.zooms.Count; j++)
                                        {
                                                main.zoomTrigger.zooms[j].select = -1;
                                        }
                                        element.select = i;
                                }
                                Handles.color = previousColor;
                        }
                }

#pragma warning restore 0414
#endif
                #endregion
        }
}
