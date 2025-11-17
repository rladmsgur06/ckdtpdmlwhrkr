using UnityEditor;
using UnityEngine;
using System.Text;
using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.Timeline;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Chronos) , true)]
        public class ChronosEditor : UnityEditor.Editor
        {
                public Chronos main;
                public SerializedObject parent;
                public ChronosVariables info = new ChronosVariables();
                public ActionWindowEditor actionWindow = new ActionWindowEditor();
                public StringBuilder timerLabel = new StringBuilder();
                public Rect window;
                public GUIStyle style = new GUIStyle();
                public StringBuilder sb = new StringBuilder();

                private void OnEnable ()
                {
                        main = target as Chronos;
                        parent = serializedObject;
                        Layout.Initialize();
                        main.lineMaterial = main.lineMaterial == null ? new Material(Shader.Find("Sprites/Default")) : main.lineMaterial;
                        info.Initialize(parent);
                        actionWindow.Initialize(this);

                        style.fontSize = 10;
                        style.fontStyle = FontStyle.Bold;
                        style.normal.textColor = Tint.BoxTwo;

                        main.transform.position = Vector3.zero;
                        main.transform.localScale = Vector3.one;
                        main.transform.localEulerAngles = Vector3.zero;
                        main.transform.hideFlags = HideFlags.HideInInspector;
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(50);
                        parent.Update();
                        {
                                float offset = info.sideBar;
                                window = Layout.CreateRect(Layout.infoWidth - offset - 3 , 139 , offset , 0);
                                Rect scrollX = Layout.CreateRect(Layout.infoWidth - offset - 3 , 8 , offset , 0);
                                Rect timeline = new Rect(window) { y = window.y - 25f , height = 25f };
                                Rect scrollY = new Rect(window) { x = window.x + window.width , width = 8 };
                                Rect track = new Rect(window) { x = window.x - offset - 11 , width = offset + 11 };
                                Rect addTrack = new Rect(window) { x = window.x - offset - 11 , y = timeline.y , width = offset + 11 , height = 25 };
                                Rect background = new Rect() { x = track.x , y = timeline.y , width = track.width + window.width + scrollY.width , height = timeline.height + window.height + scrollX.height };
                                background.DrawRect(Tint.SoftDark , Icon.Get("BackgroundLight128x128"));

                                ////////////////// place first
                                TrackProperties(info);
                                /////////////////
                                ScrollX(scrollX , window , info);
                                ScrollY(scrollY , info);
                                TimelineBar(timeline , info);
                                TrackSelectWindow(track , addTrack , info);
                                actionWindow.Execute(window);


                        }
                        parent.ApplyModifiedProperties();
                        DetectChangeEditor.RecordState(main , info);
                        Layout.VerticalSpacing(20);


                        if (FoldOut.LargeButton("Play" , Tint.Green , Tint.WarmWhite , Icon.Get("BackgroundLight")))
                        {
                                main.play.InitializeAuto(main);
                                EditorApplication.update -= main.play.PlayAuto;
                                EditorApplication.update += main.play.PlayAuto;
                        }
                        if (FoldOut.LargeButton("Stop" , Tint.Delete , Tint.WarmWhite , Icon.Get("BackgroundLight")))
                        {
                                main.play.StopAuto();
                                main.play.StopScrub();//
                                EditorApplication.update -= main.play.PlayAuto;
                        }
                        if (FoldOut.LargeButton("Pause" , Tint.Orange , Tint.WarmWhite , Icon.Get("BackgroundLight")))
                        {
                                main.play.PauseAuto();
                        }
                        if (Application.isPlaying && main.play.isPlaying)
                        {
                                main.play.StopAuto();
                                main.play.StopScrub();
                                EditorApplication.update -= main.play.PlayAuto;
                        }
                        if (Application.isPlaying || main.play.isPlaying)
                        {
                                float time = main.play.isPlaying ? main.play.time : main.time;
                                timerLabel.Length = 0;
                                timerLabel.Append(time.ToString("F2"));
                                GUILayout.Label(timerLabel.ToString());
                                TimePointer(time);
                        }
                        if (!Application.isPlaying && !main.play.isPlaying)
                        {
                                ScrubTimePointer(main.play.scrubTime);
                        }
                }

                private void TimelineBar (Rect window , ChronosVariables info)
                {
                        // zoom!
                        if (Event.current.type == EventType.ScrollWheel && Event.current.delta.y != 0)
                        {
                                float dir = Event.current.delta.y > 0 ? -1 : 1f;
                                float inc = info.zoom >= 200 ? 50f : 10f;
                                inc = info.zoom >= 500 ? 100f : inc;
                                inc = info.zoom >= 1000 ? 500f : inc;
                                info.zoom = Mathf.Clamp(info.zoom + inc * dir , -20f , 6000f);
                                if (info.zoom > -20f && info.zoom < 300f)
                                {
                                        info.scrollX = Mathf.Clamp(info.scrollX + 5f * dir , 0 , window.width);
                                }
                                Event.current.Use();
                                Repaint();
                        }
                        if (Event.current.type != EventType.Repaint || main.lineMaterial == null)
                        {
                                return;
                        }

                        GUI.BeginGroup(window);// everything starts at 0,0 and is clipped 

                        window.x = 0;
                        window.y = 0;

                        bool showMilli = info.zoom >= 2000;
                        float timeStep = info.time;
                        float halfStep = timeStep * 0.5f;
                        float smallStep = halfStep * 0.2f;
                        float milliStep = smallStep * 0.25f;
                        float top = 13f;
                        float bottom = 24f;
                        float lineHeight = showMilli ? 3 : 5f;
                        float contentLength = Mathf.Max(info.contentLengthX , window.width);
                        float offset = ScrollBar.ContentOffset(30f , info.scrollX , window.width , info.contentLengthX);
                        window.DrawRect(Tint.Box);

                        Draw.GLStart(main.lineMaterial);
                        {
                                GL.Color(Tint.BoxTwo);
                                if (showMilli)
                                {
                                        for (float i = milliStep; i < contentLength; i += milliStep) // 1 sec
                                        {
                                                if ((i - offset) <= 0 || (i - offset) >= window.width)
                                                        continue;
                                                DrawLine(new Vector2(i - offset , top + lineHeight + 5) , new Vector2(i - offset , bottom));
                                        }
                                }

                                for (float i = smallStep; i < contentLength; i += smallStep) // 1 sec
                                {
                                        if ((i - offset) <= 0 || (i - offset) >= window.width)
                                                continue;
                                        DrawLine(new Vector2(i - offset , top + lineHeight + 3) , new Vector2(i - offset , bottom));
                                }
                                for (float i = halfStep; i < contentLength; i += timeStep) // 5 sec
                                {
                                        if ((i - offset) <= 0 || (i - offset) >= window.width)
                                                continue;
                                        DrawLine(new Vector2(i - offset , top + lineHeight) , new Vector2(i - offset , bottom));
                                }
                                for (float i = timeStep; i < contentLength; i += timeStep) // 10 sec
                                {
                                        if ((i - offset) <= 0 || (i - offset) >= window.width)
                                                continue;
                                        DrawLine(new Vector2(i - offset , top) , new Vector2(i - offset , bottom));
                                }
                                DrawLine(new Vector2(0 , 1) , new Vector2(window.width , 1));
                                DrawLine(new Vector2(0 , bottom + 1) , new Vector2(window.width , bottom + 1));
                        }
                        Draw.GLEnd();

                        timeStep = info.zoom >= 2000 ? timeStep * 0.025f : info.zoom >= 400 ? timeStep * 0.1f : info.zoom >= 40 ? timeStep * 0.5f : timeStep; // show more time stamps if zoomed in                                                                                                    //Debug.Log(timeStep * 0.05f);
                        for (float i = timeStep; i < contentLength; i += timeStep)
                        {
                                if ((i - offset) <= 0 || (i - offset) >= window.width)
                                {
                                        continue;
                                }

                                float time = i / smallStep;
                                int minutes = Mathf.FloorToInt(time / 60);
                                int seconds = Mathf.FloorToInt(time % 60);
                                float fractionalSeconds = (time % 1) * 100;

                                sb.Clear();
                                if (info.zoom >= 2000 && fractionalSeconds != 0)
                                {
                                        sb.AppendFormat("{0}.{1}" , seconds , fractionalSeconds);
                                }
                                else
                                {
                                        sb.AppendFormat("{0}:{1:00}" , minutes , seconds);
                                }
                                Rect label = new Rect(i - offset - 9 , 1 , 30 , 20);
                                GUI.Label(label , sb.ToString() , style);
                        }

                        GUI.EndGroup();
                }

                private void TimePointer (float time)
                {
                        if (Event.current.type != EventType.Repaint || main.lineMaterial == null)
                        {
                                return;
                        }

                        float width = 10f;
                        float height = 15f;
                        float halfWidth = width * 0.5f;
                        float contentOffsetX = ScrollBar.ContentOffset(30f , info.scrollX , window.width , info.contentLengthX);
                        float pointer = info.time / info.timeSeconds * time - contentOffsetX;
                        Rect bar = new Rect(window) { x = window.x + pointer - halfWidth , y = window.y - 30 , width = width , height = height };

                        if (bar.x >= (window.x - halfWidth) && bar.x <= (window.x + window.width - halfWidth))
                        {
                                Draw.GLStart(main.lineMaterial);
                                {
                                        Draw.GLCircle(new Vector2(bar.x + halfWidth , bar.y - 1) , 6 , Color.white);
                                        DrawLine(new Vector2(bar.x + halfWidth , bar.y + 4) , new Vector2(bar.x + halfWidth , bar.y + window.height + height + 13));
                                }
                                Draw.GLEnd();
                        }

                        Repaint();
                }


                private void ScrubTimePointer (float time)
                {

                        float width = 10f;
                        float height = 15f;
                        float halfWidth = width * 0.5f;
                        float contentOffsetX = ScrollBar.ContentOffset(30f , info.scrollX , window.width , info.contentLengthX);
                        float pointer = info.time / info.timeSeconds * time - contentOffsetX;

                        if (Event.current.type == EventType.MouseUp)
                        {
                                main.play.canScrub = false;
                        }
                        //  main.play.scrubTime = 0;
                        if (main.play.isScrubbing && main.play.canScrub && Event.current.type == EventType.MouseDrag && Event.current.delta.x != 0)
                        {
                                float newStartTime = (Event.current.mousePosition.x - window.x + contentOffsetX) * info.timeRate / info.time;
                                pointer = newStartTime / info.timeConvert;
                                main.play.PlayScrub(pointer);
                                Repaint();
                        }


                        Rect bar = new Rect(window) { x = window.x + pointer - halfWidth , y = window.y - 30 , width = width , height = height };

                        if (bar.ContainsMouseDown())
                        {
                                main.play.InitializeScrub(main);
                                main.play.canScrub = true;
                        }

                        if (Event.current.type != EventType.Repaint || main.lineMaterial == null)
                        {
                                return;
                        }

                        if (bar.x >= (window.x - halfWidth) && bar.x <= (window.x + window.width - halfWidth))
                        {
                                Draw.GLStart(main.lineMaterial);
                                {
                                        Draw.GLCircle(new Vector2(bar.x + halfWidth , bar.y - 1) , 6 , Color.white);
                                        DrawLine(new Vector2(bar.x + halfWidth , bar.y + 4) , new Vector2(bar.x + halfWidth , bar.y + window.height + height + 13));
                                }
                                Draw.GLEnd();
                        }

                        Repaint();
                }

                private void DrawLine (Vector2 p1 , Vector2 p2)
                {
                        GL.Vertex(p1);
                        GL.Vertex(p2);
                }

                private void TrackSelectWindow (Rect window , Rect addTrack , ChronosVariables info)
                {
                        bool addNewTrack = false;
                        if (Buttons.Button(addTrack , Icon.Get("Add") , Color.white , center: true , toolTip: "Add Track"))
                        {
                                addNewTrack = true;
                        }

                        GUI.BeginGroup(window);

                        float contentOffset = ScrollBar.ContentOffset(30f , info.scrollY , window.height , info.contentLengthY);
                        info.contentLengthY = 0;
                        Rect trackBoxOrigin = new Rect(window) { x = 3 , y = 2 , width = info.sideBar + 5 , height = 20 };


                        Texture2D background = Icon.Get("BackgroundLight128x128");
                        SerializedProperty trackArray = parent.Get("track");
                        SerializedProperty gripIndex = parent.Get("signalIndex");
                        SerializedProperty gripActive = parent.Get("active");
                        SerializedProperty trackDrag = parent.Get("trackDrag");

                        if (trackArray.arraySize == 0 || addNewTrack)
                        {
                                trackArray.arraySize++;
                                trackArray.LastElement().Get("action").arraySize = 0;
                        }

                        if (gripActive.boolValue && Event.current.type == EventType.MouseDrag && Time.time > ListReorder.timeStart + 0.15f)
                        {
                                trackDrag.boolValue = true;
                        }
                        if (Event.current.type == EventType.MouseUp)
                        {
                                trackDrag.boolValue = false;
                        }

                        for (int i = 0; i < trackArray.arraySize; i++)
                        {
                                int trackKey = 1000000 * (i + 1);
                                bool selected = info.trackKey == trackKey;
                                SerializedProperty track = trackArray.Element(i);

                                Rect trackBox = new Rect(trackBoxOrigin) { y = trackBoxOrigin.y - contentOffset };
                                Rect delete = new Rect(trackBox) { x = trackBox.x , y = trackBox.y + trackBox.height - 12 , width = 11 , height = 11 };
                                trackBox.DrawRect(selected ? Tint.PurpleDark * Tint.WarmGrey : Tint.PurpleDark , background);

                                if (Buttons.Button(delete , Icon.Get("Corner") , Tint.Delete , center: false , toolTip: "Delete Track"))
                                {
                                        track.Toggle("delete");
                                }
                                if (track.Bool("delete"))
                                {
                                        if (Buttons.Button(trackBox , Icon.Get("X") , Tint.Delete , center: true , toolTip: "Delete"))
                                        {
                                                info.track = null;
                                                trackArray.DeleteArrayElementAtIndex(i);
                                                break;
                                        }
                                }
                                else
                                {
                                        if (gripActive.boolValue && trackDrag.boolValue && gripIndex.intValue == i)
                                        {
                                                Rect grip = new Rect(trackBox);
                                                grip.CenterRectContent(Vector2.one * 10).DrawRect(Color.white , Icon.Get("Grip"));
                                        }
                                        else
                                        {
                                                Labels.CenteredAndClip(trackBox , track.String("name") , Tint.White , 10);
                                        }
                                }
                                if (ListReorder.GripRaw(parent , trackArray , trackBox , i))
                                {
                                        info.trackKey = trackKey;
                                }
                                if (trackBox.ContainsMouseDown(false))
                                {
                                        info.trackKey = trackKey;
                                        info.actionArray = track.Get("action");
                                        info.restoreState = true;
                                        selected = true;
                                }
                                if (selected)
                                {
                                        info.track = track;
                                }

                                float max = trackBoxOrigin.y + trackBoxOrigin.height;
                                info.contentLengthY = max >= info.contentLengthY ? max : info.contentLengthY;
                                trackBoxOrigin.y += 23f;
                        }

                        GUI.EndGroup();
                }

                private void ScrollX (Rect window , Rect actionWindow , ChronosVariables info)
                {
                        bool entered = actionWindow.ContainsMouseMiddleDown(false);
                        GUI.BeginGroup(window);
                        {
                                window.x = window.y = 0;
                                window.DrawRect(Tint.Box);

                                if (!ScrollBar.Unnecessary(30f , window.width , info.contentLengthX))
                                {
                                        float barSize = ScrollBar.BarSize(30f , window.width , info.contentLengthX);
                                        Rect scrollBarX = new Rect(window) { x = info.scrollX , y = window.height - 8 , width = barSize };
                                        info.scrollX = Scroll.X(scrollBarX , info.scrollX , 0 , window.width , Tint.Blue , Texture2D.whiteTexture , enter: entered);
                                }
                        }
                        GUI.EndGroup();
                }

                private void ScrollY (Rect window , ChronosVariables info)
                {
                        GUI.BeginGroup(window);

                        window.x = window.y = 0;
                        window.DrawRect(Tint.Box);

                        if (!ScrollBar.Unnecessary(30f , window.height , info.contentLengthY))
                        {
                                float barSize = ScrollBar.BarSize(30f , window.height , info.contentLengthY);
                                Rect scrollBarY = new Rect(window) { x = window.width - 8 , y = info.scrollY , height = barSize };
                                info.scrollY = Scroll.Y(scrollBarY , info.scrollY , 0 , window.height , Tint.Blue , Texture2D.whiteTexture);
                        }

                        GUI.EndGroup();
                }

                private void TrackProperties (ChronosVariables info)
                {
                        if (info.track != null)
                        {
                                FoldOut.Box(2 , Tint.PurpleDark);
                                {
                                        info.track.FieldDouble("Track Object" , "gameObject" , "name" , bold: true);
                                        Labels.FieldText("Name" , execute: info.track.String("name") == "");
                                        info.track.Field("Track Play" , "type" , bold: true);
                                }
                                Layout.VerticalSpacing(5);
                        }

                        if (info.action != null && info.actionArray != null)
                        {
                                FoldOut.Box(3 , Tint.Blue , extraHeight: 3);
                                {
                                        SerializedProperty startTime = info.action.Get("startTime");
                                        SerializedProperty endTime = info.action.Get("endTime");
                                        ActionType actionType = (ActionType) info.action.Enum("type");
                                        float startTimeP = startTime.floatValue;
                                        float endTimeP = endTime.floatValue;

                                        if (endTime.floatValue < startTime.floatValue)
                                        {
                                                endTime.floatValue = startTime.floatValue + 0.25f;
                                        }

                                        endTime.floatValue = Compute.Round(endTime.floatValue , 0.05f);
                                        startTime.floatValue = Compute.Round(startTime.floatValue , 0.05f);
                                        float duration = Compute.Round(endTime.floatValue - startTime.floatValue , 0.01f);

                                        info.action.Field("Action Type" , "type" , bold: true , execute: actionType == ActionType.Set);
                                        info.action.FieldDouble("Action Type" , "type" , "tween" , bold: true , execute: actionType == ActionType.Interpolate);


                                        if (actionType == ActionType.Set)
                                        {
                                                info.action.Field("" , "startTime");
                                                Labels.FieldText("Start" , rightSpacing: 3);
                                                endTime.floatValue = startTime.floatValue + 0.25f;
                                                duration = 0;
                                        }
                                        else
                                        {
                                                info.action.FieldDouble("" , "startTime" , "endTime");
                                                Labels.FieldDoubleText("Start" , "End" , rightSpacing: 3);
                                        }
                                        Labels.Label("Duration:  " + duration.ToString("F2") , Layout.GetLastRect(100 , 20 , -3) , bold: true);


                                        if (startTimeP != startTime.floatValue || endTimeP != endTime.floatValue)
                                        {
                                                ActionWindowEditor.AdjustActions(info.actionArray , info);
                                        }
                                        Labels.Label(info.action.Get("fieldChange").String("recorded") , true);
                                }

                                if (FoldOut.FoldOutButton(info.action.Get("eventsFoldOut")))
                                {
                                        Fields.EventFoldOut(info.action.Get("onActionEnter") , info.action.Get("enterFoldOut") , "On Action Enter" , color: Tint.Blue);
                                        Fields.EventFoldOut(info.action.Get("onActionStay") , info.action.Get("stayFoldOut") , "On Action Stay" , color: Tint.Blue);
                                }
                        }
                }

        }
}
