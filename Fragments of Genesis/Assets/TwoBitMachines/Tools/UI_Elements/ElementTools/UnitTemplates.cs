#if UNITY_EDITOR
using System.Diagnostics.Tracing;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TwoBitMachines.UIElement.Editor
{
        public delegate void ScrollCatalog (int scroll, bool setScrollIndex = false, bool setMousePosition = false);
        public delegate void ArrayCallBack (SerializedProperty array, int index);
        public delegate void StringCallBack (string name);
        public delegate void BoolCallBack (bool open);
        public delegate void IntCallBack (int index);
        public delegate void NormalCallBack ();

        [System.Serializable]
        public class UIBlock : BindableElement //* All elements in the array must be available and can be reodered
        {
                [SerializeField] public SerializedProperty array; // need to update
                [SerializeField] public System.Object property; //   need to update
                [SerializeField] public SerializedObject so;
                [SerializeField] public string arrayName;
                [SerializeField] public bool standAlone;
                [SerializeField] public float inactive;
                [SerializeField] public Color color;
                [SerializeField] public NormalCallBack onGripUsed;

                public static int source;
                public static int destination;

                #region Interface
                public VisualElement target => !standAlone && childCount > 0 ? ElementAt(0) : this; // target should always be header

                public void Initialize (SerializedObject so, System.Object property, Color color, SerializedProperty array, bool standAlone = false, bool bind = true)
                {
                        this.so = so;
                        this.array = array;
                        this.color = color;
                        this.property = property;
                        this.standAlone = standAlone;
                        this.arrayName = array != null ? array.name : "";
                        if (bind)
                        {
                                bindingPath = property != null ? property.PropertyPath() : "";
                        }
                }

                public void Append<T> (T element) where T : VisualElement
                {
                        if (childCount > 0)
                        {
                                ElementAt(0).Add(element);
                        }
                        else
                        {
                                Add(element);
                        }
                }
                #endregion

                #region Virtual

                public virtual void OnOpen (bool open, int bottomSpaceOpen, int bottomSpaceClosed) { }

                public virtual void HideVisual (VisualElement element) { }

                public virtual void EnableContent (bool enable) { }

                public virtual void RebindContentChildren () { }

                public virtual void CallBack () { }
                #endregion

                #region Bind
                public static UIBlock gripUnit;

                public static void SwapUnits (SerializedObject so, UIBlock sourceUnit, UIBlock destinationUnit)
                {
                        if (sourceUnit == null || destinationUnit == null)
                                return;

                        SerializedProperty array = sourceUnit.array;
                        InsertVisualElement(sourceUnit, destinationUnit);

                        source = IndexOf(sourceUnit);
                        destination = IndexOf(destinationUnit);

                        array.MoveArrayElement(source, destination);
                        so.ApplyModifiedProperties();

                        RebindAll(so, sourceUnit.parent, array);
                        if (sourceUnit.onGripUsed != null)
                        {
                                sourceUnit.onGripUsed.Invoke();
                                destinationUnit.onGripUsed.Invoke();
                        }
                }

                public static void RebindAll (SerializedObject so, VisualElement parent, SerializedProperty array)
                {
                        if (parent == null || parent.childCount == 0)
                        {
                                return;
                        }
                        parent.Unbind();
                        int index = 0;
                        foreach (VisualElement child in parent.Children())
                        {
                                if (child is UIBlock unit && unit.array != null && index < array.arraySize)
                                {
                                        unit.RebindParent(array, index++);
                                }
                        }
                        parent.Bind(so);
                }

                public void RebindParent (SerializedProperty newArray, int newIndex)
                {
                        this.array = newArray;
                        this.property = newArray.Element(newIndex);
                        this.bindingPath = this.property.PropertyPath();
                        RebindContentChildren();
                }

                public void RebindParentNoArray (System.Object property)
                {
                        this.property = property;
                        this.bindingPath = this.property.PropertyPath();
                        RebindContentChildren();
                }

                public void RebindChild (VisualElement container)
                {
                        int index = 0;
                        foreach (VisualElement child in container.Children())
                        {
                                if (child is UIBlock unit && unit.array != null && index < this.property.Get(unit.arrayName).arraySize)
                                {
                                        unit.RebindParent(this.property.Get(unit.arrayName), index++);
                                }
                                else if (child is UIBlock unita && unita.array == null)
                                {
                                        unita.RebindParentNoArray(this.property);
                                }
                        }
                }

                public void RebindPropertyOnly (SerializedProperty property)
                {
                        parent.Unbind();
                        this.property = property;
                        this.bindingPath = this.property.PropertyPath();
                        parent.Bind(so);
                }

                public void RebindPropertyOnly (SerializedObject property)
                {
                        parent.Unbind();
                        this.property = property;
                        this.bindingPath = this.property.PropertyPath();
                        parent.Bind(so);
                }

                public void RebindList (SerializedObject so, SerializedProperty array, bool selfIsVisible, bool hardRebind, IntCallBack makeItem)
                {
                        if (array == null)
                                return;

                        if (!selfIsVisible && this.Visible())
                        {
                                this.Visible(false);
                        }
                        if (selfIsVisible && !this.Visible())
                        {
                                this.Visible(true);
                        }
                        if (!selfIsVisible)
                        {
                                return;
                        }

                        for (int i = 0; i < this.childCount; i++)
                        {
                                this.ElementAt(i).Visible(false);
                        }

                        this.Unbind();
                        for (int i = 0; i < array.arraySize; i++)
                        {
                                if (array.Element(i) == null)
                                {
                                        continue;
                                }
                                if (i < this.childCount)
                                {
                                        if (this.ElementAt(i) is UIBlock unit)
                                        {
                                                if (hardRebind)
                                                {
                                                        unit.property = array.Element(i);
                                                        unit.Visible(true);
                                                }
                                                else
                                                {
                                                        unit.RebindParent(array, i);
                                                        unit.Visible(true);
                                                }
                                                continue;
                                        }
                                        else
                                        {
                                                BindableElement element = this.ElementAt(i) as BindableElement;
                                                element.bindingPath = array.Element(i).propertyPath;
                                                element.Visible(true);
                                                continue;
                                        }
                                }
                                if (makeItem != null)
                                {
                                        makeItem.Invoke(i);
                                }
                        }
                        this.Bind(so);

                }

                #endregion

                #region Util
                public static void InsertVisualElement (UIBlock sourceUnit, UIBlock destinationUnit)
                {
                        VisualElement container = sourceUnit.parent;
                        container.Insert(container.IndexOf(destinationUnit), sourceUnit);
                }

                public static UIBlock GetIndexUnit (VisualElement container, int indexOf)
                {
                        int index = 0;
                        foreach (VisualElement child in container.Children())
                        {
                                if (child is UIBlock unit && unit.array != null && index++ == indexOf)
                                {
                                        return unit;
                                }
                        }
                        return null;
                }

                public static int IndexOf (UIBlock unit)
                {
                        if (unit.property is SerializedProperty element)
                        {
                                for (int i = 0; i < unit.array.arraySize; i++)
                                {
                                        if (unit.array.Element(i).propertyPath == element.propertyPath)
                                        {
                                                return i;
                                        }
                                }
                        }
                        return 0;
                }

                public int Index ()
                {
                        string path = property.PropertyPath();
                        for (int i = 0; i < array.arraySize; i++)
                        {
                                if (array.Element(i).propertyPath == path)
                                {
                                        return i;
                                }
                        }
                        return 0;
                }

                public void DeleteUnit ()
                {
                        if (array.arraySize > 0)
                        {
                                DeleteArrayUnit();
                                DeleteVisualUnit();
                        }
                }

                public void DeleteArrayUnit ()
                {
                        if (array.arraySize > 0)
                        {
                                array.Delete(IndexOf(this));
                                so.ApplyProperties();
                        }
                }

                public void InsertArrayUnit ()
                {
                        array.Insert(IndexOf(this));
                        so.ApplyProperties();
                }

                public void DeleteVisualUnit ()
                {
                        SerializedProperty arrayRef = array;
                        VisualElement parentRef = parent;
                        SerializedObject soRef = so;

                        if (parent.Contains(this))
                        {
                                parent.Remove(this);
                        }
                        if (arrayRef != null && arrayRef.arraySize > 0)
                        {
                                RebindAll(soRef, parentRef, arrayRef);
                        }
                }

                public void RebindAll ()
                {
                        RebindAll(so, parent, array);
                }

                public void ApplyBindChanges ()
                {
                        so.ApplyProperties();
                        parent.Bind(so);
                }

                public void BeginUnbind ()
                {
                        parent.Unbind();
                }
                #endregion

        }

        [System.Serializable]
        public class UIBlockList : UIBlock
        {
                [SerializeField] public ListCallBack callback;
                public delegate void ListCallBack (UIBlockList blockList);

                public UIBlockList (VisualElement container, ref NormalCallBack callBack, ref NormalCallBack callBackB, ListCallBack selfCallBack = null)
                {
                        container.Add(this);
                        callBack += CallBack;
                        callBackB += CallBack;
                        this.callback = selfCallBack;
                        CallBack();
                }

                public UIBlockList (VisualElement container, ref NormalCallBack callBack, ListCallBack selfCallBack = null)
                {
                        container.Add(this);
                        callBack += CallBack;
                        this.callback = selfCallBack;
                        CallBack();
                }

                public UIBlockList (VisualElement container, ListCallBack selfCallBack = null)
                {
                        container.Add(this);
                        this.callback = selfCallBack;
                        CallBack();
                }

                public override void CallBack ()
                {
                        this.callback.Invoke(this);
                }
        }

        [System.Serializable]
        public class Bar : UIBlock
        {
                [SerializeField] public List<VisualElement> hideVisual = new List<VisualElement>();
                [SerializeField] public bool isOpen;

                public Bar (VisualElement container, SerializedObject so, System.Object property, Color color, SerializedProperty array = null, int height = 23, int space = 2, string backgroundImage = "Header")
                {
                        Initialize(so, property, color, array, standAlone: true);
                        AddToClassList("box");

                        style.backgroundImage = Icon.Get(backgroundImage);
                        style.unityBackgroundImageTintColor = color;
                        style.flexDirection = FlexDirection.Row;
                        style.alignItems = Align.Center;
                        style.paddingLeft = space;
                        style.height = height;
                        style.marginLeft = 0;
                        style.marginTop = 0;
                        container.Add(this);
                }

                public override void OnOpen (bool open, int bottomSpaceOpen, int bottomSpaceClosed)
                {
                        isOpen = open;
                        style.marginBottom = open ? bottomSpaceOpen : bottomSpaceClosed;
                        SetHiddenVisuals(open);
                }

                public override void HideVisual (VisualElement element)
                {
                        hideVisual.Add(element);
                        SetHiddenVisuals(isOpen);
                }

                private void SetHiddenVisuals (bool open)
                {
                        for (int i = 0; i < hideVisual.Count; i++)
                        {
                                if (hideVisual[i].userData != null && hideVisual[i].userData is bool save)
                                {
                                        if (!open)
                                        {
                                                save = hideVisual[i].enabledInHierarchy;
                                                hideVisual[i].SetEnabled(false);
                                        }
                                        else if (save)
                                        {
                                                save = false;
                                                hideVisual[i].SetEnabled(true);
                                        }
                                        hideVisual[i].userData = save;
                                }
                                else
                                {
                                        hideVisual[i].SetEnabled(open);
                                }
                        }
                }
        }

        [System.Serializable]
        public class HardBindPlus : UIBlock
        {
                [SerializeField] public BindableElement header;
                [SerializeField] public BindableElement content;
                [SerializeField] public BoolCallBack openCallBack;

                [SerializeField] public List<VisualElement> hideVisual = new List<VisualElement>();
                [SerializeField] public NormalCallBack lazyLoad;
                [SerializeField] public bool isOpen;

                public HardBindPlus (VisualElement container, SerializedObject property, Color color, int height = 23, int space = 2, string backgroundImage = "Header")
                {
                        this.color = color;
                        this.property = property;
                        //  this.bindingPath = property.PropertyPath();
                        content = new BindableElement();
                        header = Template.Header(this, color, height, space, backgroundImage);
                        header.bindingPath = property.PropertyPath();
                        Add(content);
                        container.Add(this);

                        this.Bind(property);
                }

                public override void OnOpen (bool open, int bottomSpaceOpen, int bottomSpaceClosed)
                {
                        isOpen = open;
                        content.Visible(open);
                        header.style.marginBottom = open ? bottomSpaceOpen : bottomSpaceClosed;
                        SetHiddenVisuals(open);
                        LoadLazyContent();
                        if (openCallBack != null)
                        {
                                openCallBack.Invoke(open);
                        }
                }

                public override void EnableContent (bool enable)
                {
                        content.SetEnabled(enable);
                }

                public override void HideVisual (VisualElement element)
                {
                        hideVisual.Add(element);
                        SetHiddenVisuals(isOpen);
                }

                public override void RebindContentChildren ()
                {
                        RebindChild(content);
                }

                public void LazyContentLoad (NormalCallBack listener)
                {
                        lazyLoad += listener;
                        LoadLazyContent();
                }

                private void LoadLazyContent ()
                {
                        if (isOpen && lazyLoad != null)
                        {
                                parent.Unbind();
                                lazyLoad.Invoke();
                                lazyLoad = null;
                                parent.Bind(so);
                        }
                }

                private void SetHiddenVisuals (bool open)
                {
                        for (int i = 0; i < hideVisual.Count; i++)
                        {
                                if (hideVisual[i].userData != null && hideVisual[i].userData is bool save)
                                {
                                        if (!open)
                                        {
                                                save = hideVisual[i].Visible();
                                                hideVisual[i].Visible(false);
                                        }
                                        else if (save)
                                        {
                                                save = false;
                                                hideVisual[i].Visible(true);
                                        }
                                        hideVisual[i].userData = save;
                                }
                                else
                                {
                                        hideVisual[i].Visible(open);
                                }
                        }
                }

                public void ViewExtraContent (bool value)
                {
                        foreach (VisualElement child in content.Children())
                        {
                                if (child is ContentPlus extraContent)
                                {
                                        extraContent.Visible(value);
                                }
                        }
                }
        }

        [System.Serializable]
        public class BarPlus : UIBlock
        {
                [SerializeField] public BindableElement header;
                [SerializeField] public BindableElement content;
                [SerializeField] public BoolCallBack openCallBack;

                [SerializeField] public List<VisualElement> hideVisual = new List<VisualElement>();
                [SerializeField] public NormalCallBack lazyLoad;
                [SerializeField] public bool isOpen;

                public BarPlus (VisualElement container, SerializedObject so, System.Object property, Color color, SerializedProperty array = null, int height = 23, int space = 2, bool bind = true, string backgroundImage = "Header")
                {
                        Initialize(so, property, color, array, standAlone: false, bind: bind);

                        content = new BindableElement();
                        header = Template.Header(this, color, height, space, backgroundImage);
                        Add(content);
                        container.Add(this);
                }

                public override void OnOpen (bool open, int bottomSpaceOpen, int bottomSpaceClosed)
                {
                        isOpen = open;
                        content.Visible(open);
                        header.style.marginBottom = open ? bottomSpaceOpen : bottomSpaceClosed;
                        SetHiddenVisuals(open);
                        LoadLazyContent();
                        if (openCallBack != null)
                        {
                                openCallBack.Invoke(open);
                        }
                }

                public override void EnableContent (bool enable)
                {
                        content.SetEnabled(enable);
                }

                public override void HideVisual (VisualElement element)
                {
                        hideVisual.Add(element);
                        SetHiddenVisuals(isOpen);
                }

                public override void RebindContentChildren ()
                {
                        RebindChild(content);
                }

                public void LazyContentLoad (NormalCallBack listener)
                {
                        lazyLoad += listener;
                        LoadLazyContent();
                }

                private void LoadLazyContent ()
                {
                        if (isOpen && lazyLoad != null)
                        {
                                parent.Unbind();
                                lazyLoad.Invoke();
                                lazyLoad = null;
                                parent.Bind(so);
                        }
                }

                private void SetHiddenVisuals (bool open)
                {
                        for (int i = 0; i < hideVisual.Count; i++)
                        {
                                if (hideVisual[i].userData != null && hideVisual[i].userData is bool save)
                                {
                                        if (!open)
                                        {
                                                save = hideVisual[i].Visible();
                                                hideVisual[i].Visible(false);
                                        }
                                        else if (save)
                                        {
                                                save = false;
                                                hideVisual[i].Visible(true);
                                        }
                                        hideVisual[i].userData = save;
                                }
                                else
                                {
                                        hideVisual[i].Visible(open);
                                }
                        }
                }

                public void ViewExtraContent (bool value)
                {
                        foreach (VisualElement child in content.Children())
                        {
                                if (child is ContentPlus extraContent)
                                {
                                        extraContent.Visible(value);
                                }
                        }
                }
        }


        [System.Serializable]
        public class Block : UIBlock
        {
                public delegate void BlockCallBack (Block block, bool value);
                public BlockCallBack callBack;

                public Block (VisualElement container, SerializedObject so, System.Object property, Color color, SerializedProperty array = null, int bottomSpace = 1, int yOffset = 0, bool visible = true, bool bind = true)
                {
                        Initialize(so, property, color, array, standAlone: true, bind: bind);
                        AddToClassList("box");
                        AddToClassList("image-box");
                        style.unityBackgroundImageTintColor = color;
                        style.marginBottom = bottomSpace;

                        if (yOffset != 0)
                        {
                                style.top = -yOffset;
                                style.marginBottom = -(yOffset - 1);
                        }
                        container.Add(this);
                        if (!visible)
                        {
                                this.Visible(false);
                        }
                }

                public Block (VisualElement container, Color color, bool visible, int bottomSpace, int yOffset, BlockCallBack callBack)
                {
                        AddToClassList("box");
                        AddToClassList("image-box");
                        container.Add(this);
                        style.unityBackgroundImageTintColor = color;
                        style.marginBottom = bottomSpace;
                        this.callBack = callBack;

                        if (yOffset != 0)
                        {
                                style.top = -yOffset;
                                style.marginBottom = -(yOffset - 1);
                        }
                        if (!visible)
                        {
                                this.Visible(false);
                        }
                        if (callBack != null)
                        {
                                callBack.Invoke(this, visible);
                        }
                }

                public Block (VisualElement container, System.Object property, Color color)
                {
                        AddToClassList("box");
                        AddToClassList("image-box");
                        this.property = property;
                        container.Add(this);
                        style.unityBackgroundImageTintColor = this.color = color;
                }

                public Block (VisualElement container, Color color)
                {
                        AddToClassList("box");
                        AddToClassList("image-box");
                        container.Add(this);
                        style.unityBackgroundImageTintColor = this.color = color;
                }

                public Block (VisualElement container, Color color, bool visible)
                {
                        AddToClassList("box");
                        AddToClassList("image-box");
                        container.Add(this);
                        style.unityBackgroundImageTintColor = this.color = color;

                        if (!visible)
                        {
                                this.Visible(false);
                        }
                }

                public Block IsVisible (bool visible)
                {
                        if (!visible)
                        {
                                this.Visible(false);
                        }
                        return this;
                }

                public Block Property (SerializedObject so, System.Object property, SerializedProperty array, bool bind = true)
                {
                        Initialize(so, property, this.color, array, standAlone: true, bind: bind);
                        return this;
                }

                public Block Spacing (int bottomSpace, int yOffset)
                {
                        style.marginBottom = bottomSpace;
                        if (yOffset != 0)
                        {
                                style.top = -yOffset;
                                style.marginBottom = -(yOffset - 1);
                        }
                        return this;
                }

                public Block CallBack (BlockCallBack callBack)
                {
                        this.callBack = callBack;
                        if (callBack != null)
                        {
                                callBack.Invoke(this, visible);
                        }
                        return this;
                }

                public void CallBack (bool visible = true)
                {
                        callBack.Invoke(this, visible);
                }
        }

        [System.Serializable]
        public class ContentPlus : BindableElement
        {

        }

        public static class AddOns
        {
                public static Button TabCallback (this UIBlock uiBlock, string field, int tabIndex, string icon, Color colorOn, Color colorOff, IntCallBack intCallBack, bool hide = false, string tooltip = "")
                {
                        VisualElement target = uiBlock.target;
                        Button button = ElementTools.IconButton<Button>(icon);
                        button.Tooltip(tooltip);
                        target.Add(button);
                        button.userData = tabIndex;

                        button.style.unityBackgroundImageTintColor = uiBlock.property.Get(field).intValue == tabIndex ? colorOn : colorOff;

                        //  System.Action refresh = () => { };
                        button.clicked += () =>
                        {
                                for (int i = 0; i < uiBlock.target.childCount; i++)
                                {
                                        if (uiBlock.target.ElementAt(i) is Button but && but.userData != null && but.userData is int)
                                        {
                                                but.style.unityBackgroundImageTintColor = colorOff;
                                        }
                                }
                                SerializedProperty tabIndexRef = uiBlock.property.Get(field);
                                tabIndexRef.intValue = tabIndexRef.intValue == tabIndex ? -1 : tabIndex;
                                if (tabIndexRef.intValue == tabIndex)
                                {
                                        button.style.unityBackgroundImageTintColor = colorOn;
                                }
                                uiBlock.so.ApplyProperties();
                                if (intCallBack != null)
                                {
                                        intCallBack.Invoke(tabIndexRef.intValue);
                                }
                        };
                        if (hide)
                        {
                                uiBlock.HideVisual(button);
                        }
                        return button;
                }

                public static Button Callback (this UIBlock uiBlock, string icon, bool hide = false, Color? color = null, string tooltip = "")
                {
                        VisualElement target = uiBlock.target;
                        Button button = ElementTools.IconButton<Button>(icon);
                        button.Tooltip(tooltip);
                        target.Add(button);
                        if (color != null && color.HasValue)
                        {
                                button.style.unityBackgroundImageTintColor = color.Value;
                        }
                        if (hide)
                        {
                                uiBlock.HideVisual(button);
                        }
                        return button;
                }

                public static Button Callback (this UIBlock uiBlock, string icon, out Button button, bool hide = true, Color? color = null, string tooltip = "")
                {
                        button = uiBlock.Callback(icon, hide, color, tooltip);
                        button.userData = false;
                        button.Visible(false);
                        return button;
                }

                public static Button CallbackHiddenButton (this UIBlock uiBlock, string visibleIcon, string hiddenIcon, bool hide = true, Color? color = null, string tooltip = "")
                {
                        Button button = uiBlock.Callback(hiddenIcon, out Button hiddenButton, hide: hide);
                        uiBlock.Callback(visibleIcon, hide: hide, color, tooltip).clicked += () =>
                        {
                                hiddenButton.ToggleVisibility();
                        };
                        return button;
                }

                public static UIBlock Button (this UIBlock uiBlock, string field, string icon, string tooltip = "")
                {
                        ToolbarToggle toggle = ElementTools.IconButton<ToolbarToggle>(icon);
                        toggle.bindingPath = field;
                        toggle.Tooltip(tooltip);
                        uiBlock.Append(toggle);
                        return uiBlock;
                }

                public static UIBlock DropDownList (this UIBlock uiBlock, List<string> names, string field, int width = 0, int spaceRight = 0, Color color = default)
                {
                        uiBlock.target.Add(uiBlock.DropDownList(uiBlock.so, names, field, width, spaceRight, color));
                        return uiBlock;
                }

                public static UIBlock Grip (this UIBlock uiBlock, bool execute = true, string icon = "Grip", bool stopProp = true, int spaceRight = 0)
                {
                        if (!execute)
                                return uiBlock;

                        VisualElement target = uiBlock.target;
                        VisualElement grip = ElementTools.IconButton<VisualElement>("Grip");
                        grip.style.marginRight = spaceRight;
                        grip.style.marginLeft = 5;
                        target.Add(grip);
                        MouseGrip(uiBlock, target, grip, stopProp);
                        return uiBlock;
                }

                public static UIBlock GripCorner (this UIBlock uiBlock, VisualElement root, bool execute = true, bool stopProp = true, int spaceRight = 0)
                {
                        if (!execute)
                                return uiBlock;

                        VisualElement target = uiBlock.target;
                        VisualElement grip = ElementTools.IconButton<VisualElement>("GripCorner");
                        grip.style.position = Position.Absolute;
                        grip.style.left = 0;
                        grip.style.top = 0;
                        grip.style.marginLeft = 0;
                        grip.style.marginRight = 0;
                        target.Add(grip);
                        MouseGrip(uiBlock, target, grip, stopProp);
                        return uiBlock;
                }

                public static UIBlock Enable (this UIBlock uiBlock, string field, string tooltip = "")
                {
                        VisualElement target = uiBlock.target;
                        Button button = ElementTools.IconButton<Button>("Open");
                        target.Add(button);

                        bool open = uiBlock.property.Bool(field);
                        button.style.unityBackgroundImageTintColor = open ? Tint.Green : Tint.White;
                        uiBlock.EnableContent(open);

                        if (tooltip != "")
                        {
                                button.tooltip = tooltip;
                        }
                        button.RegisterCallback<PointerOutEvent>(evt =>
                        {
                                // header.style.unityBackgroundImageTintColor = color; // needs work
                        });
                        button.RegisterCallback<PointerOverEvent>(evt =>
                        {
                                //  header.style.unityBackgroundImageTintColor = color * hoverTint;
                        });
                        button.clicked += () =>
                        {
                                bool open = uiBlock.property.Toggle(field);
                                uiBlock.EnableContent(open);
                                button.style.unityBackgroundImageTintColor = open ? Tint.Green : Tint.White;
                                uiBlock.so.ApplyProperties();
                        };
                        return uiBlock;
                }

                public static UIBlock FoldOut (this UIBlock uiBlock, int bottomSpaceOpen = 1, int bottomSpaceClosed = 1, string iconOn = "ArrowDown", string iconOff = "ArrowRight", string foldout = "foldOut", string tooltip = "")
                {
                        VisualElement target = uiBlock.target;
                        Button button = ElementTools.IconButton<Button>(iconOn);
                        target.Add(button);

                        button.Tooltip(tooltip);
                        button.style.marginLeft = 5;
                        uiBlock.OnOpen(uiBlock.property.Bool(foldout), bottomSpaceOpen, bottomSpaceClosed);
                        button.SetImage(uiBlock.property.Bool(foldout) ? iconOn : iconOff);

                        button.clickable.activators.Clear();
                        button.RegisterCallback<PointerDownEvent>(evt =>
                        {
                                uiBlock.OnOpen(uiBlock.property.Toggle(foldout), bottomSpaceOpen, bottomSpaceClosed);
                                button.SetImage(uiBlock.property.Bool(foldout) ? iconOn : iconOff);
                                uiBlock.so.ApplyProperties();
                        }, TrickleDown.TrickleDown);
                        return uiBlock;
                }

                public static UIBlock FoldOut (this UIBlock uiBlock, string label, Color labelColor, int bottomSpaceOpen = 1, int bottomSpaceClosed = 1, string foldout = "foldOut", bool bold = false)
                {
                        VisualElement target = uiBlock.target;
                        Button button = new Button();
                        button.AddToClassList("clear");
                        target.Add(button);

                        button.style.flexGrow = 1;
                        button.style.marginLeft = 5;
                        button.style.height = 23;// target.style.height;

                        button.text = label;
                        button.style.color = labelColor;
                        button.style.unityTextAlign = TextAnchor.MiddleLeft;
                        uiBlock.OnOpen(uiBlock.property.Bool(foldout), bottomSpaceOpen, bottomSpaceClosed);

                        if (bold)
                        {
                                button.AddToClassList("label-bold");
                        }
                        button.clickable.activators.Clear();
                        button.RegisterCallback<PointerUpEvent>(evt =>
                        {
                                evt.target.ReleasePointer(evt.pointerId);
                                target.style.unityBackgroundImageTintColor = uiBlock.color;
                        });
                        button.RegisterCallback<PointerOutEvent>(evt =>
                        {
                                target.style.unityBackgroundImageTintColor = uiBlock.color;
                        });
                        button.RegisterCallback<PointerOverEvent>(evt =>
                        {
                                target.style.unityBackgroundImageTintColor = uiBlock.color * Tint.hoverTint;
                        });
                        button.RegisterCallback<PointerDownEvent>((EventCallback<PointerDownEvent>) (evt =>
                        {
                                evt.target.CapturePointer(evt.pointerId);
                                target.style.unityBackgroundImageTintColor = uiBlock.color * Tint.activeTint;
                                uiBlock.OnOpen(uiBlock.property.Toggle(foldout), bottomSpaceOpen, bottomSpaceClosed);
                                uiBlock.so.ApplyProperties();
                        }), TrickleDown.TrickleDown);
                        return uiBlock;
                }

                public static UIBlock Field (this UIBlock uiBlock, string field, int width = 25, float clampMin = 0, float clampMax = 0, int height = 20, int offsety = 0, bool execute = true, string tooltip = "")
                {
                        if (!execute)
                                return uiBlock;
                        VisualElement target = uiBlock.target;
                        PropertyField propertyField = new PropertyField() { style = { flexGrow = 1, marginLeft = -5, marginRight = 0, width = width, height = height } };
                        propertyField.AddToClassList("hide-label");
                        propertyField.bindingPath = field;// set relative property path
                        propertyField.Tooltip(tooltip);
                        target.Add(propertyField);
                        if (offsety != 0)
                        {
                                propertyField.style.top = offsety;
                        }

                        if (clampMin != 0 || clampMax != 0)
                        {
                                propertyField.RegisterValueChangeCallback(evt =>
                                {
                                        SerializedProperty element = uiBlock.property.Get(field);
                                        if (element.propertyType == SerializedPropertyType.Float)
                                        {
                                                element.Clamp(clampMin, clampMax);
                                        }
                                        uiBlock.so.ApplyProperties();
                                });
                        }
                        return uiBlock;
                }

                public static UIBlock FieldImgui (this UIBlock uiBlock, string field, int width = 25, int height = 20, int offsety = 0, string tooltip = "")
                {
                        VisualElement target = uiBlock.target;
                        IMGUIContainer propertyField = new IMGUIContainer(() =>
                        {
                                if (uiBlock.property != null)
                                {
                                        EditorGUI.BeginChangeCheck();
                                        EditorGUILayout.ObjectField(uiBlock.property.Get(field), GUIContent.none);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                                uiBlock.so.ApplyProperties();
                                        }
                                }
                        })
                        {
                                style = { flexGrow = 1, marginLeft = 0, marginRight = 5, width = width, height = height, top = offsety }
                        };
                        target.Add(propertyField);
                        return uiBlock;
                }

                public static UIBlock InvertedFloat (this UIBlock uiBlock, string field, int width = 25, int marginRight = 5, int height = 20, int offsety = 1, string tooltip = "")
                {
                        VisualElement target = uiBlock.target;
                        IMGUIContainer imguiContainer = new IMGUIContainer(() =>
                         {
                                 EditorGUI.BeginChangeCheck();
                                 uiBlock.property.Get(field).floatValue = 1f / EditorGUILayout.FloatField("", Compute.Round(1f / uiBlock.property.Get(field).floatValue, 0.25f));

                                 if (EditorGUI.EndChangeCheck())
                                 {
                                         uiBlock.so.ApplyProperties();
                                 }
                         });
                        imguiContainer.style.width = width;
                        imguiContainer.style.flexGrow = 1;
                        imguiContainer.style.marginRight = marginRight;
                        imguiContainer.style.marginLeft = -2;
                        imguiContainer.style.height = height;
                        imguiContainer.style.top = offsety;
                        imguiContainer.Tooltip(tooltip);
                        target.Add(imguiContainer);
                        return uiBlock;
                }

                public static UIBlock Space (this UIBlock uiBlock, int width, bool execute = true)
                {
                        if (!execute)
                                return uiBlock;
                        VisualElement target = uiBlock.target;
                        VisualElement space = new VisualElement();
                        space.style.width = width;
                        target.Add(space);
                        return uiBlock;
                }

                public static UIBlock BottomSpace (this UIBlock uiBlock, int height = 1)
                {
                        VisualElement target = uiBlock.target;
                        target.style.marginBottom = height;
                        return uiBlock;
                }

                public static UIBlock OffsetY (this UIBlock uiBlock, int offset = 1)
                {
                        VisualElement target = uiBlock.target;
                        target.style.top = -offset;
                        return uiBlock;
                }

                public static UIBlock MarginOffsetX (this UIBlock uiBlock, float offset)
                {
                        VisualElement target = uiBlock.target;
                        HeaderResize(target, offset);

                        target.RegisterCallback<GeometryChangedEvent>(evt =>
                        {
                                HeaderResize(target, offset);
                        });
                        return uiBlock;
                }

                public static BarPlus GetBarPlus (this UIBlock uiBlock)
                {
                        return uiBlock as BarPlus;
                }

                public static void HeaderResize (VisualElement target, float offset)
                {
                        float targetLength = Screen.width - offset; // Target length after subtraction
                        target.style.width = new StyleLength(Length.Percent((targetLength / Screen.width) * 100f));
                        target.style.marginLeft = new StyleLength(Length.Percent((offset / Screen.width) * 100f));
                }

                private static void MouseGrip (UIBlock uiBlock, VisualElement target, VisualElement grip, bool stopProp)
                {
                        target.RegisterCallback<MouseDownEvent>(evt =>
                        {
                                if (grip.ContainsPosition(evt.mousePosition))
                                {
                                        UIBlock.gripUnit = uiBlock;
                                        target.CaptureMouse(); // let the header vi capture the mouse instead
                                        if (stopProp)
                                        {
                                                evt.StopPropagation();
                                        }
                                        grip.style.unityBackgroundImageTintColor = Tint.WarmGrey;
                                        target.style.unityBackgroundImageTintColor = uiBlock.color * Tint.activeTint;
                                }
                        });
                        target.RegisterCallback<MouseUpEvent>(evt =>
                        {
                                UIBlock.gripUnit = null;
                                target.ReleaseMouse();
                                grip.style.unityBackgroundImageTintColor = Tint.White;
                                target.style.unityBackgroundImageTintColor = uiBlock.color;
                        });
                        target.RegisterCallback<MouseEnterEvent>(evt =>
                        {
                                if (UIBlock.gripUnit != null && UIBlock.gripUnit != uiBlock && UIBlock.gripUnit.parent == uiBlock.parent)
                                {
                                        UIBlock.SwapUnits(uiBlock.so, UIBlock.gripUnit, uiBlock);
                                }
                        });
                }

        }

        public static class Fields
        {
                public static T SetParent<T> (this T t, VisualElement container) where T : VisualElement
                {
                        container.Add(t);
                        return t;
                }

                public static void Rebind (this BindableElement element, string path, SerializedObject so)
                {
                        element.parent.Unbind();
                        element.bindingPath = path;
                        element.parent.Bind(so);
                }

                public static void Rebind (this PropertyField field, string path, SerializedObject so)
                {
                        field.parent.Unbind();
                        field.bindingPath = path;
                        field.parent.Bind(so);
                }

                public static PropertyField MonitorField (this UIBlock uiBlock, string field, NormalCallBack callBack)
                {
                        PropertyField property = new PropertyField();
                        property.bindingPath = field;
                        uiBlock.Add(property);
                        property.Visible(false);
                        callBack.Invoke();
                        property.RegisterValueChangeCallback(evt =>
                        {
                                callBack.Invoke();
                        });
                        return property;
                }

                public static VisualElement Field (this UIBlock uiBlock, string label, string field, string tooltip = "")
                {
                        PropertyField property = new PropertyField();
                        property.label = label;
                        property.bindingPath = field;
                        property.Tooltip(tooltip);
                        uiBlock.Add(property);
                        return property;
                }

                public static VisualElement FieldDouble (this VisualElement parent, SerializedProperty property, string label, string fieldA, string fieldB, float weight1 = 1f, float weight2 = 1f, bool execute = true, string tooltip = "")
                {
                        if (!execute)
                                return null;
                        BindableElement container = Template.HorizontalContainer(parent);
                        container.bindingPath = property.propertyPath;
                        if (label != "")
                        {
                                Template.Label(container, label);
                        }
                        Template.Field(container, fieldA, -5, weight1);
                        Template.Field(container, fieldB, 0, weight2);
                        container.Tooltip(tooltip);
                        return container;
                }

                public static VisualElement FieldDouble (this UIBlock uiBlock, string label, string fieldA, string fieldB)
                {
                        BindableElement container = Template.HorizontalContainer(uiBlock);
                        Template.Label(container, label);
                        Template.Field(container, fieldA, -5);
                        Template.Field(container, fieldB, 0);
                        return container;
                }

                public static VisualElement Button (this VisualElement container, string icon, NormalCallBack callBack)
                {
                        Button button = ElementTools.IconButton<Button>(icon);
                        button.style.alignSelf = Align.Center;
                        button.clicked += () =>
                        {
                                callBack.Invoke();
                        };
                        container.Add(button);
                        return container;
                }

                public static VisualElement FieldAndEnable (this UIBlock uiBlock, string label, string field, string toggle, string tooltip = "")
                {
                        BindableElement container = Template.HorizontalContainer(uiBlock);
                        Template.Label(container, label);
                        PropertyField property = Template.Field(container, field, 0);
                        Template.Toggle(container, toggle).RegisterValueChangedCallback((evt) =>
                        {
                                property.SetEnabled(evt.newValue);
                        });
                        return container;
                }

                public static Toggle FieldToggle (this UIBlock uiBlock, string label, string toggle, BoolCallBack onToggle = null, string tooltip = "")
                {
                        BindableElement container = Template.HorizontalContainer(uiBlock);
                        Label labelField = Template.Label(container, label);
                        container.Tooltip(tooltip);
                        Toggle toggleButton = Template.Toggle(container, toggle);
                        toggleButton.RegisterValueChangedCallback((evt) =>
                        {
                                labelField.SetEnabled(evt.newValue);
                                if (onToggle != null)
                                {
                                        onToggle.Invoke(evt.newValue);
                                }
                        });
                        return toggleButton;
                }

                public static VisualElement Slider (this UIBlock uiBlock, string label, string field, float start = 0, float end = 1f, string tooltip = "")
                {
                        VisualElement container = new VisualElement();
                        uiBlock.Add(container);
                        container.style.flexDirection = FlexDirection.Row;

                        Slider slider = new Slider(label, start, end);
                        slider.bindingPath = field;
                        slider.style.flexGrow = 1;
                        container.Add(slider);

                        Toggle enable = new Toggle();
                        enable.style.flexShrink = 0;

                        FloatField floatField = new FloatField();
                        floatField.bindingPath = field;
                        floatField.style.width = 50;
                        container.Add(floatField);

                        if (tooltip != "")
                        {
                                container.tooltip = tooltip;
                        }
                        floatField.RegisterValueChangedCallback(evt =>
                        {
                                floatField.value = Mathf.Clamp(floatField.value, start, end);
                        });
                        return container;
                }

                public static Button DropDownList (this UIBlock uiBlock, SerializedObject so, List<string> names, string field, int width = 0, int spaceRight = 0, Color color = default)
                {
                        VisualElement target = uiBlock.target;
                        Button button = new Button();
                        target.Add(button);
                        button.style.flexGrow = 1;
                        button.style.marginTop = 0;
                        button.style.paddingTop = 0;
                        button.style.marginRight = 0;
                        button.style.paddingRight = 0;
                        button.style.height = 18;

                        button.style.backgroundColor = color.Default();
                        button.style.unityTextAlign = TextAnchor.MiddleLeft;
                        button.style.marginLeft = StyleKeyword.Auto;
                        button.style.flexDirection = FlexDirection.Row;
                        button.style.alignItems = Align.Center;

                        Image image = new Image();
                        image.SetImage("DropDown");
                        image.style.marginLeft = StyleKeyword.Auto;
                        image.style.marginRight = 0;
                        image.style.unityBackgroundImageTintColor = color.Default();
                        button.Add(image);

                        button.bindingPath = field;
                        SerializedProperty name = uiBlock.property.Get(field);

                        if (spaceRight != 0)
                        {
                                button.style.marginRight = spaceRight;
                        }
                        if (width != 0)
                        {
                                button.style.width = width;
                        }
                        if (name != null && names.Count > 0)
                        {
                                bool isEmpty = name.stringValue == "";
                                button.text = isEmpty ? names[0] : name.stringValue;
                                if (isEmpty)
                                        name.stringValue = button.text;
                        }
                        else if (names.Count == 0)
                        {
                                button.text = "Empty";
                        }
                        button.RegisterCallback<MouseOutEvent>(evt =>
                        {
                                button.style.backgroundColor = color.Default();
                        });
                        button.RegisterCallback<MouseOverEvent>(evt =>
                        {
                                button.style.backgroundColor = color.Default() * Tint.hoverTint;
                        });
                        button.clicked += () =>
                        {
                                GenericMenu menu = new GenericMenu();
                                for (int i = 0; i < names.Count; i++)
                                {
                                        string itemName = names[i];
                                        menu.AddItem(new GUIContent(itemName), button.text == itemName, () =>
                                        {
                                                so.Update();
                                                uiBlock.property.Get(field).stringValue = itemName;
                                                button.text = itemName;
                                                so.ApplyModifiedProperties();
                                        });
                                }
                                menu.ShowAsContext();
                        };
                        return button;
                }

                public static Button DropDownListAndButton (this UIBlock uiBlock, SerializedObject so, List<string> names, string label, string field, string icon, out Button dropDown, string tooltip = "")
                {
                        BindableElement container = Template.HorizontalContainer(uiBlock);

                        Template.Label(container, label);

                        dropDown = uiBlock.DropDownList(so, names, field);
                        dropDown.style.marginLeft = 3;
                        dropDown.style.marginRight = 2;
                        container.Add(dropDown);

                        Button button = ElementTools.IconButton<Button>(icon);
                        button.style.flexShrink = 0;
                        button.style.alignSelf = Align.Center;
                        container.Add(button);

                        container.Tooltip(tooltip);
                        return button;
                }

                public static VisualElement DropDownDoubleList (this UIBlock uiBlock, SerializedObject so, List<string> names, string label, string fieldA, string fieldB, string tooltip = "")
                {
                        BindableElement container = Template.HorizontalContainer(uiBlock);

                        Template.Label(container, label);

                        Button dropDownA = uiBlock.DropDownList(so, names, fieldA);
                        dropDownA.style.marginLeft = 3;
                        dropDownA.style.marginRight = 0;
                        container.Add(dropDownA);

                        Button dropDownB = uiBlock.DropDownList(so, names, fieldB);
                        dropDownB.style.marginLeft = 1;
                        dropDownB.style.marginRight = 3;
                        container.Add(dropDownB);

                        container.Tooltip(tooltip);
                        return container;
                }

                public static VisualElement EventField (this UIBlock uiBlock, VisualElement container, string field, Color color, float tint = 1f, int bottomSpace = 0)
                {
                        IMGUIContainer eventField = new IMGUIContainer(() =>
                        {
                                uiBlock.so.Update();
                                EditorGUI.BeginChangeCheck();
                                Color previous = GUI.backgroundColor;
                                GUI.backgroundColor = color * tint;
                                EditorGUILayout.PropertyField(uiBlock.property.Get(field));
                                GUI.backgroundColor = previous;
                                if (EditorGUI.EndChangeCheck())
                                {
                                        uiBlock.so.ApplyModifiedProperties();
                                }
                        });
                        eventField.AddToClassList("box");
                        eventField.style.backgroundImage = Icon.Get("HeaderBasic");
                        eventField.style.unityBackgroundImageTintColor = color * (tint - 0.3f);
                        eventField.style.paddingBottom = 0;
                        eventField.style.paddingTop = 0;
                        if (bottomSpace != 0)
                        {
                                eventField.style.marginBottom = bottomSpace;
                        }
                        container.Add(eventField);
                        return eventField;
                }


        }

        public static class Imgui
        {
                public static bool GUIEnableState;

                public static void Enable (bool condition = true)
                {
                        GUIEnableState = GUI.enabled;
                        GUI.enabled = condition;
                }

                public static void EnableEnd ()
                {
                        GUI.enabled = GUIEnableState;
                }

                public static void Button (this SerializedProperty property, string icon, Color colorOn, Color colorOff, int size = 18)
                {
                        Color previous = GUI.color;
                        GUI.color = property.boolValue ? colorOn : colorOff;
                        if (GUILayout.Button(GUIContent.none, GUIStyle.none, GUILayout.Width(size), GUILayout.Height(size))) //, GUILayout.Height(18)))
                        {
                                property.boolValue = !property.boolValue;
                        }
                        GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent(Icon.Get(icon)));
                        GUI.color = previous;
                }

                public static void InfoBox (string message)
                {
                        EditorGUILayout.HelpBox(message, MessageType.Info);
                }

        }
}
#endif
