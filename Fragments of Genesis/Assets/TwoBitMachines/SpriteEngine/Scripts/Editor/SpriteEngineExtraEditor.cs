using System.Collections.Generic;
using System.Linq;
using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite.Editors
{
        [CustomEditor(typeof(SpriteEngineExtra), true)]
        [CanEditMultipleObjects]
        public class SpriteEngineExtraEditor : UnityEditor.Editor
        {
                public SpriteEngineExtra main;
                public SerializedObject parent;
                public List<string> spriteNames = new List<string>();

                public SerializedProperty frames;
                public SerializedProperty property;
                public SerializedProperty sprite;
                public SerializedProperty frameIndex;
                public SerializedProperty spriteIndex;
                public SerializedProperty sprites;
                public bool spriteExists => sprite != null && frames != null;
                public static bool inspectorLocked => ActiveEditorTracker.sharedTracker.isLocked;

                //Play in Scene
                public Sprite originalSprite;
                public Sprite currentSprite;
                public SpritePacketExtra tempSprite;
                public SpriteRenderer render;
                public int currentFrameIndex;
                public float timer;
                public bool propertyOpen;
                public string previousSprite;
                public List<Sprite> tempSprites = new List<Sprite>();

                private void OnEnable ()
                {
                        main = target as SpriteEngineExtra;
                        parent = serializedObject;
                        Layout.Initialize();
                        SpriteProperty.PropertyIcons();
                        SpriteProperty.NameList();
                }

                private void OnDisable ()
                {
                        StopPlayEditorOnly();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        serializedObject.Update();
                        {
                                SetCurrentSprite();
                                PlayButtons();
                                PreviewWindow.Display(this, parent, main);

                                if (CreateSprite())
                                {
                                        SetCurrentSprite(); // reset animation if sprites have been added or deleted
                                }
                                if (spriteExists)
                                {
                                        InspectSprite.Display(this, parent, frames, frameIndex, sprite, true);
                                        SpriteProperty.Execute(property, frameIndex, ref propertyOpen);
                                        DragAndDrop();
                                        Property();
                                        Options();
                                        Buttons();
                                }
                                SpriteTreeEditor.TreeInspector(this, main.tree, parent.Get("tree"), spriteNames, main.tree.signals);
                                TransitionEditor.Transition(this, parent, sprites, "sprites", spriteNames, main.tree.signals);
                                SpriteEngineEditor.ShowCurrentState(main.currentAnimation);
                        }
                        serializedObject.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                        SpriteProperty.SetExtraProperty(main, spriteExists, frameIndex.intValue);
                        SetSpriteEditingProperty();
                        if (main.sort)
                        {
                                main.sort = false;
                                main.sprites = main.sprites.OrderBy(sp => sp.name).ToList();
                        }
                        if (GUI.changed && !EditorApplication.isPlaying)
                                Repaint();
                }

                private void SetCurrentSprite ()
                {
                        sprites = parent.Get("sprites");
                        spriteIndex = parent.Get("spriteIndex");
                        spriteIndex.intValue = Mathf.Clamp(spriteIndex.intValue, 0, sprites.arraySize);
                        sprite = sprites.arraySize > 0 ? sprites.Element(spriteIndex.intValue) : null;
                        frames = sprite != null ? sprite.Get("frame") : null;
                        frameIndex = sprite != null ? sprite.Get("frameIndex") : null;
                        property = sprite != null ? sprite.Get("property") : null;
                        sprites.CreateNameList(spriteNames);

                }

                public void PlayButtons ()
                {
                        Bar.SetupTabBar(3, height: 22);
                        bool canPlay = main.sprites.Count != 0 && spriteIndex.intValue >= 0 && spriteIndex.intValue < main.sprites.Count;
                        if (Bar.TabButton("Play", "RoundTopLeft", Tint.SoftDark, parent.Bool("playInInspector"), "Play In Inspector") && canPlay)
                        {
                                parent.SetTrue("playInInspector");
                                parent.SetFalse("playInScene");
                                parent.SetTrue("resetPlayFrame");
                                StopPlayEditorOnly();
                        }
                        if (Bar.TabButton("PlayScene", "Square", Tint.SoftDark, parent.Bool("playInScene"), "Play in Scene") && canPlay)
                        {
                                parent.SetFalse("playInInspector");
                                parent.SetTrue("playInScene");
                                parent.SetTrue("resetPlayFrame");
                                InitializePlayEditorOnly();
                        }
                        if (Bar.TabButton("Red", "RoundTopRight", Tint.SoftDark, false) && canPlay)
                        {
                                parent.SetFalse("playInInspector");
                                parent.SetFalse("playInScene");
                                parent.SetTrue("resetPlayFrame");
                                StopPlayEditorOnly();
                        }
                }

                public bool CreateSprite ()
                {
                        bool delete = parent.Bool("deleteAsk");
                        FoldOut.Bar(parent, Tint.SoftDark, 4).BL("open", "ArrowRight").
                        BR("deleteAsk", "Yes", execute: delete).SR(8, execute: delete).BR("delete", "Close", execute: delete).
                        BR("deleteAsk", "Minus", execute: !delete).SR(8).BR(execute: !delete).BR("sort", "Sort");
                        {
                                Rect labelRect = new Rect(Bar.barStart) { width = Layout.longInfoWidth - 70 };
                                labelRect.DrawRect(texture: Skin.square, color: Color.white);
                                if (spriteNames.Count > 0)
                                {
                                        GUI.Label(labelRect.Adjust(5, labelRect.width - 4), spriteNames[spriteIndex.intValue], FoldOut.boldStyle);
                                }
                                if (spriteExists)
                                {
                                        sprite.FieldOnlyClear(labelRect, "name"); // change index string name
                                }
                        }
                        if (parent.Bool("open"))
                        {
                                if (FoldOut.DropDownMenu(spriteNames, parent.Get("shiftNames"), spriteIndex))
                                        parent.SetFalse("open");
                                Repaint();
                        }
                        return AppendSprites();
                }

                private bool AppendSprites ()
                {
                        if (parent.ReadBool("delete") && sprites.arraySize > 1)
                        {
                                parent.SetFalse("deleteAsk");
                                sprites.DeleteArrayElement(spriteIndex.intValue);
                                spriteIndex.intValue = Mathf.Clamp(spriteIndex.intValue, 0, sprites.arraySize - 1);
                                return true;
                        }
                        if (parent.ReadBool("add") || sprites.arraySize == 0)
                        {
                                sprites.CreateNewElement().FindPropertyRelative("name").stringValue = "New Sprite";
                                spriteIndex.intValue = sprites.arraySize - 1;
                                return true;
                        }
                        return false;
                }

                public void DragAndDrop ()
                {
                        if (frames.arraySize == 0)
                        {
                                CreateDragAndDropArea("Drop Sprites Here", Tint.White);
                                TransferSpritesFromTempList();
                        }
                        if (parent.Bool("resetSprites") && frames.arraySize > 0)
                        {
                                CreateDragAndDropArea("Replace Sprites", Tint.Delete);
                                TransferSpritesFromTempList();
                        }
                }

                private void CreateDragAndDropArea (string message, Color color)
                {
                        tempSprites.Clear();
                        Rect dropArea = Layout.CreateRectAndDraw(Layout.longInfoWidth, 88, offsetX: -11, texture: Skin.square, color: Tint.SoftDark);
                        {
                                Fields.DropAreaGUI<Sprite>(dropArea, tempSprites);
                                Labels.Centered(dropArea, message, color, fontSize: 10, shiftY: 15);

                                Rect dropRect = Skin.TextureCentered(dropArea, Icon.Get("DropCorner"), new Vector2(22, 22), Tint.White, shiftY: -10);
                                if (frames.arraySize == 0 && dropRect.ContainsMouseDown())
                                {
                                        frames.arraySize++;
                                        parent.SetFalse("resetSprites");
                                        ActiveEditorTracker.sharedTracker.isLocked = false;
                                        UnityEditor.Selection.activeGameObject = main.gameObject;
                                }
                                else if (dropArea.ContainsMouseDown(false))
                                {
                                        ToggleInspectorLock();
                                }
                                if (ActiveEditorTracker.sharedTracker.isLocked)
                                        Labels.Centered(dropArea, "Locked", color, fontSize: 10, shiftY: 28);
                        }
                }

                private void TransferSpritesFromTempList ()
                {
                        if (tempSprites.Count == 0 || frames == null)
                                return;

                        frames.arraySize = 0;
                        parent.SetFalse("resetSprites");
                        ActiveEditorTracker.sharedTracker.isLocked = false;
                        UnityEditor.Selection.activeGameObject = main.gameObject;

                        for (int i = 0; i < tempSprites.Count; i++)
                        {
                                frames.arraySize++;
                                frames.LastElement().Get("sprite").objectReferenceValue = tempSprites[i];
                                frames.LastElement().Get("rate").floatValue = 1f / 10f;
                        }
                }

                public void ToggleInspectorLock ()
                {
                        ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
                }

                public void Options ()
                {
                        if (sprite == null || frames == null)
                                return;

                        if (parent.Bool("spriteMenu") && frames.arraySize > 0)
                        {
                                SerializedProperty array = sprite.Get("randomSprites");
                                if (array.arraySize == 0)
                                {
                                        FoldOut.Box(Texture2D.whiteTexture, 1, Tint.SoftDark, offsetY: -1);
                                        {
                                                Fields.ConstructField();
                                                Fields.ConstructString("Add Random Sprite", S.LW);
                                                Fields.ConstructSpace(S.CW - S.B);
                                                if (Fields.ConstructButton("Add"))
                                                { array.arraySize++; }
                                        }
                                }
                                else
                                {
                                        FoldOut.Box(Texture2D.whiteTexture, array.arraySize, Tint.SoftDark, offsetY: -1);
                                        for (int i = 0; i < array.arraySize; i++)
                                        {
                                                SerializedProperty element = array.Element(i);
                                                Fields.ConstructField();
                                                Fields.ConstructString("Random Sprite", S.LW);
                                                element.ConstructList(spriteNames.ToArray(), "name", S.H - S.B);
                                                element.ConstructField("time", S.H - S.B);
                                                if (Fields.ConstructButton("Add"))
                                                { array.InsertArrayElement(i); break; }
                                                if (Fields.ConstructButton("Minus"))
                                                { array.DeleteArrayElement(i); break; }
                                        }
                                }
                                FoldOut.Box(Texture2D.whiteTexture, 4, Tint.SoftDark, offsetY: -1);
                                {
                                        sprite.Field("Loop Start Index", "loopStartIndex");
                                        sprite.FieldToggleAndEnable("Loop Once", "loopOnce");
                                }
                                if (sprite.Bool("loopOnce"))
                                {
                                        Fields.EventField(sprite.Get("onLoopOnce"), adjustX: 10, color: Tint.SoftDark);
                                }
                                else
                                {
                                        Layout.VerticalSpacing(4);
                                        Layout.CreateRectAndDraw(Layout.longInfoWidth, 42, offsetX: -11, texture: Texture2D.whiteTexture, color: Tint.SoftDark); // to keep all options height even
                                }
                        }
                }

                public void Buttons ()
                {
                        SerializedProperty menu = parent.Get("spriteMenu");
                        SerializedProperty resetSprites = parent.Get("resetSprites");
                        SerializedProperty propertyFoldOut = parent.Get("propertyFoldOut");

                        Bar.Setup(Texture2D.whiteTexture, Tint.SoftDark, space: false, height: 20);
                        if (Bar.ButtonRight(resetSprites, "DropCorner", Tint.Blue, Tint.White))
                                menu.boolValue = propertyFoldOut.boolValue = false;
                        if (Bar.ButtonRight(menu, "Event", Tint.Blue, Tint.White))
                                resetSprites.boolValue = propertyFoldOut.boolValue = false;
                        if (Bar.ButtonRight(propertyFoldOut, "Wrench", Tint.Blue, Tint.White))
                                menu.boolValue = resetSprites.boolValue = false;
                }

                public void Property ()
                {
                        if (frames == null || property == null)
                                return;

                        SpriteProperty.MatchArraySize(property, frames.arraySize);
                        SpriteProperty.SecureSameSize(property, frames.arraySize);

                        if (frames.arraySize != 0 && parent.Bool("propertyFoldOut")) /// CREATE PROPERTIES
                        {
                                int create = -1;
                                Block.CatalogList(5, 19, true).Scroll(this, 1, SpriteProperty.names.Count, parent.Get("propertyIndex"), (height, index) =>
                                {
                                        Block.Header().BoxRect(Tint.WarmGrey * index.RowTint(1f, 0.95f), selection: true, height: height)
                                             .MouseDown(height, () =>
                                             {
                                                     parent.SetFalse("propertyFoldOut");
                                                     create = index;
                                             })
                                             .Image(SpriteProperty.icons[index], 18)
                                             .Label(SpriteProperty.names[index], 11, false, Color.black)
                                             .Build();
                                });
                                Repaint();
                                if (create != -1)
                                {
                                        SpriteProperty.CreateProperty(create, property);
                                        SpriteProperty.MatchArraySize(property, frames.arraySize);
                                }
                        }
                }

                private void SetSpriteEditingProperty ()
                {
                        if (main.playingInScene)
                                return;

                        if (propertyOpen && currentSprite != null)
                        {
                                if (render == null)
                                        render = main.gameObject.GetComponent<SpriteRenderer>();
                                if (render == null)
                                        return;
                                if (!main.settingRenderer)
                                        main.rendererSprite = render.sprite;
                                main.settingRenderer = true;
                                render.sprite = currentSprite;
                                if (previousSprite != currentSprite.name)
                                        SceneView.RepaintAll();
                                previousSprite = currentSprite.name;
                        }
                        else if (main.settingRenderer)
                        {
                                if (render == null)
                                        render = main.gameObject.GetComponent<SpriteRenderer>();
                                if (main.rendererSprite != null && render != null)
                                        render.sprite = main.rendererSprite;
                                main.settingRenderer = false;
                        }
                }

                #region Play In Scene
                public void InitializePlayEditorOnly ()
                {
                        StopPlayEditorOnly();
                        render = main.gameObject.GetComponent<SpriteRenderer>();
                        TwoBitMachines.Clock.Initialize();
                        timer = currentFrameIndex = 0;
                        tempSprite = null;
                        for (int i = 0; i < main.sprites.Count; i++)
                        {
                                if (main.spriteIndex != i)
                                        continue;
                                tempSprite = main.sprites[i];
                                originalSprite = render != null ? render.sprite : originalSprite;
                                tempSprite.SetProperties(0, firstFrame: true);
                                EditorApplication.update += RunAnimation;
                                break;
                        }
                }

                private void RunAnimation ()
                {
                        if (EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer || EditorApplication.isCompiling || render == null || tempSprite == null || tempSprite.frame.Count == 0)
                        {
                                StopPlayEditorOnly();
                                return;
                        }
                        if (TwoBitMachines.Clock.TimerEditor(ref timer, tempSprite.frame[currentFrameIndex].rate))
                        {
                                currentFrameIndex = currentFrameIndex + 1 >= tempSprite.frame.Count ? 0 : currentFrameIndex + 1;
                                render.sprite = tempSprite.frame[currentFrameIndex].sprite;
                                tempSprite.SetProperties(currentFrameIndex);
                        }
                        for (int i = 0; i < tempSprite.property.Count; i++)
                        {
                                if (tempSprite.property[i].interpolate)
                                {
                                        float playRate = tempSprite.frame[currentFrameIndex].rate;
                                        tempSprite.property[i].Interpolate(currentFrameIndex, playRate, timer);
                                }
                        }
                }

                public void StopPlayEditorOnly ()
                {
                        if (tempSprite != null)
                                tempSprite.ResetProperties();
                        if (render != null && originalSprite != null)
                                render.sprite = originalSprite;
                        EditorApplication.update -= RunAnimation;
                        originalSprite = null; //
                        tempSprite = null;
                }
                #endregion
        }
}
