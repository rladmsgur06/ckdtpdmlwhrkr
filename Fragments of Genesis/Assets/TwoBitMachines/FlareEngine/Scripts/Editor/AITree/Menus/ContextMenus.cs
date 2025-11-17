using System;
using System.Collections.Generic;
using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.AI;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        public class ContextMenus
        {
                private Node nodeRef;
                private NodeMessage messageRef;
                private BranchTemplate templateRef;
                private AITreeWindowEditor editorRef;
                private Vector3 mousePosition;

                public void Set (AITreeWindowEditor editor, BranchTemplate template)
                {
                        editorRef = editor;
                        templateRef = template;
                }

                public void NodeMenu ()
                {
                        Rect rootPosition = editorRef.zoom.GroupRect;
                        mousePosition = Event.current.mousePosition - editorRef.gridOffset;

                        if (rootPosition.ContainsMouseRightDown())
                        {
                                editorRef.mousePosition = new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y);
                                editorRef.mousePosition = Compute.Round(editorRef.mousePosition, 15f);
                                GenericMenu menu = new GenericMenu();
                                for (int i = 0; i < AITreeWindowEditor.nodeList.Count; i++)
                                {
                                        string folder = AITreeWindowEditor.nodeList[i];
                                        menu.AddItem(new GUIContent(folder), false, OnNodeMenu, folder);
                                }
                                if (templateRef != null)
                                {
                                        for (int i = 0; i < templateRef.template.Count; i++)
                                        {
                                                string folder = "Template/" + templateRef.template[i].templateKey;
                                                menu.AddItem(new GUIContent(folder), false, OnNodeMenu, folder);
                                        }
                                        for (int i = 0; i < templateRef.template.Count; i++)
                                        {
                                                string folder = "Template/Delete/" + templateRef.template[i].templateKey;
                                                menu.AddItem(new GUIContent(folder), false, OnNodeMenu, folder);
                                        }
                                }
                                menu.ShowAsContext();
                        }
                }

                public void NoteMenu (Node node, NodeMessage message)
                {
                        nodeRef = node;
                        messageRef = message;

                        GenericMenu menu = new GenericMenu();
                        {
                                if (!(node is Root))
                                        menu.AddItem(new GUIContent("Delete Note"), false, OnNoteMenu, "Note");
                        }
                        menu.ShowAsContext();
                }

                public void UtilityMenu (Node node)
                {
                        nodeRef = node;
                        GenericMenu menu = new GenericMenu();
                        {
                                if (node is Root)
                                {
                                        menu.AddItem(new GUIContent("Add Notes"), false, OnUtilityMenu, "Notes");
                                }
                                else
                                {
                                        menu.AddItem(new GUIContent("Duplicate Node"), false, OnUtilityMenu, "Duplicate Node");
                                        menu.AddItem(new GUIContent("Duplicate Branch"), false, OnUtilityMenu, "Duplicate Branch");
                                        menu.AddItem(new GUIContent("Delete Node"), false, OnUtilityMenu, "Delete Node");
                                        menu.AddItem(new GUIContent("Delete Branch"), false, OnUtilityMenu, "Delete Branch");
                                        menu.AddItem(new GUIContent("Add As Template"), false, OnUtilityMenu, "CreateTemplate");
                                        menu.AddItem(new GUIContent("Add Notes"), false, OnUtilityMenu, "Notes");
                                }
                        }
                        menu.ShowAsContext();
                }

                private void OnNodeMenu (object obj)
                {
                        if (editorRef == null)
                                return;
                        string path = (string) obj;
                        string[] nameType = path.Split('/');
                        string delete = nameType.Length >= 2 ? nameType[1] : "none";
                        CreateNode(editorRef.tree.tempChildren, nameType[nameType.Length - 1], nameType[0], delete);
                }

                private void OnNoteMenu (object obj)
                {
                        if (editorRef == null || nodeRef == null)
                                return;

                        for (int i = 0; i < nodeRef.message.Count; i++)
                        {
                                if (nodeRef.message[i] == messageRef)
                                {
                                        nodeRef.message.RemoveAt(i);
                                        break;
                                }
                        }
                        editorRef.Repaint();
                        nodeRef = null;
                }

                private void OnUtilityMenu (object obj)
                {
                        if (editorRef == null || editorRef.tree == null || nodeRef == null)
                                return;

                        string type = (string) obj;
                        AITree tree = editorRef.tree;

                        if (type == "Delete Branch")
                        {
                                if (nodeRef is Root)
                                {
                                        DeleteAllInBranch(tree.root.Children());
                                }
                                else
                                {
                                        DeleteBranch(tree.root.Children(), nodeRef);
                                        DeleteBranch(tree.tempChildren, nodeRef);
                                }
                        }
                        else if (type == "Delete Node")
                        {
                                DeleteNode(tree.root.Children(), tree.tempChildren, nodeRef);
                                DeleteNode(tree.tempChildren, tree.tempChildren, nodeRef);
                        }
                        else if (type == "Notes")
                        {
                                NodeMessage newMessage = new NodeMessage(nodeRef.rectPosition);
                                nodeRef.message.Add(newMessage);
                                tree.nodeMessage = newMessage;
                                tree.showNodeMessage = true;
                        }
                        else if (type == "CreateTemplate" && templateRef != null)
                        {
                                TemplateKey newTemplate = new TemplateKey();
                                newTemplate.templateKey = nodeRef.message.Count > 0 ? nodeRef.message[0].message : "branch";
                                CreateTemplate(newTemplate.node, nodeRef);
                                if (!tree.tempChildren.Contains(nodeRef))
                                        tree.tempChildren.Add(nodeRef);
                                templateRef.template.Add(newTemplate);
                        }
                        else if (type == "Duplicate Node")
                        {
                                if (CreatedAndCopyNode(nodeRef.nameType, nodeRef.rectPosition + Vector2.up * 40f, nodeRef, out Node newNode))
                                {
                                        newNode.inspectSelected = false;
                                        newNode.SetParent(null);
                                        tree.tempChildren.Add(newNode);
                                        ResetMessagePosition(newNode);
                                        if (newNode.Children() != null)
                                                newNode.Children().Clear();
                                }
                        }
                        else if (type == "Duplicate Branch")
                        {
                                if (CreatedAndCopyNode(nodeRef.nameType, nodeRef.rectPosition + Vector2.up * 40f, nodeRef, out Node newNode))
                                {
                                        newNode.inspectSelected = false;
                                        newNode.SetParent(null);
                                        tree.tempChildren.Add(newNode);
                                        CopyNodeChildren(newNode, tree);
                                }
                        }

                        SortChildren(tree.root.Children());
                        SortChildren(tree.tempChildren);
                        editorRef.Repaint();
                        nodeRef = null;
                }

                private void CreateNode (List<Node> children, string nameType, string folder, string delete)
                {
                        if (editorRef == null || editorRef.tree == null)
                                return;

                        if (folder == "Template")
                        {
                                if (templateRef == null)
                                        return;

                                for (int i = 0; i < templateRef.template.Count; i++)
                                {
                                        if (templateRef.template[i].templateKey == nameType)
                                        {
                                                if (delete == "Delete")
                                                {
                                                        templateRef.template.RemoveAt(i);
                                                        return;
                                                }
                                                Node headNode = CreateBranchFromTemplate(templateRef.template[i].node);
                                                if (headNode != null)
                                                {
                                                        editorRef.tree.tempChildren.Add(headNode);
                                                        UpdateNodePositions(headNode, (Vector2) mousePosition - headNode.rectPosition);
                                                }
                                                return;
                                        }
                                }
                        }
                        else if (CreateNode(nameType, editorRef.mousePosition - editorRef.gridOffset, out Node newNode))
                        {
                                children.Add(newNode);
                        }
                }

                private bool DeleteNode (List<Node> children, List<Node> tempChildren, Node nodeDelete)
                {
                        for (int i = children.Count - 1; i >= 0; i--)
                        {
                                if (children[i] == nodeDelete)
                                {
                                        if (children[i].CanHaveChildren())
                                                for (int n = 0; n < nodeDelete.Children().Count; n++)
                                                {
                                                        tempChildren.Add(nodeDelete.Children()[n]);
                                                }
                                        if (children[i] != null)
                                                Undo.DestroyObjectImmediate(children[i]);
                                        children.RemoveAt(i);
                                        return true;
                                }
                                if (children[i].CanHaveChildren() && children[i].Children() != null && children[i].Children().Count > 0)
                                {
                                        if (DeleteNode(children[i].Children(), tempChildren, nodeDelete))
                                        {
                                                return true;
                                        }
                                }
                        }
                        return false;
                }

                private bool DeleteBranch (List<Node> children, Node nodeDelete)
                {
                        for (int i = children.Count - 1; i >= 0; i--)
                        {
                                if (children[i] == nodeDelete)
                                {
                                        if (children[i] != null && children[i].CanHaveChildren() && children[i].Children().Count > 0)
                                        {
                                                DeleteAllInBranch(children[i].Children());
                                        }
                                        if (children[i] != null)
                                                Undo.DestroyObjectImmediate(children[i]);
                                        children.RemoveAt(i);
                                        return true;
                                }
                                if (children[i] != null && children[i].CanHaveChildren() && children[i].Children().Count > 0)
                                {
                                        if (DeleteBranch(children[i].Children(), nodeDelete))
                                        {
                                                return true;
                                        }
                                }
                        }
                        return false;
                }

                private void DeleteAllInBranch (List<Node> children)
                {
                        for (int i = children.Count - 1; i >= 0; i--)
                        {
                                if (children[i] != null && children[i].CanHaveChildren() && children[i].Children().Count > 0)
                                {
                                        DeleteAllInBranch(children[i].Children());
                                }
                                if (children[i] != null)
                                        Undo.DestroyObjectImmediate(children[i]);
                                children.RemoveAt(i);
                        }
                }

                private void CreateTemplate (TemplateNode template, Node node)
                {
                        template.nameType = node.nameType;
                        template.position = node.rectPosition;

                        if (node is Composite || node is Decorator)
                        {
                                List<Node> children = (node as Composite).children;
                                for (int i = 0; i < children.Count; i++)
                                {
                                        template.children.Add(new TemplateNode());
                                        CreateTemplate(template.children[i], children[i]);
                                }
                        }
                }

                private void CopyNodeChildren (Node parent, AITree tree)
                {
                        ResetMessagePosition(parent);
                        List<Node> children = parent.Children();
                        if (children == null)
                                return;

                        List<Node> newChildren = new List<Node>(children); // copy since we are going to clear original list
                        children.Clear(); //                                 clear list since content belongs to original node

                        for (int i = 0; i < newChildren.Count; i++)
                        {
                                Node child = newChildren[i];
                                if (CreatedAndCopyNode(child.nameType, child.rectPosition + Vector2.up * 40f, child, out Node copiedNode))
                                {
                                        copiedNode.SetParent(parent);
                                        children.Add(copiedNode);
                                        CopyNodeChildren(copiedNode, tree);
                                }
                        }
                }

                public bool CreateNode (string nameType, Vector2 position, out Node newNode)
                {
                        newNode = null;
                        if (editorRef == null || editorRef.tree == null)
                                return false;

                        if (EditorTools.RetrieveType("TwoBitMachines.FlareEngine.AI." + nameType, out Type type))
                        {
                                newNode = Undo.AddComponent(editorRef.tree.gameObject, type) as Node;
                                newNode.nameType = nameType;
                                newNode.rectActive = true;
                                newNode.rectPosition = position;
                                newNode.nodeType = newNode is Composite || newNode is Decorator || newNode is Root ? NodeType.Composite : NodeType.Action;
                                newNode.hideFlags = HideFlags.HideInInspector;
                                return true;
                        }
                        return false;
                }

                private bool CreatedAndCopyNode (string nameType, Vector2 position, Node nodeToCopy, out Node newNode)
                {
                        newNode = null;
                        if (editorRef == null || editorRef.tree == null)
                                return false;

                        if (EditorTools.RetrieveType("TwoBitMachines.FlareEngine.AI." + nameType, out Type type))
                        {
                                newNode = Undo.AddComponent(editorRef.tree.gameObject, type) as Node;
                                EditorTools.CopySerializedObject(new SerializedObject(newNode), new SerializedObject(nodeToCopy), type); // Undo.AddComponent (editorRef.tree.gameObject, type) as Node;
                                newNode.nameType = nameType;
                                newNode.rectActive = true;
                                newNode.rectPosition = position;
                                newNode.nodeType = newNode is Composite || newNode is Decorator || newNode is Root ? NodeType.Composite : NodeType.Action;
                                newNode.hideFlags = HideFlags.HideInInspector;
                                return true;
                        }
                        return false;
                }

                private Node CreateBranchFromTemplate (TemplateNode template)
                {
                        if (!CreateNode(template.nameType, template.position, out Node newNode))
                                return null;

                        for (int i = 0; i < template.children.Count; i++)
                        {
                                Node newChild = CreateBranchFromTemplate(template.children[i]);
                                if (newChild != null && (newNode is Composite || newNode is Decorator))
                                {
                                        (newNode as Composite).children.Add(newChild);
                                }
                        }
                        return newNode;
                }

                private void UpdateNodePositions (Node node, Vector2 distance)
                {
                        if (node == null)
                                return;

                        node.rectPosition += distance;
                        for (int i = 0; i < node.message.Count; i++)
                        {
                                node.message[i].position += distance;
                        }

                        if (node.Children() == null)
                                return;

                        for (int i = 0; i < node.Children().Count; i++)
                        {
                                UpdateNodePositions(node.Children()[i], distance);
                        }
                }

                private void SortChildren (List<Node> children)
                {
                        children.Sort((x, y) => x.rectPosition.x.CompareTo(y.rectPosition.x));

                        for (int i = 0; i < children.Count; i++)
                        {
                                if (children[i].Children() != null && children[i].Children().Count > 0)
                                {
                                        SortChildren(children[i].Children());
                                }
                        }
                }

                private void ResetMessagePosition (Node parent)
                {
                        for (int i = 0; i < parent.message.Count; i++)
                        {
                                parent.message[i].position.y += 40f;
                        }
                }
        }
}
