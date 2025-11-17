using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(DialogueBubble))]
        public class DialogueBubbleEditor : UnityEditor.Editor
        {
                private DialogueBubble main;
                private SerializedObject parent;
                public static string inputName = "Name";
                public static string inputName2 = "Name";

                private void OnEnable ()
                {
                        main = target as DialogueBubble;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();
                        {
                                Settings();
                                Messengers();
                                Conversation();
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

                public void Settings ()
                {
                        if (FoldOut.Bar(parent).Label("Settings").FoldOut("settingsFoldOut"))
                        {
                                FoldOut.Box(4, FoldOut.boxColor, offsetY: -2);
                                {
                                        parent.Field("Exit Button", "exitButton");
                                        parent.Field("Skip Button", "skipButton");
                                        parent.FieldToggle("Block Inventories", "blockInventories");
                                        parent.FieldToggle("Block Player Input", "blockPlayerInput");
                                }
                                Layout.VerticalSpacing(3);

                                FoldOut.Box(2, FoldOut.boxColor, extraHeight: 3);
                                {
                                        parent.FieldDouble("Fade In Time", "fadeInTime", "tweenIn");
                                        parent.FieldDouble("Fade Out Time", "fadeOutTime", "tweenOut");
                                }

                                if (FoldOut.FoldOutButton(parent.Get("eventsFoldOut")))
                                {
                                        Fields.EventFoldOut(parent.Get("onEnter"), parent.Get("onEnterFoldOut"), "On Begin", color: FoldOut.boxColorLight);
                                        Fields.EventFoldOut(parent.Get("onExit"), parent.Get("onExitFoldOut"), "On Exit", color: FoldOut.boxColorLight);
                                }
                        }
                }

                public void Messengers ()
                {
                        if (FoldOut.Bar(parent).Label("Messengers").BR("addMessenger", execute: parent.Bool("messengerFoldOut")).FoldOut("messengerFoldOut"))
                        {
                                SerializedProperty messengers = parent.Get("messengers");

                                if (parent.ReadBool("addMessenger"))
                                {
                                        messengers.arraySize++;
                                }

                                for (int i = 0; i < messengers.arraySize; i++)
                                {
                                        SerializedProperty element = messengers.Element(i);

                                        if (FoldOut.Bar(element, FoldOut.boxColor, 45).BR("foldOut", "Reopen").BBR("Delete"))
                                        {
                                                messengers.DeleteArrayElement(i);
                                                break;
                                        }
                                        ListReorder.Grip(parent, messengers, Bar.barStart.CenterRectHeight(), i, Tint.WarmWhite);
                                        Rect rect = Bar.barStart;
                                        Bar.BarField(element, "icon", 0.18f);
                                        Bar.BarField(element, "name", 1f);

                                        Sprite icon = GetMessengerIcon(element.String("name"));
                                        Texture texture = AssetPreview.GetAssetPreview(icon as Sprite);
                                        texture = texture == null ? Texture2D.whiteTexture : texture;
                                        GUI.DrawTexture(new Rect(rect) { width = 20, x = 27 }, texture, ScaleMode.StretchToFill);

                                        if (element.Bool("foldOut"))
                                        {
                                                SerializedProperty bubbles = element.Get("bubbles");
                                                if (bubbles.arraySize == 0)
                                                {
                                                        bubbles.arraySize++;
                                                }

                                                FoldOut.Box(1 + bubbles.arraySize, FoldOut.boxColor, offsetY: -2);
                                                {
                                                        element.Field("Bubble", "bubble");

                                                        for (int j = 0; j < bubbles.arraySize; j++)
                                                        {
                                                                Fields.ArrayProperty(bubbles, bubbles.Element(j), j, "Choice");
                                                        }
                                                }
                                                Layout.VerticalSpacing(3);
                                        }

                                }
                        }
                }

                public void Conversation ()
                {
                        SerializedProperty conversation = parent.Get("conversation");
                        string[] messengers = GetMessengers();
                        string[] branches = GetBranches();

                        if (!FoldOut.Bar(parent)
                                .Label("Conversations")
                                .RightButton("addConversation", execute: parent.Bool("conversationFoldOut"), toolTip: "Add Conversation")
                                .FoldOut("conversationFoldOut")
                        )
                        {
                                return;
                        }
                        if (parent.ReadBool("addConversation"))
                        {
                                conversation.arraySize++;
                                conversation.LastElement().Get("name").stringValue = inputName2;
                                conversation.LastElement().Get("messages").arraySize = 0;
                        }

                        for (int i = 0; i < conversation.arraySize; i++)
                        {
                                SerializedProperty branch = conversation.Element(i);
                                SerializedProperty messages = branch.Get("messages");

                                FoldOut.Bar(branch, Tint.Orange, 25)
                                        .LabelAndEdit("name", "editName")
                                        .RightButton(toolTip: "Add Message")
                                        .ToggleButton("isRandom", "CheckMark", "X", toolTip: "Is Random")
                                        .RightButton("delete", "Delete");
                                ListReorder.Grip(parent, conversation, Bar.barStart.CenterRectHeight(), i, Tint.White, "signalIndex2", "active2");

                                if (Bar.FoldOpen(branch.Get("foldOut")))
                                {
                                        for (int j = 0; j < messages.arraySize; j++)
                                        {
                                                SerializedProperty message = messages.Element(j);
                                                SerializedProperty type = message.Get("type");
                                                Color color = type.enumValueIndex == 1 ? Tint.Delete : Tint.BoxTwo;
                                                float buttons = type.enumValueIndex == 1 ? 4 : 3;
                                                int width = (int) ((Layout.labelWidth + Layout.contentWidth - 42f - Layout.buttonWidth * buttons) * 0.5f);

                                                if (
                                                        FoldOut.Bar(message, color, 45)
                                                        .LDL("messenger", width, messengers)
                                                        .LF("type", width)
                                                        .RightButton("addChoice", "Add", toolTip: "Add Choice", execute: buttons == 4)
                                                        .RightButton("open", "Event", toolTip: "Message")
                                                        .RightButton("options", "Reopen", toolTip: "Options")
                                                        .BBR("Delete")
                                                )
                                                {
                                                        messages.DeleteArrayElement(j);
                                                        break;
                                                }

                                                Rect rect = Bar.barStart;
                                                ListReorder.Grip(branch, messages, rect.CenterRectHeight(), j, Tint.Grey);
                                                Sprite icon = GetMessengerIcon(message.String("messenger"));
                                                Texture texture = AssetPreview.GetAssetPreview(icon as Sprite);
                                                texture = texture == null ? Texture2D.whiteTexture : texture;
                                                GUI.DrawTexture(new Rect(rect) { width = 20, x = 27 }, texture, ScaleMode.StretchToFill);

                                                if (message.Bool("options"))
                                                {
                                                        FoldOut.Box(1, color, offsetY: -2);
                                                        {
                                                                message.DropDownList(messengers, "Message To", "messageTo");
                                                        }
                                                        Layout.VerticalSpacing(1);
                                                        Fields.EventFoldOut(message.Get("onMessage"), message.Get("eventFoldOut"), "On Message Complete", offsetY: -1, color: color);
                                                }
                                                if (message.Bool("open"))
                                                {
                                                        DisplayDialogueBox(message, type, branches, message.ReadBool("addChoice"));
                                                }
                                        }
                                        if (messages.arraySize == 0)
                                        {
                                                Layout.VerticalSpacing(5);
                                        }
                                }

                                if (branch.ReadBool("add"))
                                {
                                        messages.arraySize++;
                                        messages.LastElement().Get("type").intValue = 0;
                                        messages.LastElement().Get("messenger").stringValue = "";
                                        messages.LastElement().Get("message").stringValue = "";
                                        messages.LastElement().Get("choice").arraySize = 0;
                                }

                                if (branch.ReadBool("delete"))
                                {
                                        conversation.DeleteArrayElement(i);
                                        break;
                                }
                        }
                }

                private void DisplayDialogueBox (SerializedProperty message, SerializedProperty type, string[] branches, bool addChoice)
                {
                        if (type.enumValueIndex == 1)
                        {
                                SerializedProperty choices = message.Get("choice");

                                if (choices.arraySize == 0)
                                        choices.arraySize++;

                                for (int i = 0; i < choices.arraySize; i++)
                                {
                                        SerializedProperty choice = choices.Element(i);
                                        SerializedProperty messageString = choice.Get("choice");
                                        Rect rect = Layout.CreateRect(width: Layout.longInfoWidth - 10, height: 33, offsetX: -1);
                                        messageString.stringValue = GUI.TextArea(rect, messageString.stringValue);

                                        FoldOut.FoldOutLeftButton(choice.Get("options"), Tint.Delete, offsetX: -1, offsetY: 1, height: 31);

                                        if (choice.Bool("options"))
                                        {
                                                FoldOut.BoxSingle(1, Tint.Delete, extraHeight: 2, offsetY: -1);
                                                choice.DropDownListAndButton(branches, "Branch To", "branchTo", "delete", "Delete");
                                                Fields.EventFoldOut(choice.Get("onChoice"), choice.Get("eventFoldOut"), "On Choice", color: Tint.Delete);
                                        }
                                        if (choice.ReadBool("delete"))
                                        {
                                                choices.DeleteArrayElement(i);
                                                break;
                                        }
                                }
                                if (addChoice)
                                {
                                        choices.arraySize++;
                                }
                        }
                        else
                        {
                                GUIStyle style = new GUIStyle(EditorStyles.textArea);

                                style.wordWrap = true;

                                Layout.VerticalSpacing(1);
                                SerializedProperty messageString = message.Get("message");
                                float widthDiff = Layout.longInfoWidth > 260 ? Layout.longInfoWidth - 260 : 0;
                                float increaseHeight = widthDiff * 0.75f;
                                Rect rect = Layout.CreateRect(width: Layout.longInfoWidth, height: 60 + increaseHeight, offsetX: -11);
                                messageString.stringValue = EditorGUI.TextArea(rect, messageString.stringValue, style);
                        }
                }

                public string[] GetMessengers ()
                {
                        string[] names = new string[main.messengers.Count];
                        for (int i = 0; i < main.messengers.Count; i++)
                        {
                                names[i] = main.messengers[i].name;
                        }
                        return names;
                }

                public string[] GetBranches ()
                {
                        string[] names = new string[main.conversation.Count];
                        for (int i = 0; i < main.conversation.Count; i++)
                        {
                                names[i] = main.conversation[i].name;
                        }
                        return names;
                }

                public Sprite GetMessengerIcon (string name)
                {
                        for (int i = 0; i < main.messengers.Count; i++)
                        {
                                if (name == main.messengers[i].name)
                                {
                                        return main.messengers[i].icon;
                                }
                        }
                        return default(Sprite);
                }
        }
}
