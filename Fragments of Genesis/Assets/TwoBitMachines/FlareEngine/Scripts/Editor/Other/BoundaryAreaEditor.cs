using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(BoundaryArea))]
        public class BoundaryAreaEditor : UnityEditor.Editor
        {
                private void OnSceneGUI ()
                {
                        BoundaryArea boundaryArea = (BoundaryArea) target;
                        if (boundaryArea.SimpleBounds == null)
                                boundaryArea.SimpleBounds = new SimpleBounds();
                        boundaryArea.SimpleBounds.position = boundaryArea.transform.position;
                        Vector2 position = boundaryArea.SimpleBounds.position;
                        Vector2 size = boundaryArea.SimpleBounds.size;
                        SceneTools.DrawAndModifyBounds(ref boundaryArea.SimpleBounds.position , ref boundaryArea.SimpleBounds.size , Color.green , 1f);

                        if (position != boundaryArea.SimpleBounds.position || size != boundaryArea.SimpleBounds.size)
                        {
                                EditorUtility.SetDirty(boundaryArea);
                        }

                        var components = boundaryArea.GetComponents<BoxCollider2D>();
                        if (components.Length < 4)
                        {
                                boundaryArea.gameObject.AddComponent<BoxCollider2D>();
                        }

                        if (components.Length == 4)
                        {
                                var boxColLeft = components[0];
                                var boxColRight = components[1];
                                var boxColTop = components[2];
                                var boxColBottom = components[3];
                                float thickness = 1;

                                boxColLeft.offset = new Vector2(-thickness / 2 , size.y / 2);
                                boxColLeft.size = new Vector2(thickness , size.y + thickness * 2f);

                                boxColRight.offset = new Vector2(size.x + thickness / 2 , size.y / 2);
                                boxColRight.size = new Vector2(thickness , size.y + thickness * 2f);

                                boxColTop.offset = new Vector2(size.x / 2 , size.y + thickness / 2);
                                boxColTop.size = new Vector2(size.x , thickness);

                                boxColBottom.offset = new Vector2(size.x / 2 , -thickness / 2);
                                boxColBottom.size = new Vector2(size.x , thickness);
                        }
                        //Validate
                        boundaryArea.gameObject.layer = LayerMask.NameToLayer("World");
                }
        }
}
