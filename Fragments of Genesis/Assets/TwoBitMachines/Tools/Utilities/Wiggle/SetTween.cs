using UnityEngine;

namespace TwoBitMachines
{
        public static class SetTween
        {
                public static Vector3 v = Vector3.right;

                public static void Get (Act act, ref Vector3 from, ref Vector3 to, Transform T, RectTransform rectT, bool useAnchors)
                {
                        if (rectT != null)
                        {
                                Vector3 rectPosition = useAnchors ? (Vector3) rectT.anchoredPosition : rectT.position;

                                if (act == Act.MoveTo2D)
                                {
                                        from = rectPosition;
                                }
                                else if (act == Act.MoveTo3D)
                                {
                                        from = rectPosition;
                                }
                                else if (act == Act.MoveToX)
                                {
                                        from = v * rectPosition.x;
                                }
                                else if (act == Act.MoveToY)
                                {
                                        from = v * rectPosition.y;
                                }
                                else if (act == Act.ScaleTo2D)
                                {
                                        from = rectT.localScale;
                                }
                                else if (act == Act.ScaleTo3D)
                                {
                                        from = rectT.localScale;
                                }
                                else if (act == Act.ScaleToX)
                                {
                                        from = v * rectT.localScale.x;
                                }
                                else if (act == Act.ScaleToY)
                                {
                                        from = v * rectT.localScale.y;
                                }
                                else if (act == Act.ScaleToZ)
                                {
                                        from = v * rectT.localScale.z;
                                }
                                else if (act == Act.RotateTo)
                                {
                                        from = rectT.localEulerAngles;
                                }
                                else if (act == Act.MoveX)
                                {
                                        from = v * rectPosition.x;
                                        to += from;
                                }
                                else if (act == Act.MoveY)
                                {
                                        from = v * rectPosition.y;
                                        to += from;
                                }
                                else if (act == Act.Move2D)
                                {
                                        from = rectPosition;
                                        to += from;
                                }
                                else if (act == Act.Move3D)
                                {
                                        from = rectPosition;
                                        to += from;
                                }
                        }
                        else if (T != null)
                        {
                                if (act == Act.MoveTo2D)
                                {
                                        from = T.localPosition;
                                }
                                else if (act == Act.MoveTo3D)
                                {
                                        from = T.localPosition;
                                }
                                else if (act == Act.MoveToX)
                                {
                                        from = v * T.localPosition.x;
                                }
                                else if (act == Act.MoveToY)
                                {
                                        from = v * T.localPosition.y;
                                }
                                else if (act == Act.MoveToZ)
                                {
                                        from = v * T.localPosition.z;
                                }
                                else if (act == Act.ScaleTo2D)
                                {
                                        from = T.localScale;
                                }
                                else if (act == Act.ScaleTo3D)
                                {
                                        from = T.localScale;
                                }
                                else if (act == Act.ScaleToX)
                                {
                                        from = v * T.localScale.x;
                                }
                                else if (act == Act.ScaleToY)
                                {
                                        from = v * T.localScale.y;
                                }
                                else if (act == Act.ScaleToZ)
                                {
                                        from = v * T.localScale.z;
                                }
                                else if (act == Act.RotateTo)
                                {
                                        from = T.localEulerAngles;
                                }
                                else if (act == Act.MoveX)
                                {
                                        from = v * T.localPosition.x;
                                        to += from;
                                }
                                else if (act == Act.MoveY)
                                {
                                        from = v * T.localPosition.y;
                                        to += from;
                                }
                                else if (act == Act.MoveZ)
                                {
                                        from = v * T.localPosition.z;
                                        to += from;
                                }
                                else if (act == Act.Move2D)
                                {
                                        from = T.localPosition;
                                        to += from;
                                }
                                else if (act == Act.Move3D)
                                {
                                        from = T.localPosition;
                                        to += from;
                                }
                        }
                }

                public static void Set (Act act, float value, Transform T, RectTransform rectT, bool useAnchors)
                {
                        if (rectT != null)
                        {
                                Vector3 rectPosition = useAnchors ? (Vector3) rectT.anchoredPosition : rectT.position;

                                if (act == Act.MoveToX)
                                {
                                        rectPosition = new Vector2 (value, rectPosition.y);
                                }
                                else if (act == Act.MoveToY)
                                {
                                        rectPosition = new Vector2 (rectPosition.x, value);
                                }
                                else if (act == Act.MoveX)
                                {
                                        rectPosition = new Vector2 (value, rectPosition.y);
                                }
                                else if (act == Act.MoveY)
                                {
                                        rectPosition = new Vector2 (rectPosition.x, value);
                                }
                                else if (act == Act.ScaleToX)
                                {
                                        rectT.localScale = new Vector3 (value, rectT.localScale.y, rectT.localScale.z);
                                }
                                else if (act == Act.ScaleToY)
                                {
                                        rectT.localScale = new Vector3 (rectT.localScale.x, value, rectT.localScale.z);
                                }
                                else if (act == Act.ScaleToZ)
                                {
                                        rectT.localScale = new Vector3 (rectT.localScale.x, rectT.localScale.y, value);
                                }

                                if (useAnchors)
                                {
                                        rectT.anchoredPosition = rectPosition;
                                }
                                if (!useAnchors)
                                {
                                        rectT.position = rectPosition;
                                }
                        }
                        else if (T != null)
                        {
                                if (act == Act.MoveToX)
                                {
                                        T.localPosition = new Vector3 (value, T.localPosition.y, T.localPosition.z);
                                }
                                else if (act == Act.MoveToY)
                                {
                                        T.localPosition = new Vector3 (T.localPosition.x, value, T.localPosition.z);
                                }
                                else if (act == Act.MoveToZ)
                                {
                                        T.localPosition = new Vector3 (T.localPosition.x, T.localPosition.y, value);
                                }
                                else if (act == Act.MoveX)
                                {
                                        T.localPosition = new Vector3 (value, T.localPosition.y, T.localPosition.z);
                                }
                                else if (act == Act.MoveY)
                                {
                                        T.localPosition = new Vector3 (T.localPosition.x, value, T.localPosition.z);
                                }
                                else if (act == Act.MoveZ)
                                {
                                        T.localPosition = new Vector3 (T.localPosition.x, T.localPosition.y, value);
                                }
                                else if (act == Act.ScaleToX)
                                {
                                        T.localScale = new Vector3 (value, T.localScale.y, T.localScale.z);
                                }
                                else if (act == Act.ScaleToY)
                                {
                                        T.localScale = new Vector3 (T.localScale.x, value, T.localScale.z);
                                }
                                else if (act == Act.ScaleToZ)
                                {
                                        T.localScale = new Vector3 (T.localScale.x, T.localScale.y, value);
                                }
                        }
                }

                public static void Set (Act act, Vector2 value, Transform T, RectTransform rectT, bool useAnchors)
                {
                        if (rectT != null)
                        {
                                Vector3 rectPosition = useAnchors ? (Vector3) rectT.anchoredPosition : rectT.position;

                                if (act == Act.MoveTo2D)
                                {
                                        rectPosition = value;
                                }
                                else if (act == Act.Move2D)
                                {
                                        rectPosition = value;
                                }
                                else if (act == Act.ScaleTo2D)
                                {
                                        rectT.localScale = new Vector3 (value.x, value.y, rectT.localScale.z);
                                }

                                if (useAnchors)
                                {
                                        rectT.anchoredPosition = rectPosition;
                                }
                                if (!useAnchors)
                                {
                                        rectT.position = rectPosition;
                                }
                        }
                        else if (T != null)
                        {
                                if (act == Act.MoveTo2D)
                                {
                                        T.localPosition = new Vector3 (value.x, value.y, T.localPosition.z);
                                }
                                else if (act == Act.Move2D)
                                {
                                        T.localPosition = new Vector3 (value.x, value.y, T.localPosition.z);
                                }
                                else if (act == Act.ScaleTo2D)
                                {
                                        T.localScale = new Vector3 (value.x, value.y, T.localScale.z);
                                }
                        }
                }

                public static void Set (Act act, Vector3 value, Transform T, RectTransform rectT, bool useAnchors)
                {
                        if (rectT != null)
                        {
                                Vector3 rectPosition = useAnchors ? (Vector3) rectT.anchoredPosition : rectT.position;

                                if (act == Act.MoveTo3D)
                                {
                                        rectPosition = value;
                                }
                                else if (act == Act.Move3D)
                                {
                                        rectPosition = value;
                                }
                                else if (act == Act.ScaleTo3D)
                                {
                                        rectT.localScale = value;
                                }

                                if (useAnchors)
                                {
                                        rectT.anchoredPosition = rectPosition;
                                }
                                if (!useAnchors)
                                {
                                        rectT.position = rectPosition;
                                }
                        }
                        else if (T != null)
                        {
                                if (act == Act.MoveTo3D)
                                {
                                        T.localPosition = value;
                                }
                                else if (act == Act.Move3D)
                                {
                                        T.localPosition = value;
                                }
                                else if (act == Act.ScaleTo3D)
                                {
                                        T.localScale = value;
                                }
                        }
                }

                public static void Rotate (Act act, ref float total, float deltaTime, float turns, float seconds, Transform T, RectTransform rectT)
                {
                        float turnRate = TurnRate (ref total, deltaTime, turns, seconds);

                        if (rectT != null)
                        {
                                if (act == Act.RotateX)
                                {
                                        rectT.Rotate (Vector3.right, turnRate);
                                }
                                else if (act == Act.RotateY)
                                {
                                        rectT.Rotate (Vector3.up, turnRate);
                                }
                                else if (act == Act.RotateZ)
                                {
                                        rectT.Rotate (Vector3.forward, turnRate);
                                }
                        }
                        else if (T != null)
                        {
                                if (act == Act.RotateX)
                                {
                                        T.Rotate (Vector3.right, turnRate);
                                }
                                else if (act == Act.RotateY)
                                {
                                        T.Rotate (Vector3.up, turnRate);
                                }
                                else if (act == Act.RotateZ)
                                {
                                        T.Rotate (Vector3.forward, turnRate);
                                }
                        }
                }

                public static void RotateAround (Act act, Vector3 pivot, ref float total, float deltaTime, float turns, float seconds, Transform T, RectTransform rectT)
                {
                        float turnRate = TurnRate (ref total, deltaTime, turns, seconds);

                        if (rectT != null)
                        {
                                if (act == Act.RotateAroundX)
                                {
                                        rectT.RotateAround (pivot, Vector3.right, turnRate);
                                }
                                else if (act == Act.RotateAroundY)
                                {
                                        rectT.RotateAround (pivot, Vector3.up, turnRate);
                                }
                                else if (act == Act.RotateAroundZ)
                                {
                                        rectT.RotateAround (pivot, Vector3.forward, turnRate);
                                }
                        }
                        else if (T != null)
                        {
                                if (act == Act.RotateAroundX)
                                {
                                        T.RotateAround (pivot, Vector3.right, turnRate);
                                }
                                else if (act == Act.RotateAroundY)
                                {
                                        T.RotateAround (pivot, Vector3.up, turnRate);
                                }
                                else if (act == Act.RotateAroundZ)
                                {
                                        T.RotateAround (pivot, Vector3.forward, turnRate);
                                }
                        }
                }

                public static void SetQuaternion (Act act, Quaternion quaternion, Transform T, RectTransform rectT)
                {
                        if (rectT != null)
                        {
                                if (act == Act.RotateTo)
                                {
                                        rectT.rotation = quaternion;
                                }
                        }
                        else if (T != null)
                        {
                                if (act == Act.RotateTo)
                                {
                                        T.rotation = quaternion;
                                }
                        }
                }

                private static float TurnRate (ref float total, float deltaTime, float turns, float seconds)
                {
                        float turn = Mathf.Abs (turns);
                        float turnRate = turn * 360f * deltaTime;
                        float limit = turn * seconds * 360f;

                        if ((total + turnRate) > limit)
                        {
                                turnRate = limit - total;
                        }

                        total = Mathf.Clamp (total + turnRate, 0, limit);
                        return turnRate * Mathf.Sign (turns);
                }
        }

}