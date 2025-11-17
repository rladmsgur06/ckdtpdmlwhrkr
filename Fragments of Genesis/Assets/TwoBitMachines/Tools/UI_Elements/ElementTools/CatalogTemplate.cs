#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TwoBitMachines.UIElement.Editor
{
        [System.Serializable]
        public class Catalog : BindableElement
        {
                [SerializeField] public VisualElement catalogContainer;
                [SerializeField] public ListScrollBar scrollBar;
                [SerializeField] public Image selected;
                [SerializeField] public List<CatalogItem> catalog = new List<CatalogItem>();
                [SerializeField] public List<SerializedProperty> list = new List<SerializedProperty>();
                [SerializeField] public List<SerializedProperty> arrays = new List<SerializedProperty>();

                [SerializeField] public int selectedIndex;
                [SerializeField] public int maxItemsInWindow;
                [SerializeField] public int currentIndex;
                [SerializeField] public int scrollSpeed;
                [SerializeField] public int itemHeight;
                [SerializeField] public bool isSelected;

                [SerializeField] public delegate CatalogItem MakeCatalogItem (VisualElement parent, int index);

                #region initialize
                public Catalog ()
                {

                }

                public Catalog (int maxItemsInWindow, int itemHeight, int scrollSpeed, VisualElement container, bool isSelected = false, int selectedIndex = 0)
                {
                        Initialize(maxItemsInWindow, itemHeight, scrollSpeed, container, isSelected, selectedIndex);
                }

                public Catalog Initialize (int maxItemsInWindow, int itemHeight, int scrollSpeed, VisualElement container, bool isSelected = false, int selectedIndex = 0)
                {
                        this.maxItemsInWindow = maxItemsInWindow;
                        this.itemHeight = itemHeight;
                        this.scrollSpeed = scrollSpeed;
                        this.isSelected = isSelected;
                        this.selectedIndex = selectedIndex;

                        catalogContainer = new VisualElement();
                        catalogContainer.AddToClassList("clear");
                        catalogContainer.Add(this);
                        container.Add(catalogContainer);

                        IsSelected(selectedIndex);
                        RegisterCallback<WheelEvent>(evt =>
                        {
                                evt.StopPropagation();
                                ScrollCatalog(evt.delta.y > 0 ? 1 : -1, setMousePosition: true);
                        });
                        return this;
                }

                public Catalog Register (SerializedProperty array, bool canDelete = false)
                {
                        arrays.Add(array);
                        for (int i = 0; i < array.arraySize; i++)
                        {
                                ItemDeleteStatus(array.Element(i), canDelete);
                                list.Add(array.Element(i));
                        }
                        return this;
                }

                public Catalog MakeItems (MakeCatalogItem makeItem)
                {
                        for (int i = 0; i < maxItemsInWindow; i++)
                        {
                                if (catalog.Count < maxItemsInWindow && catalog.Count < list.Count)
                                {
                                        catalog.Add(makeItem(this, i));
                                }
                        }
                        return this;
                }

                public Catalog ScrollBar (Color barColor, Color handleColor, int bottomMargin = 0)
                {
                        this.style.flexGrow = 1;
                        scrollBar = new ListScrollBar(catalogContainer, maxItemsInWindow, catalog.Count, itemHeight, list.Count, scrollSpeed, barColor, handleColor, ScrollCatalog);
                        scrollBar.bar.style.marginBottom = bottomMargin;
                        return this;
                }

                public Catalog Refresh ()
                {
                        ScrollCatalog(currentIndex, true);
                        return this;
                }
                #endregion

                public void IsSelected (int index)
                {
                        if (!isSelected)
                        {
                                return;
                        }
                        if (selected == null)
                        {
                                selected = new Image();
                                selected.SetImage("CheckMark", color: Tint.SoftDark);
                        }
                        selectedIndex = index;
                        selected.Visible(false);

                        int mainIndex = currentIndex;
                        for (int i = 0; i < catalog.Count; i++)
                        {
                                if (mainIndex < list.Count && mainIndex++ == index && catalog[i] != null)
                                {
                                        // catalog[i].Add(selected);
                                        // int childIndex = selected[i].IndexOf(selected);
                                        catalog[i].Insert(0, selected);
                                        selected.style.position = Position.Absolute;
                                        selected.style.marginLeft = 5;
                                        selected.Visible(true);
                                        break;
                                }
                        }
                }
                #region Run
                public void ScrollCatalog (int scroll, bool setScrollIndex = false, bool setMousePosition = false)
                {
                        currentIndex = setScrollIndex ? scroll : currentIndex + scroll * scrollSpeed;
                        int mainIndex = currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, list.Count - maxItemsInWindow));

                        for (int i = 0; i < catalog.Count; i++)
                        {
                                if (mainIndex < list.Count && catalog[i] != null && list[mainIndex] != null)
                                {
                                        catalog[i].Refresh(list[mainIndex++]);
                                        continue;
                                }
                                break;
                        }
                        if (setMousePosition && scrollBar != null)
                        {
                                scrollBar.SetHandleTop(currentIndex);
                        }
                        IsSelected(selectedIndex);
                }
                #endregion

                #region Update
                public void MakeItem (int index, SerializedProperty newItem, bool canDelete, MakeCatalogItem makeItem)
                {
                        ItemDeleteStatus(newItem, canDelete);
                        if (!list.Contains(newItem))
                        {
                                list.Add(newItem);
                        }
                        if (catalog.Count < maxItemsInWindow)
                        {
                                catalog.Add(makeItem(this, index));
                        }
                        UpdateScrollBar();
                        ScrollCatalog(currentIndex + 1, true, true);
                }

                public void RemoveItem (CatalogItem item)
                {
                        DeleteItem(item);
                        if (catalog.Contains(item) && list.Count <= catalog.Count)
                        {
                                catalog.Remove(item);
                                item.DeleteVisualUnit();
                        }
                        RefreshList();
                }

                public void RefreshList ()
                {
                        list.Clear();
                        for (int i = 0; i < arrays.Count; i++) // refresh item list
                        {
                                for (int j = 0; j < arrays[i].arraySize; j++)
                                {
                                        list.Add(arrays[i].Element(j));
                                }
                        }
                        UpdateScrollBar();
                        ScrollCatalog(currentIndex, true, true);
                }

                public void DeleteItem (CatalogItem item)
                {
                        string propertyPath = item.property.PropertyPath();
                        for (int i = 0; i < arrays.Count; i++)
                        {
                                for (int j = 0; j < arrays[i].arraySize; j++)
                                {
                                        if (propertyPath == arrays[i].Element(j).propertyPath)
                                        {
                                                arrays[i].Delete(j);
                                                break;
                                        }
                                }
                        }
                        item.so.ApplyModifiedProperties();
                }

                public void ItemDeleteStatus (SerializedProperty item, bool canDelete)
                {
                        if (item.Get("canDelete") != null)
                        {
                                item.Get("canDelete").boolValue = canDelete;
                        }
                }

                public void UpdateScrollBar ()
                {
                        if (scrollBar != null)
                        {
                                scrollBar.UpdateLayout(catalog.Count, list.Count);
                        }
                }


                public int IndexOf (SerializedProperty property)
                {
                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i].propertyPath == property.PropertyPath())
                                        return i;
                        }
                        return 0;
                }
                #endregion
        }

        [System.Serializable]
        public class CatalogItem : Bar
        {
                [SerializeField] public Label labelField;
                [SerializeField] public Button deleteButton;
                [SerializeField] public Color labelColor;
                [SerializeField] public string active;
                [SerializeField] public string labelRef;
                [SerializeField] public int bottomSpaceOpen;
                [SerializeField] public int bottomSpaceClosed;
                [SerializeField] public float rowTint = 1f;
                [SerializeField] public float rowTintValue = 0.95f;
                [SerializeField] public bool rowTintEnable;

                public string itemName => property == null ? "" : property.String(labelRef);

                public CatalogItem (VisualElement container, SerializedObject so, System.Object property, Color color, SerializedProperty array = null, int height = 23, int space = 2, string backgroundImage = "Header")
                : base(container, so, property, color, array, height, space, backgroundImage) { }

                public UIBlock Style (string name, string active, Color labelColor, int fontSize = 12, int bottomSpaceOpen = 0, int bottomSpaceClosed = 0, int yOffset = 2, int marginLeft = 5, bool bold = false)
                {
                        VisualElement target = this.target;
                        VisualElement button = new VisualElement();
                        button.style.alignSelf = Align.Center;
                        button.AddToClassList("clear");
                        target.Add(button);

                        button.style.flexGrow = 1;
                        button.style.marginLeft = marginLeft;
                        button.style.height = target.style.height;
                        button.style.paddingTop = yOffset;

                        labelField = new Label();
                        button.Add(labelField);
                        labelField.style.fontSize = fontSize;
                        labelField.style.unityTextAlign = TextAnchor.MiddleLeft;

                        this.bottomSpaceOpen = bottomSpaceOpen;
                        this.bottomSpaceClosed = bottomSpaceClosed;
                        this.labelColor = labelColor;
                        this.active = active;
                        this.labelRef = name;


                        if (bold)
                        {
                                button.AddToClassList("label-bold");
                        }

                        Refresh(property as SerializedProperty);

                        button.RegisterCallback<MouseOutEvent>(evt =>
                        {
                                target.style.unityBackgroundImageTintColor = color * inactive * rowTint;
                        });
                        button.RegisterCallback<MouseOverEvent>(evt =>
                        {
                                target.style.unityBackgroundImageTintColor = color * Tint.hoverTint * inactive;
                        });
                        button.RegisterCallback<MouseDownEvent>(evt =>
                        {
                                if (property == null)
                                        return;

                                inactive = property.Get(active) == null || property.Toggle(active) ? 1f : 0.15f;
                                target.style.unityBackgroundImageTintColor = color * inactive * rowTint;
                                labelField.style.color = inactive >= 1 ? labelColor : labelColor * 0.5f;
                                OnOpen(property.Get(active) == null ? true : property.Bool(active), bottomSpaceOpen, bottomSpaceClosed);
                                so.ApplyProperties();
                        });
                        return this;
                }

                public CatalogItem RowTint (float value = 0.95f)
                {
                        rowTintEnable = true;
                        rowTintValue = value;
                        return this;
                }

                public Button DeleteButton (string icon, bool hide = false, Color? color = null, string tooltip = "")
                {
                        deleteButton = this.Callback(icon, hide, color, tooltip = "");
                        RefreshDeleteButton();
                        return deleteButton;
                }

                public void Refresh (SerializedProperty property)
                {
                        if (property != null)
                        {
                                this.property = property;
                                this.bindingPath = property.PropertyPath();
                                VisualElement target = this.target;
                                labelField.text = property.String(labelRef);
                                inactive = property.Get(active) == null || property.Bool(active) ? 1f : 0.15f;
                                labelField.style.color = inactive >= 1 ? labelColor : labelColor * 0.5f;
                                rowTint = rowTintEnable && this.parent.IndexOf(this) % 2 == 0 ? rowTintValue : 1f;
                                target.style.unityBackgroundImageTintColor = color * inactive * rowTint;
                                RefreshDeleteButton();
                                OnOpen(property.Get(active) == null ? true : property.Bool(active), bottomSpaceOpen, bottomSpaceClosed);
                        }
                }

                private void RefreshDeleteButton ()
                {
                        if (deleteButton != null)
                        {
                                deleteButton.Visible(property.Get("canDelete") != null ? property.Bool("canDelete") : false);
                        }
                }

        }

        [System.Serializable]
        public class CatalogList : BindableElement
        {
                [SerializeField] public VisualElement container;
                [SerializeField] public ListScrollBar scrollBar;
                [SerializeField] public Image selected;
                [SerializeField] public List<string> list = new List<string>();
                [SerializeField] public List<CatalogListItem> catalog = new List<CatalogListItem>();
                [SerializeField] public List<VisualElement> iconList = new List<VisualElement>();

                [SerializeField] public int selectedIndex;
                [SerializeField] public int maxItemsInWindow;
                [SerializeField] public int currentIndex;
                [SerializeField] public int scrollSpeed;
                [SerializeField] public int itemHeight;
                [SerializeField] public bool isSelected;

                [SerializeField] public delegate CatalogListItem MakeCatalogItem (CatalogList catalog, VisualElement parent, string name, int index);

                public CatalogList () { }

                public CatalogList Initialize (VisualElement container, int maxItemsInWindow, int itemHeight, int scrollSpeed, bool isSelected = false, int selectedIndex = 0, bool visible = true)
                {
                        this.maxItemsInWindow = maxItemsInWindow;
                        this.itemHeight = itemHeight;
                        this.scrollSpeed = scrollSpeed;
                        this.isSelected = isSelected;
                        this.selectedIndex = selectedIndex;

                        this.container = new VisualElement();
                        this.container.AddToClassList("clear");
                        this.container.Add(this);
                        this.container.Visible(visible);
                        container.Add(this.container);

                        IsSelected(selectedIndex);
                        RegisterCallback<WheelEvent>(evt =>
                        {
                                evt.StopPropagation();
                                ScrollCatalog(evt.delta.y > 0 ? 1 : -1, setMousePosition: true);
                        });
                        return this;
                }

                public CatalogList RegisterList (List<string> newList)
                {
                        for (int i = 0; i < newList.Count; i++)
                        {
                                list.Add(newList[i]);
                        }
                        return this;
                }

                public CatalogList RegisterIcons (List<Texture2D> icons)
                {
                        for (int i = 0; i < icons.Count; i++)
                        {
                                VisualElement image = new VisualElement().SetImage(icons[i], size: 13);
                                image.style.position = Position.Absolute;
                                image.style.marginLeft = 5;
                                iconList.Add(image);
                        }
                        return this;
                }

                public CatalogList MakeItem (MakeCatalogItem makeItem)
                {
                        for (int i = 0; i < maxItemsInWindow; i++)
                        {
                                if (catalog.Count < maxItemsInWindow && catalog.Count < list.Count)
                                {
                                        catalog.Add(makeItem(this, this, list[i], i));
                                }
                        }
                        return this;
                }

                public CatalogList ScrollBar (Color barColor, Color handleColor, int bottomMargin = 0)
                {
                        this.style.flexGrow = 1;
                        scrollBar = new ListScrollBar(container, maxItemsInWindow, catalog.Count, itemHeight, list.Count, scrollSpeed, barColor, handleColor, ScrollCatalog);
                        scrollBar.bar.style.marginBottom = bottomMargin;
                        return this;
                }

                public CatalogList Refresh ()
                {
                        ScrollCatalog(currentIndex, true);
                        return this;
                }

                public void IsSelected (int index)
                {
                        if (!isSelected)
                        {
                                return;
                        }
                        if (selected == null)
                        {
                                selected = new Image();
                                selected.SetImage("CheckMark", color: Tint.SoftDark);
                        }
                        selectedIndex = index;
                        selected.Visible(false);

                        int mainIndex = currentIndex;
                        for (int i = 0; i < catalog.Count; i++)
                        {
                                if (mainIndex < list.Count && mainIndex++ == index && catalog[i] != null)
                                {
                                        catalog[i].Insert(0, selected);
                                        selected.style.position = Position.Absolute;
                                        selected.style.marginLeft = 5;
                                        selected.Visible(true);
                                        break;
                                }
                        }
                }

                public void ScrollCatalog (int scroll, bool setScrollIndex = false, bool setMousePosition = false)
                {
                        currentIndex = setScrollIndex ? scroll : currentIndex + scroll * scrollSpeed;
                        int mainIndex = currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, list.Count - maxItemsInWindow));

                        for (int i = 0; i < iconList.Count; i++)
                        {
                                iconList[i].Visible(false);
                        }
                        for (int i = 0; i < catalog.Count; i++)
                        {
                                if (mainIndex < list.Count && catalog[i] != null && list[mainIndex] != null)
                                {
                                        catalog[i].Refresh(list[mainIndex]);
                                        if (mainIndex < iconList.Count)
                                        {
                                                iconList[mainIndex].Visible(true);
                                                catalog[i].Insert(0, iconList[mainIndex]);
                                        }
                                        mainIndex++;
                                        continue;
                                }
                                break;
                        }
                        if (setMousePosition && scrollBar != null)
                        {
                                scrollBar.SetHandleTop(currentIndex);
                        }
                        IsSelected(selectedIndex);
                }

                public int IndexOf (string name)
                {
                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i] == name)
                                        return i;
                        }
                        return 0;
                }
        }

        [System.Serializable]
        public class CatalogListItem : BindableElement
        {
                [SerializeField] public CatalogList catalog;
                [SerializeField] public Label labelField;
                [SerializeField] public Color labelColor;
                [SerializeField] public Color color;
                [SerializeField] public int bottomSpaceOpen;
                [SerializeField] public int bottomSpaceClosed;
                [SerializeField] public float rowTint = 1f;
                [SerializeField] public float rowTintValue = 0.95f;
                [SerializeField] public bool rowTintEnable;

                public string label => labelField.text;

                public CatalogListItem (CatalogList catalog, VisualElement container, string name, Color color, Color labelColor, string background, int height, int fontSize = 12, int bottomSpace = 0, int yOffset = 2, int paddingLeft = 5, bool bold = false)
                {
                        AddToClassList("box");
                        this.catalog = catalog;
                        style.backgroundImage = Icon.Get(background);
                        style.unityBackgroundImageTintColor = color;
                        style.flexDirection = FlexDirection.Row;
                        style.alignItems = Align.Center;
                        style.paddingLeft = paddingLeft;
                        style.height = height;
                        style.marginLeft = 0;
                        style.marginTop = 0;
                        container.Add(this);

                        VisualElement button = new VisualElement();
                        button.style.alignSelf = Align.Center;
                        button.AddToClassList("clear");
                        this.Add(button);

                        button.style.flexGrow = 1;
                        button.style.height = height;
                        button.style.paddingTop = yOffset;
                        button.style.marginBottom = bottomSpace;

                        labelField = new Label();
                        button.Add(labelField);
                        labelField.style.fontSize = fontSize;
                        labelField.style.unityTextAlign = TextAnchor.MiddleLeft;

                        this.labelColor = labelColor;
                        this.color = color;
                        Refresh(name);

                        if (bold)
                        {
                                button.AddToClassList("label-bold");
                        }

                        button.RegisterCallback<MouseOutEvent>(evt =>
                        {
                                style.unityBackgroundImageTintColor = color * rowTint;
                        });
                        button.RegisterCallback<MouseOverEvent>(evt =>
                        {
                                style.unityBackgroundImageTintColor = color * Tint.hoverTint;
                        });
                }

                public CatalogListItem RowTint (float value = 0.95f)
                {
                        rowTintEnable = true;
                        rowTintValue = value;
                        return this;
                }

                public CatalogListItem CallBack (StringCallBack CallBack)
                {
                        this.RegisterCallback<MouseDownEvent>(evt => CallBack(label));
                        return this;
                }

                public CatalogListItem RegisterCallBack (IntCallBack CallBack)
                {
                        this.RegisterCallback<MouseDownEvent>(evt => CallBack(catalog.IndexOf(this)));
                        return this;
                }

                public void Refresh (string name)
                {
                        labelField.text = name;
                        rowTint = rowTintEnable && this.parent.IndexOf(this) % 2 == 0 ? rowTintValue : 1f;
                        style.unityBackgroundImageTintColor = color * rowTint;
                }

        }

        public class ListScrollBar
        {
                public VisualElement handle;
                public VisualElement bar;
                private ScrollCatalog scrollAction;

                private float totalItems;
                private float currentItemsInWindow;
                private float maxItemsInWindow;
                private float itemHeight;
                private float scrollSpeed;
                private float colorActive;
                private bool isDragging;
                private Color handleColor;

                private int activeSize => (int) Mathf.Max(totalItems - currentItemsInWindow, 1f);
                private float maxHandleTop => bar.resolvedStyle.height - handle.resolvedStyle.height;

                public ListScrollBar (VisualElement container, int maxItemsInWindow, int currentItemsInWindow, int itemHeight, int totalItems, int scrollSpeed, Color barColor, Color handleColor, ScrollCatalog scrollAction)
                {
                        this.totalItems = totalItems;
                        this.maxItemsInWindow = maxItemsInWindow;
                        this.currentItemsInWindow = currentItemsInWindow;
                        this.itemHeight = itemHeight;
                        this.scrollAction = scrollAction;
                        this.scrollSpeed = scrollSpeed;
                        this.handleColor = handleColor;
                        this.colorActive = 1f;
                        container.style.flexDirection = FlexDirection.Row;

                        bar = new VisualElement();
                        handle = new VisualElement();

                        handle.style.width = 10;
                        bar.style.flexGrow = 0;
                        bar.style.width = 10;
                        bar.style.left = -10;
                        bar.style.backgroundColor = barColor;

                        container.Add(bar);
                        bar.Add(handle);

                        SetHandleHeight();
                        SetHandleColor(handleColor);

                        handle.RegisterCallback<MouseDownEvent>(e =>
                        {
                                isDragging = true;
                                e.target.CaptureMouse();
                                e.StopPropagation();
                                SetHandleColor(handleColor * Tint.activeTint);
                        });
                        handle.RegisterCallback<MouseUpEvent>(evt =>
                        {
                                isDragging = false;
                                evt.target.ReleaseMouse();
                                SetHandleColor(handleColor);
                        });
                        handle.RegisterCallback<MouseOutEvent>(e =>
                        {
                                if (!isDragging)
                                {
                                        SetHandleColor(handleColor);
                                }
                        });
                        handle.RegisterCallback<MouseOverEvent>(e =>
                        {
                                if (!isDragging)
                                {
                                        SetHandleColor(handleColor * Tint.hoverTint);
                                }
                        });
                        handle.RegisterCallback<MouseMoveEvent>(evt =>
                        {
                                if (isDragging)
                                {
                                        Scroll(evt.mouseDelta.y);
                                }
                        });
                        bar.RegisterCallback<WheelEvent>(evt =>
                        {
                                Scroll(evt.mouseDelta.y * scrollSpeed);
                        });
                }

                private void Scroll (float mouseDelta)
                {
                        float newTop = UpdateHandleTop(handle.resolvedStyle.top + mouseDelta);
                        float percent = newTop / maxHandleTop;
                        int scrollPosition = (int) Compute.Round(activeSize * percent, 1);
                        scrollAction(Mathf.Clamp(scrollPosition, 0, activeSize), setScrollIndex: true);
                }

                public void UpdateLayout (int currentItemsInWindow, int totalItems)
                {
                        this.currentItemsInWindow = currentItemsInWindow;
                        this.totalItems = totalItems;
                        SetHandleHeight();
                        UpdateHandleTop(handle.resolvedStyle.top);
                        SetHandleColor(handleColor);
                }

                public float UpdateHandleTop (float newTop)
                {
                        newTop = Mathf.Clamp(newTop, 0f, maxHandleTop);
                        handle.style.top = newTop;
                        return newTop;
                }

                public void SetHandleHeight ()
                {
                        float percent = Mathf.Min(currentItemsInWindow / totalItems, 1f);
                        handle.style.height = Mathf.Clamp(percent * (currentItemsInWindow * itemHeight), Mathf.Min(3, currentItemsInWindow) * itemHeight, currentItemsInWindow * itemHeight);
                        colorActive = totalItems > maxItemsInWindow ? 1f : 0f;
                }

                public void SetHandleTop (float positionIndex)
                {
                        float percent = positionIndex / activeSize;
                        handle.style.top = Mathf.Clamp(percent * maxHandleTop, 0f, maxHandleTop);
                }

                public void SetHandleColor (Color color)
                {
                        handle.style.backgroundColor = color * colorActive;
                }
        }

}

#endif
