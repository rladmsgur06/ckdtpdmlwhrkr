using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;

public class ChronosEditorWindow : EditorWindow
{
        public VisualElement root;
        public VisualElement rightPane;
        [SerializeField] private int m_SelectedIndex = -1;

        // [MenuItem("Window/Flare Chronos")]
        public static void ShowChronos ()
        {
                ChronosEditorWindow wnd = GetWindow<ChronosEditorWindow>();
                wnd.titleContent = new GUIContent("Flare Chronos");

                // Limit size of the window
                wnd.minSize = new Vector2(450, 200);
                wnd.maxSize = new Vector2(1920, 720);
        }

        public void CreateGUI ()
        {
                // Each editor window contains a root VisualElement object
                root = rootVisualElement;

                // VisualElements objects can contain other VisualElement following a tree hierarchy.
                VisualElement label = new Label("Hello World! From C#");
                label.style.height = 20;
                root.Add(label);

                var allObjectGuids = AssetDatabase.FindAssets("t:Sprite");
                var allObjects = new List<Sprite>();
                foreach (var guid in allObjectGuids)
                {
                        allObjects.Add(AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)));
                }
                //
                // Create a two-pane view with the left pane being fixed with
                var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
                // A TwoPaneSplitView always needs exactly two child elements
                var leftPane = new ListView();
                splitView.Add(leftPane);
                rightPane = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
                splitView.Add(rightPane);
                leftPane.makeItem = () =>
                {
                        return new Label();
                };
                leftPane.bindItem = (element, i) =>
                {
                        (element as Label).text = allObjects[i].name;
                };
                leftPane.itemsSource = allObjects;
                //leftPane.onSelectionChange -= OnSpriteSelectionChange;
                leftPane.onSelectionChange += OnSpriteSelectionChange;
                leftPane.selectedIndex = m_SelectedIndex;

                // Store the selection index when the selection changes
                leftPane.onSelectionChange += (items) => { m_SelectedIndex = leftPane.selectedIndex; };


                // leftPane.makeItem = CreateItem;

                // // Define a method to bind data to items
                // void BindItem (VisualElement item , int index)
                // {
                //         Label label = item as Label;
                //         if (label != null)
                //         {
                //                 label.text = allObjects[index].name;
                //         }
                // }
                // leftPane.bindItem = BindItem;

                // // Set the items source
                // leftPane.itemsSource = allObjects;


                // Add the view to the visual tree by adding it as a child to the root element
                root.Add(splitView);
        }

        private void OnSpriteSelectionChange (IEnumerable<object> selectedItems)
        {
                //   Debug.Log("Clear");
                // Clear all previous content from the pane
                rightPane.Clear();

                // Get the selected sprite
                var selectedSprite = selectedItems.First() as Sprite;
                if (selectedSprite == null)
                        return;

                // Add a new Image control and display the sprite
                Image spriteImage = new Image();
                spriteImage.scaleMode = ScaleMode.ScaleToFit;
                spriteImage.image = selectedSprite.texture;

                // Add the Image control to the right-hand pane
                rightPane.Add(spriteImage);
        }

        VisualElement CreateItem ()
        {
                return new Label();
        }
}
