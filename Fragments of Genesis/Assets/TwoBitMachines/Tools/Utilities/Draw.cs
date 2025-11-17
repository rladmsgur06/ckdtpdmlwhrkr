using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines
{
        public static class Draw
        {
                public static Material lineMaterial;

                public static void GLStart (int mode = GL.LINES)
                {
                        lineMaterial = lineMaterial == null ? new Material(Shader.Find("Sprites/Default")) : lineMaterial;
                        lineMaterial.SetPass(0);
                        GL.PushMatrix();
                        GL.Begin(mode);
                }

                public static void GLStart (Material material, int mode = GL.LINES)
                {
                        material.SetPass(0);
                        GL.PushMatrix();
                        GL.Begin(mode);
                }

                public static void GLEnd ()
                {
                        GL.End();
                        GL.PopMatrix();
                }

                public static void GLCross (Vector2 center, float length, Color color)
                {
                        GL.Color(color);

                        float halfLength = length * 0.5f;

                        Vector2 horizontalLeft = center + Vector2.left * halfLength;
                        Vector2 horizontalRight = center + Vector2.right * halfLength;
                        Vector2 verticalUp = center + Vector2.up * halfLength;
                        Vector2 verticalDown = center + Vector2.down * halfLength;

                        // Draw lines for the cross shape
                        GL.Vertex(horizontalLeft);
                        GL.Vertex(horizontalRight);

                        GL.Vertex(verticalUp);
                        GL.Vertex(verticalDown);
                }

                public static void GLDiamond (Vector2 center, float length, Color color)
                {
                        GL.Color(color);

                        float halfLength = length * 0.5f;

                        Vector2 horizontalLeft = center + Vector2.left * halfLength;
                        Vector2 horizontalRight = center + Vector2.right * halfLength;
                        Vector2 verticalUp = center + Vector2.up * halfLength;
                        Vector2 verticalDown = center + Vector2.down * halfLength;

                        GL.Vertex(horizontalLeft);
                        GL.Vertex(verticalUp);

                        GL.Vertex(verticalUp);
                        GL.Vertex(horizontalRight);

                        GL.Vertex(horizontalRight);
                        GL.Vertex(verticalDown);

                        GL.Vertex(verticalDown);
                        GL.Vertex(horizontalLeft);
                }

                public static void GLLine (Vector2 center, float length, float offset, Color color)
                {
                        GL.Color(color);

                        float halfLength = length * 0.5f;

                        Vector2 horizontalLeft = center + Vector2.left * halfLength + Vector2.up * offset;
                        Vector2 horizontalRight = center + Vector2.right * halfLength + Vector2.up * offset;

                        // Draw lines for the cross shape
                        GL.Vertex(horizontalLeft);
                        GL.Vertex(horizontalRight);

                }

                public static void GLLine (Vector2 pointA, Vector2 pointB, Color color)
                {
                        GL.Color(color);
                        GL.Vertex(pointA);
                        GL.Vertex(pointB);
                }

                public static void GLLine (Vector2 pointA, Vector2 pointB)
                {
                        GL.Vertex(pointA);
                        GL.Vertex(pointB);
                }

                public static void GLArrow (Vector2 center, float length, float angle, Color color)
                {
                        GL.Color(color);

                        Vector2 direction = Vector2.right.Rotate(angle);
                        Vector2 point = center + direction * length;
                        Vector2 left = direction.Rotate(90f);
                        Vector2 right = direction.Rotate(-90f);

                        // Draw lines for the cross shape
                        GL.Vertex(center + left * length * 0.5f);
                        GL.Vertex(point);

                        GL.Vertex(center + right * length * 0.5f);
                        GL.Vertex(point);
                }

                public static void GLCircleInit (Vector2 center, float radius, Color color, int segments = 5)
                {
                        GLStart();
                        GLCircle(center, radius, color, segments);
                        GLEnd();
                }

                public static void GLTriangleTopInit (Vector2 top, float length, Color color)
                {
                        GLStart(GL.TRIANGLES);
                        GL.Color(color);
                        Vector2 r = Vector2.right * length * 0.5f;
                        GL.Vertex3(top.x - r.x, top.y, 0);
                        GL.Vertex3(top.x + r.x, top.y, 0);
                        GL.Vertex3(top.x, top.y - length, 0);
                        GLEnd();
                }

                public static void GLCircle (Vector2 center, float radius, Color color, int segments = 5)
                {
                        GL.Color(color);
                        // segments is per quadrant, not total circle
                        Vector2 centerBottom = center + Vector2.down * radius;
                        float angle = 45f / segments;
                        float c = 2f * Mathf.PI * radius;
                        float quadrant = c / 4f;
                        float segment = quadrant / segments;
                        Vector2 startPoint = centerBottom;

                        for (int i = 0; i < segments * 4f; i++)
                        {
                                float curve = 1f + (i * 2f);
                                Vector2 direction = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle * curve) * segment, Mathf.Sin(Mathf.Deg2Rad * angle * curve) * segment);
                                GL.Vertex(startPoint);
                                GL.Vertex(startPoint + direction);
                                startPoint += direction;
                        }
                }

                public static void GLPartialCircle (Vector2 center, float radius, Color color, float lowerAngle, float upperAngle, int segments = 5)
                {
                        GL.Color(color);

                        // Convert angles to radians
                        lowerAngle = Mathf.Deg2Rad * lowerAngle;
                        upperAngle = Mathf.Deg2Rad * upperAngle;

                        // Calculate the starting and ending points of the partial circle
                        Vector2 startPoint = new Vector2(center.x + Mathf.Cos(lowerAngle) * radius, center.y + Mathf.Sin(lowerAngle) * radius);

                        // Calculate the angle step based on the number of segments
                        int seg = segments * 4;
                        float angleStep = (upperAngle - lowerAngle) / (seg);

                        // Draw the partial circle using GL.Vertex
                        for (int i = 0; i < seg; i++)
                        {
                                float angle = lowerAngle + i * angleStep;
                                float nextAngle = lowerAngle + (i + 1) * angleStep;

                                ///   GL.Vertex(center)
                                if (i == 0)
                                {
                                        GL.Vertex(center);
                                        GL.Vertex(center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
                                }
                                if (i == seg - 1)
                                {
                                        GL.Vertex(center);
                                        GL.Vertex(center + new Vector2(Mathf.Cos(nextAngle) * radius, Mathf.Sin(nextAngle) * radius));
                                }
                                GL.Vertex(center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
                                GL.Vertex(center + new Vector2(Mathf.Cos(nextAngle) * radius, Mathf.Sin(nextAngle) * radius));
                        }
                }

                public static void Triangle (Vector2 v1, Vector2 v2, Vector2 v3, Color32 c1, Color32 c2, Color32 c3)
                {
                        GL.Color(c1);
                        GL.Vertex(v1);
                        GL.Color(c2);
                        GL.Vertex(v2);
                        GL.Color(c3);
                        GL.Vertex(v3);
                }


                public static void TriangleFrame (Vector2 v1, Vector2 v2, Vector2 v3, Color32 c1)
                {
                        GL.Color(c1);
                        GL.Vertex(v1);
                        GL.Vertex(v2);

                        GL.Vertex(v2);
                        GL.Vertex(v3);

                        GL.Vertex(v3);
                        GL.Vertex(v1);
                }

                public static void DrawMesh (List<ushort> triangles, List<Vector3> vertices, Color color)
                {
                        for (int i = 0; i < triangles.Count; i += 3)
                        {
                                int index1 = triangles[i];
                                int index2 = triangles[i + 1];
                                int index3 = triangles[i + 2];

                                Vector2 v1 = vertices[index1];
                                Vector2 v2 = vertices[index2];
                                Vector2 v3 = vertices[index3];

                                Triangle(v1, v2, v3, color, color, color);
                        }
                }

                public static void DrawMeshFrame (List<ushort> triangles, List<Vector3> vertices, Color opacity)
                {
                        for (int i = 0; i < triangles.Count; i += 3)
                        {
                                int index1 = triangles[i];
                                int index2 = triangles[i + 1];
                                int index3 = triangles[i + 2];

                                Vector2 v1 = vertices[index1];
                                Vector2 v2 = vertices[index2];
                                Vector2 v3 = vertices[index3];

                                TriangleFrame(v1, v2, v3, Color.grey * opacity * 0.25f);
                        }
                }


                public static void Circle (Vector2 center, float radius, Color color, int segments = 5)
                {
                        Vector2 centerBottom = center + Vector2.down * radius;
                        float angle = 45f / segments;
                        float c = 2f * Mathf.PI * radius;
                        float quadrant = c / 4f;
                        float segment = quadrant / segments;
                        Vector2 startPoint = centerBottom;

                        for (int i = 0; i < segments * 4f; i++)
                        {
                                float curve = 1f + (i * 2f);
                                Vector2 direction = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle * curve) * segment, Mathf.Sin(Mathf.Deg2Rad * angle * curve) * segment);
                                Debug.DrawLine(startPoint, startPoint + direction, color);
                                startPoint += direction;
                        }
                }

                public static void BasicSquare (Vector2 point, float size, Color color)
                {
                        Vector2 lT = new Vector2(-1, 1);
                        Vector2 rT = new Vector2(1, 1);
                        Vector2 lB = new Vector2(-1, -1);
                        Vector2 rB = new Vector2(1, -1);

                        UnityEngine.Debug.DrawLine(point + lT * size, point + rT * size, color);
                        UnityEngine.Debug.DrawLine(point + rT * size, point + rB * size, color);
                        UnityEngine.Debug.DrawLine(point + rB * size, point + lB * size, color);
                        UnityEngine.Debug.DrawLine(point + lB * size, point + lT * size, color);
                }

                public static void CircleSector (Vector2 center, Vector2 direction, float radius, float angleP, float angleN, float sign, Color color, float precision = 10)
                {
                        Vector2 right = direction * radius;

                        Vector2 positiveDirection = Compute.RotateVector(right, angleP);
                        Vector2 negativeDirection = Compute.RotateVector(right, angleN);

                        if (sign < 0)
                        {
                                negativeDirection.x *= -1f;
                                positiveDirection.x *= -1f;
                        }

                        float totalAngle = Vector2.Angle(positiveDirection, negativeDirection);
                        float intervalAngle = Mathf.Abs(totalAngle) / precision;
                        Vector2 startDirection = negativeDirection;

                        for (int i = 0; i < precision; i++)
                        {
                                Vector2 currentEndPoint = center + startDirection;
                                startDirection = Compute.RotateVector(startDirection, intervalAngle * sign);
                                Debug.DrawLine(currentEndPoint, center + startDirection, color);
                        }
                        Debug.DrawLine(center, center + negativeDirection, color);
                        Debug.DrawLine(center, center + positiveDirection, color);

                }

                public static void Cross (Vector2 point, Vector2 size, Color color, float angle = 0)
                {
                        Vector2 up = Compute.RotateVector(new Vector2(0, 1), angle);
                        Vector2 right = Compute.RotateVector(new Vector2(1, 0), angle);
                        Vector2 left = Compute.RotateVector(new Vector2(-1, 0), angle);
                        Vector2 down = Compute.RotateVector(new Vector2(0, -1), angle);

                        UnityEngine.Debug.DrawLine(point, point + right * size.x, color);
                        UnityEngine.Debug.DrawLine(point, point + left * size.x, color);
                        UnityEngine.Debug.DrawLine(point, point + down * size.y, color);
                        UnityEngine.Debug.DrawLine(point, point + up * size.y, color);
                }

                public static void DrawRay (Vector2 point, Vector2 direction, Color color, float angle = 0)
                {
                        direction = Compute.RotateVector(direction, angle);
                        UnityEngine.Debug.DrawRay(point, direction, color);
                }

                public static void Grid2D (Vector2 position, Vector2 gridSize, Vector2 cellSize, Color color)
                {

                        if (cellSize.x == 0 || cellSize.y == 0)
                                return;

                        float linesX = gridSize.x / cellSize.x + 1f;
                        float linesY = gridSize.y / cellSize.y + 1f;

                        for (int i = 0; i < linesX; i++)
                        {
                                Vector2 p = position + Vector2.right * cellSize.x * i;
                                Debug.DrawLine(p, p + Vector2.up * gridSize.y, color);
                        }

                        for (int i = 0; i < linesY; i++)
                        {
                                Vector2 p = position + Vector2.up * cellSize.y * i;
                                Debug.DrawLine(p, p + Vector2.right * gridSize.x, color);

                        }
                }

                public static void Square (Vector2 center, float size, Color color, float angle = 0)
                {
                        Vector2 lT = Compute.RotateVector(new Vector2(-1, 1), angle);
                        Vector2 rT = Compute.RotateVector(new Vector2(1, 1), angle);
                        Vector2 lB = Compute.RotateVector(new Vector2(-1, -1), angle);
                        Vector2 rB = Compute.RotateVector(new Vector2(1, -1), angle);

                        UnityEngine.Debug.DrawLine(center + lT * size, center + rT * size, color);
                        UnityEngine.Debug.DrawLine(center + rT * size, center + rB * size, color);
                        UnityEngine.Debug.DrawLine(center + rB * size, center + lB * size, color);
                        UnityEngine.Debug.DrawLine(center + lB * size, center + lT * size, color);
                }

                public static void SquareCenter (Vector2 center, Vector2 size, Color color)
                {
                        Vector2 bL = center - size * 0.5f;
                        Vector2 tL = bL + Vector2.up * size.y;
                        Vector2 tR = tL + Vector2.right * size.x;
                        Vector2 bR = tR + Vector2.down * size.y;

                        Debug.DrawLine(bL, tL, color);
                        Debug.DrawLine(tL, tR, color);
                        Debug.DrawLine(tR, bR, color);
                        Debug.DrawLine(bL, bR, color);
                }

                public static void Square (Vector2 bottomLeft, Vector2 size, Color color)
                {
                        Vector2 bL = bottomLeft;
                        Vector2 tL = bL + Vector2.up * size.y;
                        Vector2 tR = tL + Vector2.right * size.x;
                        Vector2 bR = tR + Vector2.down * size.y;

                        Debug.DrawLine(bL, tL, color);
                        Debug.DrawLine(tL, tR, color);
                        Debug.DrawLine(tR, bR, color);
                        Debug.DrawLine(bL, bR, color);

                }

                public static void Rectangle (Vector2 bL, Vector2 bR, Vector2 tR, Vector2 tL, Color color)
                {
                        Debug.DrawLine(bL, tL, color);
                        Debug.DrawLine(tL, tR, color);
                        Debug.DrawLine(tR, bR, color);
                        Debug.DrawLine(bL, bR, color);

                }

                public static void Square (Rect rect, Color color)
                {

                        Vector2 p = rect.position;
                        Vector2 r = Vector2.right * rect.width;
                        Vector2 u = Vector2.up * rect.height;

                        UnityEngine.Debug.DrawLine(p, p + u, color);
                        UnityEngine.Debug.DrawLine(p + u, p + u + r, color);
                        UnityEngine.Debug.DrawLine(p + u + r, p + r, color);
                        UnityEngine.Debug.DrawLine(p + r, p, color);
                }

        }

}
