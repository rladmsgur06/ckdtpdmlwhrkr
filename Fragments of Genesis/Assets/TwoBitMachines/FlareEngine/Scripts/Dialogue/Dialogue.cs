using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("Flare Engine/Dialogue")]
        public class Dialogue : MonoBehaviour
        {
                [SerializeField] public DialogueUI dialogueUI;
                [SerializeField] public InputButtonSO exitButton;
                [SerializeField] public InputButtonSO skipButton;
                [SerializeField] public bool isRandom;
                [SerializeField] public bool positionIcon;
                [SerializeField] public bool blockInventories;
                [SerializeField] public List<Messenger> messengers = new List<Messenger>();
                [SerializeField] public List<Conversation> conversation = new List<Conversation>();

                [SerializeField] public bool rememberState;
                [SerializeField] public string saveKey;
                [SerializeField] public SaveFloat saveFloat = new SaveFloat();
                [SerializeField] public List<string> conversationIndex = new List<string>();
                [SerializeField] public UnityEvent onEnter = new UnityEvent();
                [SerializeField] public UnityEvent onExit = new UnityEvent();

                [System.NonSerialized] private MessengerAnimation animationM;
                [System.NonSerialized] private int messageIndex = -1;
                [System.NonSerialized] private List<Message> messages;
                [System.NonSerialized] public static List<Dialogue> dialogues = new List<Dialogue>();

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool active;
                [SerializeField, HideInInspector] private bool active2;
                [SerializeField, HideInInspector] private bool addState;
                [SerializeField, HideInInspector] private bool eventsFoldOut;
                [SerializeField, HideInInspector] private bool onEnterFoldOut;
                [SerializeField, HideInInspector] private bool onExitFoldOut;
                [SerializeField, HideInInspector] private bool settingsFoldOut;
                [SerializeField, HideInInspector] private bool messengerFoldOut;
                [SerializeField, HideInInspector] private bool conversationFoldOut;
                [SerializeField, HideInInspector] private bool rememberFoldOut;
                [SerializeField, HideInInspector] private bool addMessenger;
                [SerializeField, HideInInspector] private bool addConversation;
                [SerializeField, HideInInspector] private int signalIndex = -1;
                [SerializeField, HideInInspector] private int signalIndex2 = -1;
                [SerializeField, HideInInspector] private string currentNodeID = "root";
                [SerializeField, HideInInspector] public string branchName = "Conversation";
                [SerializeField, HideInInspector] public string animationName = "Talking";
#pragma warning restore 0414
#endif
                #endregion

                private void Awake ()
                {
                        WorldManager.RegisterInput(skipButton);
                        WorldManager.RegisterInput(exitButton);

                }
                private void Start ()
                {
                        if (rememberState)
                        {
                                saveFloat = Storage.Load<SaveFloat>(saveFloat, WorldManager.saveFolder, saveKey);
                        }
                }
                private void OnEnable ()
                {
                        if (!dialogues.Contains(this))
                        {
                                dialogues.Add(this);
                        }
                }

                private void OnDisable ()
                {
                        if (dialogues.Contains(this))
                        {
                                dialogues.Remove(this);
                        }
                }

                public static void ResetDialogue (bool hardReset)
                {
                        for (int i = 0; i < dialogues.Count; i++)
                        {
                                if (dialogues[i] != null)
                                {
                                        dialogues[i].EndConversation(hardReset);
                                }
                        }
                }

                public void SaveConversationState ()
                {
                        if (rememberState)
                        {
                                saveFloat.value = Mathf.Clamp(saveFloat.value + 1f, 0, conversationIndex.Count);
                                Storage.Save(saveFloat, WorldManager.saveFolder, saveKey);
                        }
                }
                private void Update ()
                {
                        if (messageIndex >= 0) // conversation is active
                        {
                                if (blockInventories)
                                {
                                        Inventory.blockInventories = true;
                                }
                                if (exitButton != null && exitButton.Pressed())
                                {
                                        EndConversation();
                                }
                                else if (skipButton != null && skipButton.Released())
                                {
                                        NextMessage();
                                }
                                else if (Input.anyKeyDown || Input.touchCount > 0)
                                {
                                        if (dialogueUI != null && !dialogueUI.isLoading)
                                        {
                                                NextMessage();
                                        }
                                }
                                if (animationM != null && dialogueUI != null && dialogueUI.canAnimate)
                                {
                                        animationM.Update(dialogueUI.icon);
                                }
                        }
                }

                public void BeginConversation ()
                {
                        if (dialogueUI == null)
                        {
                                return;
                        }
                        if (!gameObject.activeInHierarchy)
                        {
                                gameObject.SetActive(true);
                        }
                        if (isRandom)
                        {
                                int randomCount = 0;
                                for (int i = 0; i < conversation.Count; i++)
                                {
                                        randomCount = conversation[i].isRandom ? randomCount + 1 : randomCount;
                                }
                                int chosenIndex = Random.Range(0, randomCount);
                                int memIndex = 0;
                                for (int i = 0; i < conversation.Count; i++)
                                {
                                        if (conversation[i].isRandom && memIndex++ >= chosenIndex)
                                        {
                                                onEnter.Invoke();
                                                dialogueUI.StartConversation();
                                                SetConversation(conversation[i]);
                                                return;
                                        }
                                }
                        }

                        if (rememberState && conversationIndex.Count > 0)
                        {
                                for (int i = 0; i < conversation.Count; i++)
                                {
                                        if (conversation[i].name == conversationIndex[(int) saveFloat.value])
                                        {
                                                onEnter.Invoke();
                                                dialogueUI.StartConversation();
                                                SetConversation(conversation[i]);
                                        }
                                }
                        }
                        else if (conversation.Count > 0)
                        {
                                onEnter.Invoke();
                                dialogueUI.StartConversation();
                                SetConversation(conversation[0]);
                        }
                }

                public void EndConversation (bool gameReset = false)
                {
                        if (messageIndex >= 0)
                        {
                                onExit.Invoke();
                        }
                        messageIndex = -1;
                        ThePlayer.Player.BlockAllInputs(false);
                        if (dialogueUI != null)
                        {
                                dialogueUI.EndConversation(gameReset);
                        }
                }

                public void EndConversation ()
                {
                        if (messageIndex >= 0)
                        {
                                onExit.Invoke();
                        }
                        messageIndex = -1;
                        ThePlayer.Player.BlockAllInputs(false);
                        if (dialogueUI != null)
                        {
                                dialogueUI.EndConversation(false);
                        }
                }

                public void NextMessage ()
                {
                        if (messages == null)
                        {
                                EndConversation();
                                return;
                        }
                        if (messageIndex < messages.Count && messages[messageIndex].type == MessageType.Choice)
                        {
                                return;
                        }
                        if (++messageIndex < messages.Count)
                        {
                                SetDialogueUI(messages[messageIndex]);
                                return;
                        }
                        EndConversation();
                }

                public void NextConversation (string branchTo)
                {
                        for (int i = 0; i < conversation.Count; i++)
                        {
                                if (conversation[i].name == branchTo)
                                {
                                        SetConversation(conversation[i]);
                                        return;
                                }
                        }
                        EndConversation();
                }

                private void SetConversation (Conversation newConversation)
                {
                        if (newConversation.messages.Count == 0)
                        {
                                EndConversation();
                                return;
                        }

                        messageIndex = 0;
                        ThePlayer.Player.BlockAllInputs(true);
                        messages = newConversation.messages;
                        SetDialogueUI(messages[messageIndex]);
                }

                private void SetDialogueUI (Message message)
                {
                        message.SetReference(this);
                        Messenger messenger = GetMessenger(message.messenger);

                        if (messenger != null && dialogueUI != null)
                        {
                                MessagePosition messagePosition = IconPosition(messenger, message.messageTo);
                                animationM = messenger.GetAnimation(message);
                                if (animationM != null)
                                        animationM.Reset();
                                Sprite icon = animationM != null && animationM.sprites.Count > 0 ? animationM.sprites[0] : messenger.icon;
                                dialogueUI.StartMessage(message.type, icon, messenger.background, messenger.name, message.message, message.choice, messagePosition, message.onMessage);
                        }
                }

                private Messenger GetMessenger (string name)
                {
                        for (int i = 0; i < messengers.Count; i++)
                        {
                                if (messengers[i].name == name)
                                {
                                        return messengers[i];
                                }
                        }
                        return null;
                }

                private MessagePosition IconPosition (Messenger messenger, string messageTo)
                {
                        if (!positionIcon)
                        {
                                return MessagePosition.Ignore;
                        }
                        if (messenger == null || messenger.transform == null)
                        {
                                return MessagePosition.Left;
                        }
                        for (int i = 0; i < messengers.Count; i++)
                        {
                                if (messengers[i].name == messageTo)
                                {
                                        return messenger.transform.position.x < messengers[i].transform.position.x ? MessagePosition.Left : MessagePosition.Right;
                                }
                        }
                        return MessagePosition.Left;
                }

        }

        [System.Serializable]
        public class Conversation
        {
                [SerializeField] public string name;
                [SerializeField] public bool isRandom = false;
                [SerializeField] public List<Message> messages = new List<Message>();

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool settingsFoldOut;
                [SerializeField, HideInInspector] private bool add;
                [SerializeField, HideInInspector] private bool delete;
                [SerializeField, HideInInspector] private bool deleteAsk;
                [SerializeField, HideInInspector] private bool foldOut;
                [SerializeField, HideInInspector] private bool active;
                [SerializeField, HideInInspector] private bool editName;
                [SerializeField, HideInInspector] private int signalIndex = -1;
#pragma warning restore 0414
#endif
                #endregion
        }

        [System.Serializable]
        public class Message
        {
                [SerializeField] public MessageType type;
                [SerializeField] public string messenger;
                [SerializeField] public string messageTo;
                [SerializeField] public string message;
                [SerializeField] public string animation;
                [SerializeField] public bool useAnimation;
                [SerializeField] public UnityEvent onMessage;
                [SerializeField] public List<Choice> choice = new List<Choice>();

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool open;
                [SerializeField, HideInInspector] private bool options;
                [SerializeField, HideInInspector] private bool addChoice;
                [SerializeField, HideInInspector] private bool eventFoldOut;
#pragma warning restore 0414
#endif
                #endregion

                public void SetReference (Dialogue dialogue)
                {
                        for (int i = 0; i < choice.Count; i++)
                        {
                                choice[i].dialogue = dialogue;
                        }
                }
        }

        [System.Serializable]
        public class Messenger
        {
                [SerializeField] public string name;
                [SerializeField] public Sprite icon;
                [SerializeField] public Bubble bubble;
                [SerializeField] public Sprite background;
                [SerializeField] public Transform transform;
                [SerializeField] public List<Bubble> bubbles = new List<Bubble>();
                [SerializeField] public List<MessengerAnimation> animation = new List<MessengerAnimation>();

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool foldOut;
                [SerializeField, HideInInspector] private bool addAnimation;
#pragma warning restore 0414
#endif
                #endregion

                public MessengerAnimation GetAnimation (Message message)
                {
                        if (!message.useAnimation)
                        {
                                return null;
                        }
                        for (int i = 0; i < animation.Count; i++)
                        {
                                if (animation[i].name == message.animation)
                                        return animation[i];
                        }
                        return null;
                }
        }

        [System.Serializable]
        public class MessengerAnimation
        {
                [SerializeField] public List<Sprite> sprites = new List<Sprite>();
                [SerializeField] public float fps = 10f;
                [SerializeField] public bool loop;
                [SerializeField] public string name = "";

                [System.NonSerialized] private float counter = 0;
                [System.NonSerialized] private int frameIndex = 0;
                [System.NonSerialized] private bool block = false;

                public void Reset ()
                {
                        counter = 0;
                        frameIndex = 0;
                        fps = fps <= 0 ? 10f : fps;
                        block = false;
                }

                public void Update (Image image)
                {
                        if (block)
                                return;
                        float rate = 1f / fps;

                        if (Clock.Timer(ref counter, rate))
                        {
                                frameIndex = frameIndex + 1 >= sprites.Count ? 0 : frameIndex + 1;
                                image.sprite = sprites[frameIndex];
                                if (!loop && frameIndex == 0)
                                        block = true;
                        }
                }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool foldOut;
                [SerializeField, HideInInspector] private bool deleteAsk;
                [SerializeField, HideInInspector] private bool delete;
                [SerializeField, HideInInspector] private bool add;
#pragma warning restore 0414
#endif
                #endregion
        }

        [System.Serializable]
        public class Choice
        {
                [SerializeField] public string choice;
                [SerializeField] public string branchTo;
                [SerializeField] public UnityEvent onChoice;
                [System.NonSerialized] public Dialogue dialogue;
                [System.NonSerialized] public DialogueBubble dialogueBubble;
                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool delete;
                [SerializeField, HideInInspector] private bool options;
                [SerializeField, HideInInspector] private bool eventFoldOut;
#pragma warning restore 0414
#endif
                #endregion

                public void ChoiceSelected ()
                {
                        if (onChoice != null)
                                onChoice.Invoke();
                        if (dialogue != null)
                                dialogue.NextConversation(branchTo);
                        if (dialogueBubble != null)
                                dialogueBubble.NextConversation(branchTo);
                }
        }

        public enum MessageType
        {
                Message,
                Choice
        }

        public enum MessagePosition
        {
                Right,
                Left,
                Ignore
        }
}
