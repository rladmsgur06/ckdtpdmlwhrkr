using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.MapSystem
{
        public class PathUtil
        {
                public static bool cull = true;
                public static float colinear = 0.00002f;
                public static float overlap = 0.001f;
                private static float steps = 10f;
                private static int triangulationLimit = 1000;
                private static bool useSharpCorner = false;
                private static float cornerWidth = 0.1f;

                public static List<int> index = new List<int>();
                public static List<ushort> triangles = new List<ushort>();
                public static List<Color32> colors = new List<Color32>();
                public static List<Vector2> vertices = new List<Vector2>();

                public static List<Vector2> triangulateVertices = new List<Vector2>();
                public static List<Vector2> currentVertices = new List<Vector2>();
                public static List<Vector2> tempVertices = new List<Vector2>();
                public static List<Vector2> previousVertices = new List<Vector2>();
                public static List<Vector2> firstVertices = new List<Vector2>();

                public static void CreatePath (List<Point> pointList, int indexA, int indexB, float thickness, float resolution, bool createLine = true, bool addTriangles = true)
                {
                        tempVertices.Clear();
                        currentVertices.Clear();

                        Point bezierPointA = pointList[indexA];
                        Point bezierPointB = pointList[indexB];

                        float length = Length(bezierPointA, bezierPointB);
                        float deltaTime = resolution == 0 ? 0.01f : 1f / (steps * resolution * length);
                        GetPath(bezierPointA, bezierPointB, 0, 1f, deltaTime);

                        if (createLine)
                        {
                                if (addTriangles)
                                {
                                        PathUtil.triangulateVertices.AddRange(tempVertices);
                                }
                                PathThickness(thickness);
                        }
                }

                private static void GetPath (Point pointA, Point pointB, float start, float end, float inc)
                {
                        if (cull && pointA.offsetStart == Vector2.zero && pointB.offsetEnd == Vector2.zero) // if there is no curve, do not calculate bezier, much faster
                        {
                                AddTempPoint(pointA, pointB, start);
                                AddTempPoint(pointA, pointB, end);
                                return;
                        }
                        for (float time = start; time <= end; time += inc)
                        {
                                AddTempPoint(pointA, pointB, time);
                                if (time < end && time + inc >= end)
                                {
                                        AddTempPoint(pointA, pointB, end, checkForSamePoint: true);
                                        return;
                                }
                        }
                }

                private static void PathThickness (float thickness)
                {
                        if (tempVertices.Count == 1)
                        {
                                return;
                        }

                        int neighbor = 1;
                        for (int i = 0; i < tempVertices.Count; i++)
                        {
                                bool isLast = i == tempVertices.Count - 1;
                                Vector2 currentPoint = tempVertices[i];
                                Vector2 nextPoint = tempVertices[isLast ? i - 1 : i + 1];
                                Vector2 direction = isLast ? currentPoint - nextPoint : nextPoint - currentPoint;

                                for (int j = 1; j <= neighbor; j++)
                                {
                                        if (i + j < tempVertices.Count)
                                        {
                                                direction += (tempVertices[i + j] - currentPoint);
                                        }
                                        if (i - j >= 0)
                                        {
                                                direction += (currentPoint - tempVertices[i - j]);
                                        }
                                }
                                direction = new Vector2(-direction.y, direction.x).normalized;

                                SetPoint(-1, currentPoint, direction, thickness); // direction used to create left and right bezier points
                                SetPoint(+1, currentPoint, direction, thickness);

                        }
                }

                private static void SetPoint (int sign, Vector2 tempPoint, Vector2 tempDirection, float width)
                {
                        Vector2 direction = tempDirection * width * sign * 0.5f;
                        Vector2 position = tempPoint + direction;
                        currentVertices.Add(position);
                }

                private static void AddTempPoint (Point pointA, Point pointB, float time, bool checkForSamePoint = false)
                {
                        Vector2 position = Vector2.zero;
                        if (pointA.offsetStart == Vector2.zero && pointB.offsetEnd == Vector2.zero)
                        {
                                position = Vector2.Lerp(pointA.position, pointB.position, Mathf.Clamp(time, 0, 1f));//
                        }
                        else
                        {
                                position = BezierPoint(pointA, pointB, Mathf.Clamp(time, 0, 1f));
                        }
                        if (cull && checkForSamePoint && tempVertices.Count > 0 && tempVertices[tempVertices.Count - 1] == position)
                        {
                                return;
                        }
                        tempVertices.Add(position);
                        if (cull && tempVertices.Count > 2 && ArePointsCollinear(tempVertices))
                        {
                                tempVertices.RemoveAt(tempVertices.Count - 2);
                        }
                }

                public static float Length (Point a, Point b)
                {
                        //https://stackoverflow.com/questions/29438398/cheap-way-of-calculating-cubic-bezier-length faster method of estimating bezier arc length
                        float line = (b.position - a.position).magnitude;
                        float cont_net = (a.position - a.controlStart).magnitude + (b.controlEnd - a.controlStart).magnitude + (b.position - b.controlEnd).magnitude;
                        float length = (cont_net + line) / 2f;
                        return length;
                }

                public static Vector2 BezierPoint (Point a, Point b, float t)
                {
                        float u = 1f - t;
                        float tt = t * t;
                        float uu = u * u;
                        float uuu = uu * u;
                        float ttt = tt * t;

                        Vector2 p = uuu * a.position;
                        p += 3f * uu * t * a.controlStart;
                        p += 3f * u * tt * b.controlEnd;
                        p += ttt * b.position;
                        return p;
                }

                public static void CreateMeshLine (List<Vector2> vertices, MeshData meshData, Color color)
                {
                        int verticeCount = MeshInfo.vertices.Count;

                        for (int i = 0; i < vertices.Count - 1; i += 2)
                        {
                                MeshInfo.AddPoint(vertices[i + 0], color);
                                MeshInfo.AddPoint(vertices[i + 1], color);

                                if (i < vertices.Count - 2)
                                {
                                        MeshInfo.AddTriangle(meshData.shapeOffset + verticeCount, i, i + 1, i + 3);
                                        MeshInfo.AddTriangle(meshData.shapeOffset + verticeCount, i, i + 3, i + 2);
                                }
                        }
                }

                public static void CreateElbow (MeshData meshData, Vector2 center, Vector2 lastPosition, Vector2 nextPosition, float width, int divisions, Color32 color)
                {
                        Vector2 direction1 = lastPosition - center;
                        Vector2 direction2 = nextPosition - center;
                        float rawAngle = Vector2.Angle(direction1, direction2);
                        useSharpCorner = false;

                        if (rawAngle == 0)
                        {
                                return;
                        }
                        if (rawAngle <= 90.1f)
                        {
                                divisions = 2;
                        }

                        int verticeCount = MeshInfo.vertices.Count;
                        float stepAngle = rawAngle / (float) divisions;
                        int sign = Compute.CrossSign(direction1, direction2) < 0 ? 1 : -1;

                        if (rawAngle <= 90.1f)
                        {
                                Vector2 cornerDir1 = direction1.Rotate(-90f * sign);
                                Vector2 cornerDir2 = direction2.Rotate(90f * sign);
                                if (Compute.LineIntersection(nextPosition, nextPosition + cornerDir2 * 2f, lastPosition, lastPosition + cornerDir1 * 2f, out Vector2 intersect))
                                {
                                        cornerWidth = (intersect - center).magnitude;
                                        useSharpCorner = true;
                                }
                        }

                        direction1 = sign > 0 ? direction1 : -direction1;
                        stepAngle = sign > 0 ? -Mathf.Abs(stepAngle) : stepAngle;
                        direction1.Normalize();

                        MeshInfo.AddPoint(center, color);

                        for (int i = 0; i <= divisions; i++)
                        {
                                float radius = width * 0.5f;
                                if (i == 1 && useSharpCorner)
                                {
                                        radius = cornerWidth;
                                }
                                MeshInfo.AddPoint(center + Compute.Rotate(direction1 * radius, stepAngle * i), color);
                                MeshInfo.AddTriangle(meshData.shapeOffset + verticeCount, 0, sign < 0 ? i + 1 : i, sign < 0 ? i : i + 1);
                        }
                }

                public static void CreateHalfCircle (MeshData meshData, Vector2 center, Vector2 direction, float diameter, Color color, int cap)
                {
                        float angle = 180f / cap;
                        float radius = diameter * 0.5f;
                        int verticeCount = MeshInfo.vertices.Count;
                        MeshInfo.AddPoint(center, color);

                        for (int i = 0; i <= cap; i++)
                        {
                                MeshInfo.AddPoint(center + Compute.Rotate(direction, angle * i) * radius, color);
                                MeshInfo.AddTriangle(meshData.shapeOffset + verticeCount, 0, i + 1, i);
                        }
                }

                public static void Triangulate (List<Vector2> points, Color color, ushort indexOffset)
                {
                        int limit = 0;
                        MeshInfo.ClearTempLists();
                        List<int> index = MeshInfo.index;
                        List<ushort> triangles = MeshInfo.triangles;
                        List<Color32> colors = MeshInfo.colors;
                        List<Vector3> vertices = MeshInfo.vertices;

                        if (points == null || points.Count < 3)
                        {
                                return;
                        }

                        if (Compute.IsClockwise(points))
                        {
                                points.Reverse();
                        }

                        for (int i = 0; i < points.Count; i++)
                        {
                                vertices.Add(points[i]);
                                colors.Add(color);
                                index.Add(i);
                        }

                        while (index.Count > 3 && limit++ <= triangulationLimit) // limit will prevent infinite loops
                        {
                                for (int i = 0; i < index.Count; i++)
                                {
                                        int indexA = i - 1 < 0 ? index[index.Count - 1] : index[i - 1];
                                        int indexB = index[i];
                                        int indexC = i + 1 > index.Count - 1 ? index[0] : index[i + 1];

                                        Vector2 pointA = vertices[indexA];
                                        Vector2 pointB = vertices[indexB];
                                        Vector2 pointC = vertices[indexC];

                                        if (Compute.CrossSign(pointA - pointB, pointC - pointB) < 0f)
                                        {
                                                continue;
                                        }

                                        bool isTriangle = true;
                                        for (int j = 0; j < vertices.Count; j++)
                                        {
                                                if (j == indexB || j == indexA || j == indexC)
                                                {
                                                        continue;
                                                }
                                                if (Compute.IsPointInTriangle(pointA, pointB, pointC, vertices[j]))
                                                {
                                                        isTriangle = false;
                                                        break;
                                                }
                                        }

                                        if (isTriangle)
                                        {
                                                triangles.Add((ushort) (indexA + indexOffset));
                                                triangles.Add((ushort) (indexB + indexOffset));
                                                triangles.Add((ushort) (indexC + indexOffset));
                                                index.RemoveAt(i);
                                                break;
                                        }
                                }
                        }

                        triangles.Add((ushort) (index[0] + indexOffset));
                        triangles.Add((ushort) (index[1] + indexOffset));
                        triangles.Add((ushort) (index[2] + indexOffset));

                        if (limit > triangulationLimit)
                        {
                                Debug.Log("Animesh: possible mesh triangulation error");
                        }
                }

                public static void RemoveDuplicatePoints (List<Vector2> vertices)
                {
                        float startCount = vertices.Count;
                        float sqrMagnitude = overlap * overlap;

                        for (int i = 1; i < vertices.Count; i++) // start at 1, do not remove first point
                        {
                                Vector2 next = vertices[i];
                                Vector2 last = vertices[i - 1];
                                if ((next - last).sqrMagnitude < sqrMagnitude)
                                {
                                        vertices.RemoveAt(i);
                                        i--;
                                }
                                else if (i == vertices.Count - 1 && vertices.Count > 1)
                                {
                                        if ((next - vertices[0]).sqrMagnitude < sqrMagnitude)
                                        {
                                                vertices.RemoveAt(i);
                                                i--;
                                        }
                                }
                        }
                }

                public static void RemoveColinearPoints (List<Vector2> vertices)
                {
                        for (int i = 1; i < vertices.Count - 1; i++)
                        {
                                if (ArePointsCollinear(vertices[i - 1], vertices[i], vertices[i + 1]))
                                {
                                        vertices.RemoveAt(i);
                                        i--;
                                }
                        }
                }

                public static bool ArePointsCollinear (List<Vector2> vertices)
                {
                        Vector2 p1 = vertices[vertices.Count - 3];
                        Vector2 p2 = vertices[vertices.Count - 2];
                        Vector2 p3 = vertices[vertices.Count - 1];
                        float a = (p2.y - p1.y) * (p3.x - p2.x);
                        float b = (p3.y - p2.y) * (p2.x - p1.x);
                        return Mathf.Abs(a - b) < colinear;
                }

                public static bool ArePointsCollinear (Vector2 p1, Vector2 p2, Vector2 p3)
                {
                        float a = (p2.y - p1.y) * (p3.x - p2.x);
                        float b = (p3.y - p2.y) * (p2.x - p1.x);
                        return Mathf.Abs(a - b) < colinear;
                }

        }
}
