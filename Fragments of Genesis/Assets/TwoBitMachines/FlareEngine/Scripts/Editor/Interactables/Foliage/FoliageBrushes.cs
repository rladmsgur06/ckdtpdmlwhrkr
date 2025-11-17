using System.Collections.Generic;
using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.Interactables;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        public class FoliageBrushes
        {
                private float placementRange = 1;
                private Vector2 mouseDownPosition;
                private List<int> range = new List<int>();

                public void SetBrushSprite (Foliage main , SerializedProperty index , SpriteRenderer renderer , GameObject brush)
                {
                        if (brush != null && renderer != null && main.brushes == FoliageBrush.Single)
                        {
                                for (int i = 0; i < main.textureArray.textures.Count; i++)
                                {
                                        if (index.intValue == i && (renderer.sprite == null || renderer.sprite.texture != main.textureArray.textures[i].texture))
                                        {
                                                Texture2D texture = main.textureArray.textures[i].texture;
                                                if (texture == null)
                                                        continue;
                                                renderer.sprite = Sprite.Create(texture , new Rect(0.0f , 0.0f , texture.width , texture.height) , new Vector2(0 , 0) , main.textureArray.PPU);
                                                return;
                                        }
                                }
                        }
                        if (brush != null && renderer != null && main.brushes == FoliageBrush.Random)
                        {
                                for (int i = 0; i < main.textureArray.textures.Count; i++)
                                {
                                        if (main.textureArray.textures[i].isRandom)
                                        {
                                                Texture2D texture = main.textureArray.textures[i].texture;
                                                if (texture == null)
                                                        continue;

                                                if (renderer.sprite == null)
                                                {
                                                        renderer.sprite = Sprite.Create(texture , new Rect(0.0f , 0.0f , texture.width , texture.height) , new Vector2(0 , 0) , main.textureArray.PPU);
                                                        return;
                                                }
                                                else if (renderer.sprite.texture == texture)
                                                {
                                                        return;
                                                }
                                                else if (renderer.sprite.texture != texture)
                                                {
                                                        renderer.sprite = Sprite.Create(texture , new Rect(0.0f , 0.0f , texture.width , texture.height) , new Vector2(0 , 0) , main.textureArray.PPU);
                                                        return;
                                                }
                                        }
                                }
                        }
                }

                public void SingleBrush (SerializedObject parent , SerializedProperty foliage , GameObject brush , FoliageBrush brushType)
                {
                        if (brushType == FoliageBrush.Single && brush != null)
                        {
                                Vector2 position = new Vector3(Mouse.position.x , Mouse.position.y);
                                brush.transform.position = Compute.Round(position , 0.5f);
                                Draw.GLCircleInit(brush.transform.position , 0.5f , Color.red , 3);

                                if (Mouse.MouseDown() && foliage.arraySize < 1024)
                                {
                                        foliage.arraySize++;
                                        SerializedProperty newFoliage = foliage.LastElement();
                                        newFoliage.Get("position").vector3Value = new Vector3(brush.transform.position.x , brush.transform.position.y , 0);
                                        newFoliage.Get("textureIndex").intValue = parent.Get("index").intValue;
                                }
                        }
                }

                public void RandomBrush (SerializedObject parent , SerializedProperty foliage , GameObject brush , FoliageBrush brushType)
                {
                        if (brushType == FoliageBrush.Random)
                        {
                                Vector2 position = Compute.Round(Mouse.position , 0.5f);

                                if (Mouse.MouseDown() && foliage.arraySize < 1024)
                                {
                                        mouseDownPosition = position;
                                        CreateRandomGrass(parent , foliage , mouseDownPosition);
                                }
                                if (Mouse.MouseDrag() && foliage.arraySize < 1024)
                                {
                                        if (Mathf.Abs(mouseDownPosition.x - position.x) >= placementRange)
                                        {
                                                mouseDownPosition = new Vector2(position.x , mouseDownPosition.y); // remember y position, draw straight line
                                                CreateRandomGrass(parent , foliage , mouseDownPosition);
                                        }
                                }
                                brush.transform.position = position;
                                Draw.GLCircleInit(brush.transform.position , 0.25f , Color.red , 3);
                                SceneView.RepaintAll();
                        }
                }

                private void CreateRandomGrass (SerializedObject parent , SerializedProperty foliage , Vector2 position)
                {
                        range.Clear();
                        SerializedProperty textures = parent.Get("textureArray").Get("textures");
                        for (int i = 0; i < textures.arraySize; i++)
                                if (textures.Element(i).Bool("isRandom"))
                                        range.Add(i);

                        if (range.Count == 0)
                                return;

                        placementRange = Random.Range(0.5f , 2f);
                        int amount = Random.Range(0 , (int) parent.Float("randomDensity") + 1);
                        for (int i = 0; i < amount; i++)
                        {
                                foliage.arraySize++;
                                SerializedProperty grass = foliage.LastElement();
                                grass.Get("position").vector3Value = position;
                                grass.Get("textureIndex").intValue = range[Random.Range(0 , range.Count)];
                        }
                }

                public void EraseBrush (SerializedObject parent , SerializedProperty foliage , FoliageBrush brushType)
                {
                        if (brushType != FoliageBrush.Eraser)
                                return;

                        SerializedProperty deleteRadius = parent.Get("deleteRadius");
                        Draw.GLCircleInit(Mouse.position , deleteRadius.floatValue , Tint.Delete , 10);

                        if (Mouse.MouseDrag() || Mouse.MouseDown())
                        {
                                float distance = deleteRadius.floatValue * deleteRadius.floatValue;
                                for (int i = foliage.arraySize - 1; i >= 0; i--)
                                {
                                        Vector2 foliageP = (Vector2) foliage.Element(i).Get("position").vector3Value;
                                        if ((foliageP - (Vector2) Mouse.position).sqrMagnitude < distance)
                                        {
                                                foliage.DeleteArrayElement(i);
                                                SceneView.RepaintAll();
                                        }
                                }
                        }
                }

        }

}
