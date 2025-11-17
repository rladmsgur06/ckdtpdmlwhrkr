#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class Clamp : Action
        {
                [SerializeField] public SetLimits type;
                [SerializeField] public bool clampX;
                [SerializeField] public bool clampY;
                [SerializeField] public float distanceX;
                [SerializeField] public float distanceY;

                [SerializeField] public float left;
                [SerializeField] public float right;
                [SerializeField] public float up;
                [SerializeField] public float down;

                [SerializeField, HideInInspector] public SimpleBounds bounds = new SimpleBounds();

                private void Awake ()
                {
                        if (type == SetLimits.Automatically)
                        {
                                Vector3 p = this.transform.position;
                                left = p.x - distanceX;
                                right = p.x + distanceX;
                                down = p.y - distanceY;
                                up = p.y + distanceY;
                        }
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        Vector3 p = this.transform.position;
                        if (clampX)
                        {
                                if (root.position.x <= left && root.velocity.x < 0)
                                {
                                        this.transform.position = new Vector3(left, p.y, p.z);
                                        root.velocity.x = 0;
                                }
                                if (root.position.x >= right && root.velocity.x > 0)
                                {
                                        this.transform.position = new Vector3(right, p.y, p.z);
                                        root.velocity.x = 0;
                                }
                        }
                        if (clampY)
                        {
                                if (root.position.y <= down && root.velocity.y < 0)
                                {
                                        this.transform.position = new Vector3(p.x, down, p.z);
                                        root.velocity.y = 0;
                                }
                                if (root.position.y >= up && root.velocity.y > 0)
                                {
                                        this.transform.position = new Vector3(p.x, up, p.z);
                                        root.velocity.y = 0;
                                }
                        }
                        return NodeState.Running;
                }

                public bool IsClamping (Root root)
                {
                        if (clampX)
                        {
                                if (root.position.x <= left || root.position.x >= right)
                                {
                                        return true;
                                }
                        }
                        if (clampY)
                        {
                                if (root.position.y <= down || root.position.y >= up)
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                public bool IsClampingLeft (Root root)
                {
                        return clampX && root.position.x <= left;
                }

                public bool IsClampingRight (Root root)
                {
                        return clampX && root.position.x >= right;
                }

                public bool IsClampingUp (Root root)
                {
                        return clampY && root.position.y >= up;
                }

                public bool IsClampingDown (Root root)
                {
                        return clampY && root.position.y <= down;
                }

                public enum SetLimits
                {
                        Automatically,
                        Manually
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
                public override bool HasNextState () { return false; }
                public override void OnSceneGUI (UnityEditor.Editor editor)
                {
                        if (Application.isPlaying || type == SetLimits.Manually)
                        {
                                if (clampX)
                                {
                                        Vector3 px = this.transform.position;
                                        px.x = left;
                                        Draw.DrawRay(px + Vector3.up, Vector2.down * 2f, Color.red);
                                        px.x = right;
                                        Draw.DrawRay(px + Vector3.up, Vector2.down * 2f, Color.red);
                                }
                                if (clampY)
                                {
                                        Vector3 py = this.transform.position;
                                        py.y = up;
                                        Draw.DrawRay(py - Vector3.right, Vector2.right * 2f, Color.red);
                                        py.y = down;
                                        Draw.DrawRay(py - Vector3.right, Vector2.right * 2f, Color.red);
                                }
                                return;
                        }
                        Vector3 p = this.transform.position;
                        if (clampX)
                        {
                                Draw.DrawRay(p - Vector3.right * distanceX + Vector3.up, Vector2.down * 2f, Color.red);
                                Draw.DrawRay(p + Vector3.right * distanceX + Vector3.up, Vector2.down * 2f, Color.red);
                        }
                        if (clampY)
                        {
                                Draw.DrawRay(p + Vector3.down * distanceY - Vector3.right, Vector2.right * 2f, Color.red);
                                Draw.DrawRay(p + Vector3.up * distanceY - Vector3.right, Vector2.right * 2f, Color.red);
                        }

                }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(75, "Clamp the AI's position. The limits will be determined by the distance from the player at scene start. Or the limits can be placed manually." +
                                        "\n \nReturns Running");
                        }

                        int type = parent.Enum("type");
                        FoldOut.Box(3, color, offsetY: -2);
                        parent.Field("Set", "type");
                        if (type == 0)
                        {
                                parent.FieldAndEnable("Clamp X", "distanceX", "clampX");
                                parent.FieldAndEnable("Clamp Y", "distanceY", "clampY");
                        }
                        else
                        {
                                parent.FieldDoubleAndEnable("Clamp X", "left", "right", "clampX");
                                Labels.FieldDoubleText("Left", "Right", rightSpacing: 18);
                                parent.FieldDoubleAndEnable("Clamp Y", "up", "down", "clampY");
                                Labels.FieldDoubleText("Up", "Down", rightSpacing: 18);
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#endif
                #endregion

        }

}
