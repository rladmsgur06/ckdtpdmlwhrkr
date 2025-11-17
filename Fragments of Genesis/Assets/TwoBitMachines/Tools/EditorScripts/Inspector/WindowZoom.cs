#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        //https://cdn2.hubspot.net/hubfs/2603837/CustomZoomableEditorWindowsinUnity3D-2.pdf?t=1504038261535
        [System.Serializable]
        public class WindowZoom // window zoom and draw grid 
        {
                private static readonly Color lightColor = new Color(0f, 0f, 0f, 0.08f);
                private static readonly Color darkColor = new Color(0f, 0f, 0f, 0.04f);
                public float scaling = 1f;
                public Matrix4x4 oldGUIMatrix;
                public Vector2 ScrollPosition;

                public Rect GroupRect;
                public const float MaxGraphSize = 20000.0f;

                public void Begin (Rect rect, float zoomScaling, Vector2 zoomPosition, Matrix4x4 guiMatrix)
                {
                        this.scaling = zoomScaling;
                        this.ScrollPosition = zoomPosition;
                        oldGUIMatrix = guiMatrix;

                        ProcessScrollWheel(Event.current);

                        ScaleWindowGroup();
                        ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition, false, false);
                        ScaleScrollGroup(rect);

                        oldGUIMatrix = GUI.matrix;
                        Matrix4x4 translation = Matrix4x4.TRS(new Vector3(0, 21, 1), Quaternion.identity, Vector3.one);
                        Matrix4x4 scale = Matrix4x4.Scale(new Vector3(scaling, scaling, scaling));
                        GUI.matrix = translation * scale * translation.inverse;
                        GUILayout.BeginArea(new Rect(0, 0, MaxGraphSize * scaling, MaxGraphSize * scaling));
                }

                public void End (Rect rect)
                {
                        GUILayout.EndArea();
                        GUI.matrix = oldGUIMatrix;

                        EditorGUILayout.EndScrollView(); // stop the scrollable view.
                                                         // reset the windows group for any additional content
                        GUI.EndGroup();
                        GUI.BeginGroup(new Rect(0, 0, rect.width, rect.height));
                }

                public void SetVariables (ref float zoomScaling, ref Vector2 zoomPosition, ref Matrix4x4 guiMatrix)
                {
                        zoomScaling = scaling;
                        zoomPosition = ScrollPosition;
                        guiMatrix = oldGUIMatrix;
                }

                private void ProcessScrollWheel (Event currentEvent)
                {
                        if (currentEvent.type == EventType.ScrollWheel)
                        {
                                float shiftMultiplier = currentEvent.shift ? 4 : 2;
                                scaling = Mathf.Clamp(scaling - currentEvent.delta.y * 0.01f * shiftMultiplier, 0.50f, 1.5f);
                                scaling = Compute.Round(scaling, 0.1f);
                                currentEvent.Use();
                                //  Debug.Log(ScrollPosition);
                        }
                }

                private void ScaleWindowGroup ()
                {
                        GUI.EndGroup();
                        GroupRect.x = 0;
                        GroupRect.y = 21;
                        GroupRect.width = (MaxGraphSize + ScrollPosition.x) / scaling;
                        GroupRect.height = (MaxGraphSize + ScrollPosition.y) / scaling;
                        GUI.BeginGroup(GroupRect);
                }

                private void ScaleScrollGroup (Rect rect)
                {
                        GUI.EndGroup();
                        GroupRect.x = -ScrollPosition.x / scaling;
                        GroupRect.y = -ScrollPosition.y / scaling;
                        GroupRect.width = (rect.width + ScrollPosition.x) / scaling + 1;
                        GroupRect.height = (rect.height + ScrollPosition.y) / scaling;
                        GUI.BeginGroup(GroupRect);
                }

                public void Grid (Material lineMaterial, float offsetX = 0, float offsetY = 0)
                {
                        if (Event.current.type != EventType.Repaint || lineMaterial == null)
                        {
                                return;
                        }

                        lineMaterial.SetPass(0);

                        GL.PushMatrix();
                        GL.Begin(GL.LINES);
                        {
                                DrawGridLines(15.0f, lightColor, offsetX, offsetY);
                                // DrawGridLines (50.0f, darkColor, zoom,  offsetX, offsetY);
                        }
                        GL.End();
                        GL.PopMatrix();
                }

                public void DrawGridLines (float gridSize, Color gridColor, float offsetX = 0, float offsetY = 0)
                {
                        GL.Color(gridColor);
                        Vector2 screenSize = new Vector2(GroupRect.width, GroupRect.height);
                        for (float x = offsetX; x < screenSize.x; x += gridSize)
                        {
                                if (x < 0)
                                        continue;
                                DrawLine(new Vector2(x, 0), new Vector2(x, screenSize.y));
                        }
                        for (float x = offsetX - gridSize; x >= 0; x -= gridSize) // since it's offset, draw remaining lines going in opposite direction
                        {
                                if (x > screenSize.x)
                                        continue;
                                DrawLine(new Vector2(x, 0), new Vector2(x, screenSize.y));
                        }
                        for (float y = offsetY; y <= screenSize.y; y += gridSize)
                        {
                                if (y < 0)
                                        continue;
                                DrawLine(new Vector2(0, y), new Vector2(screenSize.x, y));
                        }
                        for (float y = offsetY - gridSize; y > 0; y -= gridSize)
                        {
                                if (y > screenSize.y)
                                        continue;
                                DrawLine(new Vector2(0, y), new Vector2(screenSize.x, y));
                        }
                }

                private void DrawLine (Vector2 p1, Vector2 p2)
                {
                        GL.Vertex(p1);
                        GL.Vertex(p2);
                }
        }
}
#endif
