using System.Collections.Generic;
using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite.Editors
{
        [CustomEditor(typeof(SpriteSwap), true)]
        public class SpriteSwapEditor : UnityEditor.Editor
        {
                private SpriteSwap main;
                private SerializedObject parent;
                public static string inputName = " Skin Name";
                public static string spriteName = " Sprite Name";
                public List<Sprite> tempSprites = new List<Sprite>();
                public List<Texture2D> tempTexture2D = new List<Texture2D>();

                private void OnEnable ()
                {
                        main = target as SpriteSwap;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();

                        SerializedProperty array = parent.Get("characterSkin");

                        if (Fields.InputAndButtonBox("Create New Skin", "Add", Tint.Blue, ref inputName))
                        {
                                array.arraySize++;
                                array.LastElement().Get("name").stringValue = inputName;
                                inputName = " Skin Name";
                        }

                        for (int i = 0; i < array.arraySize; i++)
                        {
                                SerializedProperty characterSkin = array.Element(i);

                                bool open = characterSkin.Bool("foldOut");
                                bool deleteAsk = characterSkin.Bool("deleteAsk");

                                if (
                                        FoldOut.Bar(characterSkin, Tint.Orange, 0)
                                        .Grip(parent, array, i, color: Tint.WarmWhite)
                                        .LabelAndEdit("name", "edit", Color.white)
                                        .RightButton("deleteAsk", "Delete", on: Tint.WarmWhite, off: Tint.WarmWhite, toolTip: "Delete Skin", execute: open && !deleteAsk)
                                        .RightButton("deleteAsk", "Close", toolTip: "Return", execute: open && deleteAsk)
                                        .RightButton("delete", "Yes", toolTip: "Delete", execute: open && deleteAsk)
                                        .RightButton("add", "Add", toolTip: "Add Sprite", execute: open)
                                        .FoldOut())
                                {

                                        if (characterSkin.ReadBool("delete"))
                                        {
                                                array.DeleteArrayElement(i);
                                                break;
                                        }

                                        SerializedProperty skin = characterSkin.Get("skin");
                                        if (characterSkin.ReadBool("add"))
                                        {
                                                skin.arraySize++;
                                                skin.LastElement().Get("name").stringValue = spriteName;
                                                skin.LastElement().Get("sprite").arraySize = 0;
                                                spriteName = " Sprite Name"; //
                                        }
                                        for (int j = 0; j < skin.arraySize; j++)
                                        {
                                                Skins(characterSkin, skin, skin.Element(j), j);
                                        }
                                }
                        }

                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                }

                public void Skins (SerializedProperty characterSkin, SerializedProperty array, SerializedProperty skin, int i)
                {
                        bool open = skin.Bool("foldOut");
                        bool deleteAsk = skin.Bool("deleteAsk");

                        if (
                                FoldOut.Bar(skin, Tint.Box, 0)
                                .Grip(characterSkin, array, i, color: Tint.WarmWhite)
                                .LabelAndEdit("name", "edit", Color.white)
                                .RightButton("deleteAsk", "Delete", on: Tint.WarmWhite, off: Tint.WarmWhite, toolTip: "Delete Sprite", execute: open && !deleteAsk)
                                .RightButton("deleteAsk", "Close", toolTip: "Return", execute: open && deleteAsk)
                                .RightButton("delete", "Yes", toolTip: "Delete", execute: open && deleteAsk)
                                .RightButton("replace", "DropCorner", toolTip: "Replace Sprites", execute: open)
                                .FoldOut())
                        {

                                if (skin.ReadBool("delete"))
                                {
                                        array.DeleteArrayElement(i);
                                        return;
                                }

                                SerializedProperty sprite = skin.Get("sprite");

                                if (sprite.arraySize == 0)
                                {
                                        CreateDragAndDropArea(sprite, "Add Sprites", Tint.WarmWhite);
                                        TransferSprites(sprite, skin);
                                }
                                if (sprite.arraySize == 0)
                                {
                                        return;
                                }

                                FoldOut.Box(sprite.arraySize, FoldOut.boxColor, offsetY: -2);
                                {
                                        for (int j = 0; j < sprite.arraySize; j++)
                                        {
                                                SerializedProperty element = sprite.Element(j);
                                                Fields.ConstructField();
                                                Fields.Grip(skin, sprite, j, color: Tint.WarmGrey);
                                                Fields.ShowSprite((Sprite) element.objectReferenceValue, 16, offsetX: 6, offsetY: 0);
                                                element.ConstructField(S.FW - 54);

                                                if (Fields.ConstructButton("xsAdd"))
                                                {
                                                        sprite.InsertArrayElement(j);
                                                        break;
                                                }
                                                if (Fields.ConstructButton("xsMinus"))
                                                {
                                                        sprite.DeleteArrayElement(j);
                                                        break;
                                                }
                                        }
                                }
                                Layout.VerticalSpacing(3);

                                if (skin.Bool("replace"))
                                {
                                        CreateDragAndDropArea(sprite, "Replace Sprites", Tint.WarmWhite);
                                        TransferSprites(sprite, skin);
                                }
                        }
                }

                private void CreateDragAndDropArea (SerializedProperty array, string message, Color color)
                {
                        tempSprites.Clear();
                        tempTexture2D.Clear();
                        Rect dropArea = Layout.CreateRectAndDraw(Layout.longInfoWidth, 88, offsetX: -11, offsetY: -2, texture: FoldOut.background, color: Tint.Box);
                        {
                                Fields.DropAreaGUI<Sprite>(dropArea, tempSprites);
                                Fields.DropAreaGUI<UnityEngine.Texture2D>(dropArea, tempTexture2D);
                                Labels.Centered(dropArea, message, color, fontSize: 12, shiftY: 15);

                                Rect dropRect = Skin.TextureCentered(dropArea, Icon.Get("DropCorner"), new Vector2(22, 22), Tint.White, shiftY: -10);
                                if (array.arraySize == 0 && dropRect.ContainsMouseDown())
                                {
                                        array.arraySize++;
                                }
                        }
                }

                private void TransferSprites (SerializedProperty array, SerializedProperty skin)
                {
                        if ((tempSprites.Count == 0 && tempTexture2D.Count == 0) || array == null)
                        {
                                return;
                        }

                        array.arraySize = 0;
                        skin.SetFalse("replace");

                        for (int i = 0; i < tempSprites.Count; i++)
                        {
                                array.arraySize++;
                                array.LastElement().objectReferenceValue = tempSprites[i];
                        }
                        for (int i = 0; i < tempTexture2D.Count; i++)
                        {
                                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(tempTexture2D[i]));
                                if (sprite == null)
                                        continue;
                                array.arraySize++;
                                array.LastElement().objectReferenceValue = sprite;
                        }
                }

        }
}
