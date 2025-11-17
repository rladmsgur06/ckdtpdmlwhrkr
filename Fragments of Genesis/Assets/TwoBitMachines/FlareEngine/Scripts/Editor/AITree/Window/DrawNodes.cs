using System.Collections.Generic;
using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.AI;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [System.Serializable]
        public class DrawNodes
        {
                public Color backgroundColor;
                public Color connectionColor;
                public Color actionColor;
                public Color connectionHookColor;
                public Color iconColor;
                public Color nodeColor;
                public Color nodeSuccessColor;
                public GUIStyle textStyle = new GUIStyle();

                public static Texture2D compositeTexture;
                public static Texture2D decoratorTexture;
                public static Texture2D selectFrame;
                public static Texture2D nodeTexture;
                public static Texture2D hide;
                public static Texture2D unhide;
                public static Texture2D trashTexture;
                public static Texture2D circleEmpty;
                public static Texture2D circle;
                public static Texture2D guardTexture;
                public static Texture2D selfInterruptTexture;
                public static Texture2D lowerInterruptTexture;

                public static Texture2D failedTexture;
                public static Texture2D checkTexture;
                public static Texture2D recheckTexture;

                public static List<Texture2D> icons = new List<Texture2D>();
                public static float debugTimer = 0.15f;

                public void Initialize ()
                {
                        if (icons == null || icons.Count == 0)
                                EditorTools.LoadGUI("TwoBitMachines" , "/FlareEngine/AssetsFolder/GUI/DrawNodesGUI" , icons);
                        SetStyle();
                        GetTextures();
                        SetColors();
                }

                public void SetStyle ()
                {
                        textStyle.alignment = TextAnchor.MiddleCenter;
                        textStyle.fontSize = 8;
                        textStyle.wordWrap = true;
                        textStyle.fontStyle = FontStyle.Bold;
                        textStyle.normal.textColor = Tint.SoftDark;
                        textStyle.padding = Labels.rectZero;
                }

                public void GetTextures ()
                {
                        if (selfInterruptTexture == null)
                                selfInterruptTexture = icons.GetIcon("SelfInterrupt");
                        if (lowerInterruptTexture == null)
                                lowerInterruptTexture = icons.GetIcon("LowerInterrupt");
                        if (compositeTexture == null)
                                compositeTexture = icons.GetIcon("CompositeFrame");
                        if (decoratorTexture == null)
                                decoratorTexture = icons.GetIcon("DecoratorFrame");
                        if (guardTexture == null)
                                guardTexture = icons.GetIcon("Guard");
                        if (selectFrame == null)
                                selectFrame = icons.GetIcon("SelectFrame");
                        if (nodeTexture == null)
                                nodeTexture = icons.GetIcon("FrameSquare");
                        ;
                        if (trashTexture == null)
                                trashTexture = icons.GetIcon("d_TreeEditor.Trash");
                        ;
                        if (circleEmpty == null)
                                circleEmpty = icons.GetIcon("CircleEmpty");
                        if (circle == null)
                                circle = icons.GetIcon("CircleJoin");
                        if (hide == null)
                                hide = icons.GetIcon("EyeClosed");
                        if (unhide == null)
                                unhide = icons.GetIcon("EyeOpen");

                        if (failedTexture == null)
                                failedTexture = icons.GetIcon("X");
                        if (checkTexture == null)
                                checkTexture = icons.GetIcon("Check");
                        if (recheckTexture == null)
                                recheckTexture = icons.GetIcon("Recheck");
                }

                private void SetColors ()
                {
                        backgroundColor = Tint.WarmWhite;
                        nodeColor = Tint.Orange;
                        actionColor = Tint.Orange;
                        connectionColor = Tint.Blue;
                        iconColor = Tint.WarmWhite;
                        nodeSuccessColor = Tint.PastelGreen;
                        connectionHookColor = Tint.HardDark;
                        textStyle.normal.textColor = Tint.WarmWhite;
                }

                public Rect Draw (AITree tree , Node node , bool debug , Vector2 parentMoved , Vector2 gridOffset , Event currentEvent)
                {
                        bool childIsActive = false;
                        Texture2D nTexture = node.nodeType == NodeType.Composite ? (node is Decorator ? decoratorTexture : compositeTexture) : nodeTexture;
                        if (nTexture == null)
                                nTexture = Texture2D.whiteTexture;
                        node.rectSize = new Vector2(nTexture.width , nTexture.height);
                        Color nodeColorSelected = DebugNodeColor(node , debug , ref childIsActive);
                        Rect visualRect = node.GetRect(gridOffset);

                        if (node.inspectSelected)
                        {
                                Rect select = new Rect(visualRect) { x = visualRect.x - 4 , y = visualRect.y - 4 , height = 38 , width = 68 };
                                select.DrawTexture(selectFrame , Tint.HardDark);
                        }
                        if (node.failed)
                        {
                                Rect select = new Rect(visualRect) { x = visualRect.x , y = visualRect.y - 10 , height = 10 , width = 10 };
                                select.DrawTexture(failedTexture , Tint.Delete);
                                if (Clock.Timer(ref node.failedCounter , debugTimer)) // failed counter
                                {
                                        node.failed = false;
                                }
                        }
                        if (node.interruptCheck)
                        {
                                Rect select = new Rect(visualRect) { x = visualRect.x + 10 , y = visualRect.y - 10 , height = 10 , width = 10 };

                                bool failed = node.nodeState == NodeState.Failure;
                                Texture2D texture = failed ? failedTexture : checkTexture;
                                Color textureColor = failed ? Tint.Delete : Tint.PastelGreen;
                                select.DrawTexture(texture , textureColor);

                                if (Clock.Timer(ref node.interruptCounter , debugTimer))
                                {
                                        node.interruptCheck = false;
                                }
                        }

                        visualRect.DrawTexture(nTexture , nodeColorSelected);
                        IconButton(node , visualRect);
                        ParentButton(tree , node , visualRect);
                        HideChildrenButton(node , visualRect);
                        DisplayNodeConnections(node , visualRect , parentMoved , gridOffset);
                        DebugConnectionColor(node , childIsActive , parentMoved , gridOffset);
                        ChildButton(node , visualRect);

                        if (node is Composite)
                        {
                                Composite composite = node as Composite;
                                if (composite.onInterrupt == OnInterruptType.CancelInterruptAndComplete)
                                {
                                        Rect select = new Rect(visualRect) { x = visualRect.x + 22 , y = visualRect.y - 12 , height = 14 , width = 14 };
                                        select.DrawTexture(guardTexture , Tint.HardDark);
                                }
                                if (composite.canInterruptSelf)
                                {
                                        Rect select = new Rect(visualRect) { x = visualRect.x + 24 , y = visualRect.y - 17 , height = 11 , width = 11 };
                                        select.DrawTexture(selfInterruptTexture , Tint.HardDark);
                                }
                                if (composite.canInterruptLowerNodes)
                                {
                                        Rect select = new Rect(visualRect) { x = visualRect.x + 24 , y = visualRect.y - 19 , height = 11 , width = 11 };
                                        select.DrawTexture(lowerInterruptTexture , Tint.HardDark);
                                }
                        }

                        if (node.message.Count > 0)
                        {
                                for (int i = 0; i < node.message.Count; i++)
                                {
                                        Rect rect = node.message[i].GetRect(gridOffset);
                                        Skin.DrawRect(rect , Tint.HardDark);
                                        EditorGUI.LabelField(rect , node.message[i].message , textStyle);
                                }
                        }

                        return visualRect;
                }

                public Color DebugNodeColor (Node node , bool debug , ref bool childIsActive)
                {
                        Color nodeColorSelected = nodeColor;

                        if (node.copySelected)
                        {
                                nodeColorSelected = Tint.Orange;
                        }
                        if (debug && !TwoBitMachines.Clock.TimerEditorExpired(ref node.selectCounter , debugTimer)) // this will catch nodes that are active for less than a frame
                        {
                                childIsActive = true;
                                nodeColorSelected = nodeSuccessColor;
                                node.selectCounter -= EditorApplication.isPaused ? Clock.unscaledDeltaTime : 0;
                        }
                        return nodeColorSelected;
                }

                public void DebugConnectionColor (Node node , bool childIsActive , Vector2 parentMoved , Vector2 gridOffset)
                {
                        if (!childIsActive || node.nodeParent == null)
                                return;
                        ConnectBezierLine(node.nodeParent.GetRect(gridOffset) , Compute.OffsetRect(node.GetRect(parentMoved) , gridOffset) , nodeSuccessColor , 4);
                }

                public void IconButton (Node node , Rect nodeRect)
                {
                        if (node is Action || node is Conditional)
                        {
                                LeafButton(node , nodeRect);
                                nodeRect.x += 18;
                                nodeRect.width -= 21;
                                EditorGUI.LabelField(nodeRect , node.nameType , textStyle);
                                return;
                        }

                        Texture2D icon = icons.GetIcon(node.nameType); //

                        Vector2 iconSize = new Vector2(icon.width , icon.height);

                        nodeRect.width = iconSize.x;
                        nodeRect.height = iconSize.y;
                        nodeRect.x += 30;
                        nodeRect.y += 8;
                        nodeRect.DrawTexture(icon , iconColor);
                }

                public void ParentButton (AITree tree , Node node , Rect nodeRect)
                {
                        if (!node.CanHaveChildren() || node.Children() == null)
                                return;

                        Texture2D icon = node.Children().Count > 0 ? icons.GetIcon("Connected") : icons.GetIcon("Connect");
                        Vector2 childSize = new Vector2(icon.width , icon.height);
                        Vector2 buttonPosition = new Vector2(nodeRect.x + 3 , nodeRect.y + nodeRect.height - childSize.y - 3);
                        Rect buttonRect = new Rect(buttonPosition.x , buttonPosition.y , childSize.x , childSize.y);
                        Color color = Color.white;

                        if (node.hideChildren)
                        {
                                buttonRect.DrawTexture(icon , Tint.Grey200);
                                return;
                        }

                        if (node.lookingForChild)
                        {
                                buttonRect.DrawTexture(icons.GetIcon("ConnectLook") , Color.white);
                        }

                        if (buttonRect.Button(icon , color))
                        {
                                if (!node.lookingForChild)
                                {
                                        TurnOffAllLookForChildren(tree.root.children);
                                        TurnOffAllLookForChildren(tree.tempChildren);
                                }
                                node.lookingForChild = !node.lookingForChild;
                        }
                }

                public void TurnOffAllLookForChildren (List<Node> children)
                {
                        for (int i = 0; i < children.Count; i++)
                        {
                                children[i].lookingForChild = false;
                                if (children[i].CanHaveChildren() && children[i].Children() != null)
                                {
                                        TurnOffAllLookForChildren(children[i].Children());
                                }
                        }
                }

                public void ChildButton (Node node , Rect nodeRect)
                {
                        if (node is Root)
                                return; // root can't look for a parent

                        Texture2D icon = node.nodeParent == null ? icons.GetIcon("ConnectBlock") : icons.GetIcon("LookingToConnectBlock");
                        Vector2 childSize = new Vector2(icon.width , icon.height);
                        Vector2 buttonPosition = new Vector2(nodeRect.x + nodeRect.width * 0.5f - childSize.x * 0.5f , nodeRect.y - 16);
                        Rect buttonRect = new Rect(buttonPosition.x , buttonPosition.y , childSize.x , childSize.y);

                        if (buttonRect.Button(icon , connectionHookColor))
                        {
                                node.childActive = !node.childActive;
                        }
                }

                public void HideChildrenButton (Node node , Rect nodeRect)
                {
                        if (!node.CanHaveChildren() || node.Children() == null)
                                return;

                        Texture2D icon = node.hideChildren ? hide : unhide;
                        Vector2 childSize = new Vector2(icon.width , icon.height);
                        Vector2 buttonPosition = new Vector2(nodeRect.x + 3 , nodeRect.y + 3);
                        Rect buttonRect = new Rect(buttonPosition.x , buttonPosition.y , childSize.x , childSize.y);

                        if (buttonRect.Button(icon , Color.white))
                        {
                                node.hideChildren = !node.hideChildren;
                        }
                }

                public void LeafButton (Node node , Rect nodeRect)
                {
                        Texture2D icon = icons.GetIcon("Leaf");
                        Vector2 childSize = new Vector2(icon.width , icon.height);
                        Vector2 buttonPosition = new Vector2(nodeRect.x + 2 , nodeRect.y + nodeRect.height - 19);
                        Rect buttonRect = new Rect(buttonPosition.x , buttonPosition.y , childSize.x , childSize.y);
                        Color color = Color.white;
                        buttonRect.DrawTexture(icon , color);
                }

                public void DisplayNodeConnections (Node node , Rect nodeRect , Vector2 movedDelta , Vector2 gridOffset)
                {
                        if (node.hideChildren || !node.CanHaveChildren() || node.Children() == null)
                                return;

                        float parentLastDigit = Mathf.Abs(nodeRect.x) % 10;
                        List<Node> children = node.Children();
                        for (int i = children.Count - 1; i >= 0; i--)
                        {
                                Node child = node.Children()[i];
                                if (child == null)
                                {
                                        children.RemoveAt(i);
                                        continue;
                                }
                                ConnectBezierLine(nodeRect , Compute.OffsetRect(child.GetRect(movedDelta) , gridOffset) , connectionColor , 3);
                        }
                }

                public static void ConnectBezierLine (Rect parent , Rect child , Color color , float thickness = 2f)
                {
                        Vector2 origin = new Vector2(parent.x + parent.width * 0.5f , parent.y + parent.height + 1);
                        Vector2 offset = Vector2.up * thickness * 0.5f;
                        Vector2 start = origin + Vector2.up * 15f;
                        Vector2 end = new Vector2(child.x + child.width * 0.5f , child.y - 2f);
                        Vector2 corner = new Vector2(end.x , start.y);
                        Vector2 adjustedCorner = new Vector2(corner.x + Mathf.Sign(end.x - start.x) * thickness * 0.5f , corner.y);

                        Color normal = Handles.color;
                        Handles.color = color;
                        if (Mathf.Abs(end.x - start.x) > thickness) // if end point is below start point, no need to draw corner
                        {
                                Handles.DrawBezier(start , adjustedCorner , adjustedCorner , adjustedCorner , color , Texture2D.whiteTexture , thickness);
                        }
                        else
                        {
                                corner.y -= thickness * 0.5f; //        if no corner point, then add a bit of height to vertical line
                        }
                        Handles.DrawBezier(corner , end , end , end , color , Texture2D.whiteTexture , thickness);
                        Handles.DrawBezier(origin , start + offset , start + offset , start + offset , color , Texture2D.whiteTexture , thickness);
                        Handles.color = normal;
                }

        }

}
