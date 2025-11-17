using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using TwoBitMachines.Editors;

[CustomEditor(typeof(ElementsPractice))]
public class PracticeEditor : Editor
{
        public ElementsPractice main;
        public SerializedObject parent;
        public VisualElement root { get; set; }

        //
        public TextField textField;
        public IntegerField intField;
        private VisualElement mousePositionElement;

        private void OnEnable ()
        {
                main = target as ElementsPractice;
                parent = serializedObject;
                root = new VisualElement();
                //  mousePositionElement = new VisualElement();
                //  mousePositionElement.style.marginTop = 5;



        }

        public override VisualElement CreateInspectorGUI ()
        {
                //root.FlexDirection(FlexDirection.Row);

                // textField = new TextField()
                //  .Bind(parent.Get("myName"))
                //  .ToolTip("name")
                //  .FlexGrow(1);

                // intField = new IntegerField()
                //   .Bind(parent.Get("health"))
                //   .ToolTip("Health")
                //   .FlexGrow(10);

                // root
                //  .AddChild(textField)
                //  .AddChild(intField);

                // Create header box
                // Create header box
                var headerBox = new VisualElement();
                headerBox.style.height = 50;
                headerBox.style.backgroundColor = new Color(0.2f , 0.2f , 0.2f);
                root.Add(headerBox);

                // Create divider
                var divider = new VisualElement();
                divider.style.height = 2;
                divider.style.backgroundColor = Color.black;
                root.Add(divider);

                // Create bottom container
                var bottomContainer = new VisualElement();
                bottomContainer.style.flexDirection = FlexDirection.Row;
                bottomContainer.style.flexGrow = 1;

                // Create left, center, and right panes
                var leftPane = new VisualElement();
                var centerPane = new VisualElement();
                var rightPane = new VisualElement();
                leftPane.style.flexGrow = 1;
                centerPane.style.flexGrow = 1;
                rightPane.style.flexGrow = 1;

                // Add content to panes
                leftPane.Add(new Label("Left Pane"));
                centerPane.Add(new Label("Center Pane"));
                rightPane.Add(new Label("Right Pane"));

                // Add panes to bottom container
                bottomContainer.Add(leftPane);
                bottomContainer.Add(centerPane);
                bottomContainer.Add(rightPane);

                // Add header, divider, and bottom container to root
                root.Add(headerBox);
                root.Add(divider);
                root.Add(bottomContainer);

                return root;
        }

}
