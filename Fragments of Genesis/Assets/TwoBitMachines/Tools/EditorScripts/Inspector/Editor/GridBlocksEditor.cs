using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Blocks))]
        public class GridBlocksEditor : UnityEditor.Editor
        {
                private Blocks main;
                private SerializedObject parent;
                private SpriteRenderer renderer;
                private Collider2D collider;

                private void OnEnable ()
                {
                        main = target as Blocks;
                        parent = serializedObject;
                        Layout.Initialize();
                        renderer = main.GetComponent<SpriteRenderer>();
                        collider = main.GetComponent<Collider2D>();
                        Mouse.holding = false;
                        Mouse.middleHold = false;
                        Tools.current = UnityEditor.Tool.Rect;
                }

                public void OnSceneGUI ()
                {
                        Layout.Update();

                        if (Mouse.MouseUp() || Mouse.dragging) //                   round
                        {
                                main.transform.position = Compute.Round(main.transform.position , 1f);
                                main.transform.localScale = Compute.Round(main.transform.localScale , 0.25f);
                                if (renderer != null)
                                        renderer.size = Compute.Round(renderer.size , 1f);
                        }
                        if (Mouse.middleHold && Mouse.MouseRightDown()) //          delete
                        {
                                if (collider.OverlapPoint(Mouse.position))
                                {
                                        Undo.DestroyObjectImmediate(main.gameObject);
                                        return;
                                }
                        }
                        if (Mouse.holding && Mouse.MouseRightDown()) //             clone
                        {
                                Mouse.holding = false;
                                Blocks block = MonoBehaviour.Instantiate(main , main.transform.position + Vector3.up , Quaternion.identity , main.transform.parent);
                                block.name = main.name;
                        }
                        else if (Mouse.MouseRightDown() && renderer != null) //     rotate
                        {
                                main.transform.RotateAround(renderer.bounds.center , Vector3.forward , 90f);
                        }
                }
        }
}
