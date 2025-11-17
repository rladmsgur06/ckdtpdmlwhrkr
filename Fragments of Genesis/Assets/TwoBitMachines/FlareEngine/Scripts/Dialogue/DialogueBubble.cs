using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("Flare Engine/DialogueBubble")]
        public class DialogueBubble : MonoBehaviour
        {
                [SerializeField] public InputButtonSO exitButton;
                [SerializeField] public InputButtonSO skipButton;

                [SerializeField] public bool blockInventories;
                [SerializeField] public bool blockPlayerInput = true;
                [SerializeField] public List<Messenger> messengers = new List<Messenger>();
                [SerializeField] public List<Conversation> conversation = new List<Conversation>();

                [SerializeField] public SaveFloat saveFloat = new SaveFloat();
                [SerializeField] public List<string> conversationIndex = new List<string>();

                [SerializeField] public float fadeInTime = 0.25f;
                [SerializeField] public Tween tweenIn = Tween.EaseOut;

                [SerializeField] public float fadeOutTime = 0.15f;
                [SerializeField] public Tween tweenOut = Tween.EaseOut;

                [SerializeField] public UnityEvent onEnter = new UnityEvent();
                [SerializeField] public UnityEvent onExit = new UnityEvent();

                [System.NonSerialized] public Message message;
                [System.NonSerialized] private Messenger messenger;
                [System.NonSerialized] private int messageIndex = -1;
                [System.NonSerialized] private List<Message> messages;
                [System.NonSerialized] public static List<DialogueBubble> dialogues = new List<DialogueBubble>();

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

                public static void ResetDialogue ()
                {
                        for (int i = 0; i < dialogues.Count; i++)
                        {
                                dialogues[i]?.EndConversation();
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
                                if (messenger == null || messenger.bubble == null)
                                {
                                        EndConversation();
                                }
                                else if (exitButton != null && exitButton.Pressed())
                                {
                                        EndConversation();
                                }
                                else if (skipButton != null && skipButton.Released())
                                {
                                        NextMessage();
                                }
                                else if (Input.anyKeyDown || Input.touchCount > 0)
                                {
                                        if (messenger.bubble.messageLoadingComplete)
                                        {
                                                NextMessage();
                                        }
                                }
                        }
                }

                public void BeginConversation ()
                {
                        if (!gameObject.activeInHierarchy)
                        {
                                gameObject.SetActive(true);
                        }
                        if (conversation.Count > 0)
                        {
                                messenger = null;
                                onEnter.Invoke();
                                SetConversation(conversation[0]);
                        }
                }

                public void EndConversation ()
                {
                        if (messageIndex >= 0)
                        {
                                onExit.Invoke();
                        }
                        messenger = null;
                        messageIndex = -1;
                        TurnOffOtherBubbles();
                        if (blockPlayerInput)
                        {
                                ThePlayer.Player.BlockAllInputs(false);
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
                        if (blockPlayerInput)
                        {
                                ThePlayer.Player.BlockAllInputs(true);
                        }
                        messageIndex = 0;
                        messages = newConversation.messages;
                        SetDialogueUI(messages[messageIndex]);
                }

                private void SetDialogueUI (Message message)
                {
                        this.message = message;
                        messenger = GetMessenger(message.messenger);

                        if (messenger != null && messenger.bubble != null)
                        {
                                TurnOffOtherBubbles();
                                if (message.type == MessageType.Message)
                                {
                                        messenger.bubble.TransitionIn(message.message, fadeInTime, tweenIn, MessageDirection(message.messageTo), null, this);
                                }
                                else
                                {
                                        for (int i = 0; i < message.choice.Count; i++)
                                        {
                                                if (i < messenger.bubbles.Count && messenger.bubbles[i] != null)
                                                {
                                                        if (i == 0 && EventSystem.current != null)
                                                        {
                                                                EventSystem.current.SetSelectedGameObject(messenger.bubbles[i].gameObject);
                                                        }
                                                        Bubble bubble = messenger.bubbles[i];
                                                        Choice choice = message.choice[i];
                                                        bubble.TransitionIn(choice.choice, fadeInTime, tweenIn, MessageDirection(message.messageTo), choice, this);
                                                }
                                        }
                                }
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

                private void TurnOffOtherBubbles ()
                {
                        for (int i = 0; i < messengers.Count; i++)
                        {
                                if (messengers[i] != messenger && messengers[i].bubble != null)
                                {
                                        messengers[i].bubble.TransitionOut(fadeOutTime, tweenOut);
                                }
                                if (messengers[i] != messenger && messengers[i].bubbles != null)
                                {
                                        for (int j = 0; j < messengers[i].bubbles.Count; j++)
                                        {
                                                messengers[i].bubbles[j]?.TransitionOut(fadeOutTime, tweenOut);
                                        }
                                }
                        }
                }

                private float MessageDirection (string messageTo)
                {
                        if (messenger == null || messenger.bubble == null || !messenger.bubble.isDirectional)
                        {
                                return 1f;
                        }
                        for (int i = 0; i < messengers.Count; i++)
                        {
                                if (messengers[i].name == messageTo && messengers[i].bubble != null)
                                {
                                        return messenger.bubble.transform.parent.transform.position.x < messengers[i].bubble.transform.parent.transform.position.x ? 1f : -1f;
                                }
                        }
                        return 1f;
                }
        }
}
