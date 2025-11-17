#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class Grid : Action
        {
                [SerializeField] public int rowLength = 1;
                [SerializeField] public Vector2 space = Vector2.one;
                [SerializeField] public Vector2 offset = -Vector2.one;
                [SerializeField] public Vector2 highlightOffset;
                [SerializeField] public bool setGrid = true;

                [SerializeField] public WorldFloat payment;
                [SerializeField] public bool useTempValue;
                [SerializeField] public string saveKey = "GridKey";

                [SerializeField] public InputButtonSO left;
                [SerializeField] public InputButtonSO right;
                [SerializeField] public InputButtonSO up;
                [SerializeField] public InputButtonSO down;
                [SerializeField] public InputButtonSO select;
                [SerializeField] public GameObject highlight;
                [SerializeField] public UnityEvent onGridSet = new UnityEvent();

                [SerializeField] private int rows = 1;
                [SerializeField] private Vector2Int index;
                [SerializeField] private int previousRealIndex = -1;
                [SerializeField] private Transform previousCurrentItem;
                [SerializeField] private GridItem oldGridItem;
                [SerializeField] private Transform currentItem;
                [SerializeField] private bool gridSet;
                [SerializeField] private bool end;

                [SerializeField] private SaveStringList saved = new SaveStringList();

                private int realIndex => index.x + index.y * rowLength;

                public override void OnReset (bool skip = false, bool enteredState = false)
                {
                        if (oldGridItem != null)
                        {
                                oldGridItem.OnOutOfFocus();
                        }
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                index = Vector2Int.zero;
                                previousRealIndex = -1;
                                previousCurrentItem = null;
                                oldGridItem = null;
                                currentItem = null;
                                gridSet = false;
                                end = false;
                                saved = Storage.Load<SaveStringList>(saved, WorldManager.saveFolder, saveKey);
                                if (setGrid)
                                {
                                        SetGrid();
                                }
                        }
                        Navigate();

                        if (end)
                        {
                                TurnOff();
                        }

                        return end ? NodeState.Success : NodeState.Running;
                }

                public void SaveManually ()
                {
                        Storage.Save(saved, WorldManager.saveFolder, saveKey);
                }

                public void SaveItem (string ID)
                {
                        if (!saved.list.Contains(ID))
                        {
                                saved.list.Add(ID);
                        }
                }

                public bool ItemIsPurchased (string ID)
                {
                        return saved.list.Contains(ID);
                }

                public override bool HardReset ()
                {
                        TurnOff();
                        return true;
                }

                public void EndGrid ()
                {
                        end = true;
                }

                public void SelectThisGridItem (Transform gridItem)
                {
                        if (rowLength == 0)
                                return;

                        int x = 0;
                        for (int i = 0; i < transform.childCount; i++)
                        {
                                Transform child = transform.GetChild(i);
                                if (child != null && child == gridItem)
                                {
                                        index.x = x;
                                        index.y = (i - index.x) / rowLength;
                                        return;
                                }
                                if (child != null && child.childCount > 0)
                                {
                                        for (int j = 0; j < child.childCount; j++)
                                        {
                                                Transform child2 = child.GetChild(j);
                                                if (child2 != null && child2 == gridItem)
                                                {
                                                        index.x = x;
                                                        index.y = (i - index.x) / rowLength;
                                                        return;
                                                }
                                        }
                                }
                                x = (x + 1) >= rowLength ? 0 : x + 1;
                        }
                }

                private void Navigate ()
                {
                        Move();

                        if (!CurrentItemExists())
                        {
                                return;
                        }
                        if (currentItem != previousCurrentItem && oldGridItem != null)
                        {
                                oldGridItem.OnOutOfFocus();
                        }

                        GridItem gridItem = GetGridItem();

                        if (select != null && select.Pressed() && previousCurrentItem == currentItem && gridItem != null)
                        {
                                if (select.type != InputType.Mouse || !Tool.PointerOverUI())
                                {
                                        gridItem.OnSelect();
                                }
                        }
                        previousCurrentItem = currentItem;
                        oldGridItem = gridItem;

                        if (highlight != null && realIndex >= 0 && realIndex < transform.childCount)
                        {
                                highlight.transform.position = transform.GetChild(realIndex).position + (Vector3) highlightOffset;
                        }
                        if (gridItem != null)
                        {
                                highlight.SetActive(gridItem.gridElement);
                        }
                }

                private void Move ()
                {
                        if (right != null && right.Pressed())
                        {
                                index.x = index.x + 1 >= rowLength ? 0 : index.x + 1;
                                if (realIndex >= transform.childCount)
                                {
                                        index.x = 0;
                                }
                                if (realIndex >= 0 && realIndex < transform.childCount) // skip deactivated gameobjects.
                                {
                                        for (int i = realIndex; i < transform.childCount; i++)
                                        {
                                                Transform child = transform.GetChild(i);
                                                if (child != null && !child.gameObject.activeInHierarchy)
                                                {
                                                        index.x = index.x + 1 >= rowLength ? 0 : index.x + 1;
                                                        continue;
                                                }
                                                break;
                                        }
                                }
                                if (realIndex >= transform.childCount)
                                {
                                        index.x = 0;
                                }
                        }
                        if (left != null && left.Pressed())
                        {
                                index.x = index.x - 1 < 0 ? rowLength - 1 : index.x - 1;
                                if (realIndex >= transform.childCount)
                                {
                                        index.x = (transform.childCount - 1) - index.y * rowLength;
                                }
                                if (realIndex >= 0 && realIndex < transform.childCount)
                                {
                                        for (int i = realIndex; i >= 0; i--)
                                        {
                                                Transform child = transform.GetChild(i);
                                                if (child != null && !child.gameObject.activeInHierarchy)
                                                {
                                                        index.x = index.x - 1 < 0 ? rowLength - 1 : index.x - 1;
                                                        continue;
                                                }
                                                break;
                                        }
                                }
                                if (realIndex >= transform.childCount)
                                {
                                        index.x = (transform.childCount - 1) - index.y * rowLength;
                                }
                        }
                        if (down != null && down.Pressed())
                        {
                                index.y = index.y + 1 >= rows ? 0 : index.y + 1;
                                if (realIndex >= transform.childCount)
                                {
                                        index.y = 0;
                                }
                                if (realIndex >= 0 && realIndex < transform.childCount)
                                {
                                        for (int i = realIndex; i < transform.childCount; i++)
                                        {
                                                Transform child = transform.GetChild(i);
                                                if (child != null && !child.gameObject.activeInHierarchy)
                                                {
                                                        index.y = index.y + 1 >= rows ? 0 : index.y + 1;
                                                        continue;
                                                }
                                                break;
                                        }
                                }
                                if (realIndex >= transform.childCount)
                                {
                                        index.y = 0;
                                }
                        }
                        if (up != null && up.Pressed())
                        {
                                index.y = index.y - 1 < 0 ? rows - 1 : index.y - 1;
                                if (realIndex >= transform.childCount)
                                {
                                        index.y = rows - 2;
                                }
                                if (realIndex >= 0 && realIndex < transform.childCount)
                                {
                                        for (int i = realIndex; i >= 0; i--)
                                        {
                                                Transform child = transform.GetChild(i);
                                                if (child != null && !child.gameObject.activeInHierarchy)
                                                {
                                                        index.y = index.y - 1 < 0 ? rows - 1 : index.y - 1;
                                                        continue;
                                                }
                                                break;
                                        }
                                }
                                if (realIndex >= transform.childCount)
                                {
                                        index.y = rows - 2;
                                }
                        }

                }

                private bool CurrentItemExists ()
                {
                        if (realIndex >= 0 && realIndex < transform.childCount)
                        {
                                currentItem = transform.GetChild(realIndex);
                        }
                        if (currentItem == null)
                        {
                                return false;
                        }
                        return true;
                }

                private GridItem GetGridItem ()
                {
                        GridItem gridItem = currentItem.GetComponent<GridItem>();
                        if (gridItem == null)
                        {
                                for (int i = 0; i < currentItem.childCount; i++)
                                {
                                        Transform child = currentItem.GetChild(i);
                                        if (child != null && child.gameObject.activeInHierarchy)
                                        {
                                                gridItem = child.GetComponent<GridItem>();
                                                if (gridItem != null)
                                                        break;
                                        }
                                }
                        }
                        if (previousRealIndex != realIndex)
                        {
                                if (gridItem != null)
                                {
                                        gridItem.OnFocus();
                                }
                                previousRealIndex = realIndex;
                        }
                        return gridItem;
                }

                public void SetGrid ()
                {
                        if (gridSet)
                        {
                                return;
                        }
                        onGridSet.Invoke();
                        gridSet = true;
                        Vector2 start = (Vector2) transform.position + offset;
                        Vector2 advanced = Vector2.zero;
                        int lengthIndex = 0;
                        rowLength = rowLength <= 0 ? 1 : rowLength;
                        rows = 1;

                        for (int i = 0; i < transform.childCount; i++)
                        {
                                Transform child = transform.GetChild(i);
                                if (child == null)
                                {
                                        continue;
                                }

                                GridItem gridItem = child.GetComponent<GridItem>();
                                if (gridItem != null)
                                {
                                        if (gridItem.gridElement)
                                        {
                                                child.position = start + advanced;
                                        }
                                        gridItem.Initialize(this);
                                }
                                if (gridItem == null || gridItem.gridElement)
                                {
                                        child.position = start + advanced;
                                }
                                else
                                {
                                        continue;
                                }

                                lengthIndex++;
                                advanced.x += space.x;

                                if (lengthIndex >= rowLength)
                                {
                                        rows++;
                                        lengthIndex = 0;
                                        advanced.x = 0;
                                        advanced.y -= space.y;
                                }
                        }
                }

                private void TurnOff ()
                {
                        if (highlight != null)
                        {
                                highlight.SetActive(false);
                        }
                        for (int i = 0; i < transform.childCount; i++)
                        {
                                Transform child = transform.GetChild(i);
                                if (child == null)
                                {
                                        continue;
                                }
                                GridItem gridItem = child.GetComponent<GridItem>();
                                if (gridItem != null)
                                {
                                        gridItem.ResetAll();
                                }
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool onGridSetFoldOut;
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        color = Tint.PurpleDark;

                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Create a 2D grid from all the child objects. Only active objects will be placed in the grid." +
                                        "\n \nReturns Running");
                        }

                        FoldOut.Box(5, color, offsetY: -2);
                        {
                                parent.Field("Row Length", "rowLength");
                                parent.Field("Grid Offset", "offset");
                                parent.Field("Element Spacing", "space");
                                parent.FieldToggleAndEnable("Set Grid On Enter", "setGrid");
                        }
                        Layout.VerticalSpacing(3);

                        FoldOut.Box(3, color);
                        {
                                parent.Field("Payment", "payment");
                                parent.FieldToggle("Use Temp Value", "useTempValue");
                                parent.Field("Save Key", "saveKey");
                        }
                        Layout.VerticalSpacing(5);

                        FoldOut.Box(2, color, extraHeight: 2, offsetY: -2);
                        {
                                parent.FieldDouble("Left, Right", "left", "right");
                                parent.FieldDouble("Up, Down", "up", "down");
                        }
                        FoldOut.Box(3, color, extraHeight: 2, offsetY: -2);
                        {
                                parent.Field("Select", "select");
                                parent.Field("Highlight", "highlight");
                                parent.Field("Higlight Offset", "highlightOffset");
                        }
                        Fields.EventFoldOut(parent.Get("onGridSet"), parent.Get("onGridSetFoldOut"), "On Grid Set", color: color);

                        Layout.VerticalSpacing(5);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
