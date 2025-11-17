using System.Collections.Generic;
using TwoBitMachines.FlareEngine.AI;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("Flare Engine/AIFSM")]
        public partial class AIFSM : AIBase
        {
                [SerializeField] public List<AIState> state = new List<AIState>();
                [SerializeField] public List<AIState> alwaysState = new List<AIState>();
                [SerializeField] public AIState resetState = new AIState();
                [SerializeField] public bool resetToFirst;

                [System.NonSerialized] public AIState currentState;
                [System.NonSerialized] public AIState previousState;

                public override void Initialize ()
                {
                        if (state.Count > 0)
                        {
                                currentState = state[0];
                        }
                }

                public override void Execute ()
                {
                        if (root == null || root.pause)
                        {
                                return;
                        }

                        SlowDown();
                        velocity.x = externalVelocity.x;
                        damage.PauseTimer();
                        ApplyGravity();
                        RunStates();
                        Collision(velocity, ref velocity.y);
                        SlowDownReset();
                }

                private void RunStates ()
                {
                        Vector2 position = transform.position; // set reference to beginning position

                        if (currentState != null && root.RunFSM(currentState, position, ref velocity, ref signals.characterDirection, out string nextState, true))
                        {
                                ChangeState(nextState);
                        }
                        for (int i = 0; i < alwaysState.Count; i++)
                        {
                                if (alwaysState[i].enabled)
                                {
                                        root.RunFSM(alwaysState[i], position, ref velocity, ref signals.characterDirection, out string none, false);
                                }
                        }
                }

                public override void ChangeState (string stateName)
                {
                        for (int i = 0; i < state.Count; i++)
                        {
                                if (state[i].stateName == stateName)
                                {
                                        if (currentState != null && currentState.stateName != stateName)
                                        {
                                                root.previousStateName = currentState.stateName;
                                        }
                                        currentState?.Reset();
                                        state[i].Reset();
                                        currentState = state[i];
                                        return;
                                }
                        }
                }

                public override void ChangeStateCheckInterrupt (string stateName)
                {
                        if (currentState != null && currentState.cantInterrupt)
                        {
                                return;
                        }

                        for (int i = 0; i < state.Count; i++)
                        {
                                if (state[i].stateName == stateName)
                                {
                                        if (currentState != null && currentState.stateName != stateName)
                                        {
                                                root.previousStateName = currentState.stateName;

                                        }
                                        currentState?.Reset();
                                        state[i].Reset();
                                        currentState = state[i];
                                        return;
                                }
                        }
                }

                public void ChangeStateSkipReset (string stateName)
                {
                        for (int i = 0; i < state.Count; i++) // change state
                        {
                                if (state[i].stateName == stateName)
                                {
                                        if (currentState != null && currentState.stateName != stateName)
                                        {
                                                root.previousStateName = currentState.stateName;
                                        }
                                        currentState?.ResetSkip();
                                        state[i].ResetSkip();
                                        currentState = state[i];
                                        return;
                                }
                        }
                }

                public override void ResetAI ()
                {
                        root.selectInterrupt = false;
                        root.previousStateName = "";
                        if (resetToActive && !gameObject.activeInHierarchy)
                        {
                                gameObject.SetActive(true);
                        }
                        if (currentState != null)
                        {
                                if (currentState.HardReset() && state.Count > 0)
                                {
                                        currentState = state[0]; // force reset, some ai nodes need to reset
                                }
                        }

                        if (resetState.action.Count == 0)
                        {
                                if (resetToFirst && state.Count > 0)
                                {
                                        currentState?.Reset();
                                        currentState = state[0];
                                }
                                return;
                        }

                        currentState?.Reset();
                        velocity = Vector2.zero;
                        root.RunFSM(resetState, transform.position, ref velocity, ref signals.characterDirection, out string none, true);
                        transform.position += (Vector3) velocity * Time.deltaTime;

                        if (state.Count > 0)
                        {
                                currentState = state[0];
                        }
                        for (int i = 0; i < alwaysState.Count; i++)
                        {
                                alwaysState[i].Reset();
                        }
                }
        }

        [System.Serializable]
        public class AIState
        {
                [SerializeField] public List<StateNode> action = new List<StateNode>();
                [SerializeField] public string stateName = "State Name";
                [SerializeField] public string defaultSignal = "Signal";
                [SerializeField] public bool cantInterrupt = false;
                [SerializeField] public bool useSignal = false;
                [SerializeField] public bool enabled = true; // for always state only
                [SerializeField] public int childIndex = 0;
                [SerializeField] public AIStateType type;

                public void Reset ()
                {
                        childIndex = 0;
                        for (int i = 0; i < action.Count; i++)
                        {
                                action[i].node.Reset(false, true);
                        }
                }

                public void ResetSkip ()
                {
                        childIndex = 0;
                        for (int i = 0; i < action.Count; i++)
                        {
                                action[i].node.Reset(true, true);
                        }
                }

                public bool HardReset ()
                {
                        childIndex = 0;
                        bool reset = false;
                        for (int i = 0; i < action.Count; i++)
                        {
                                if (action[i].node.HardReset())
                                {
                                        reset = true;
                                }
                        }
                        return reset;
                }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀ 
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool showStateExtra;
                [SerializeField, HideInInspector] private bool editName;
                [SerializeField, HideInInspector] private bool signal;
                [SerializeField, HideInInspector] private bool changeState;
                [SerializeField, HideInInspector] private bool deleteAsk;
                [SerializeField, HideInInspector] private bool foldOut;
                [SerializeField, HideInInspector] private bool active;
                [SerializeField, HideInInspector] private bool delete;
                [SerializeField, HideInInspector] private bool add;
                [SerializeField, HideInInspector] private bool menu = true;
                [SerializeField, HideInInspector] private int signalIndex = -1;

                public void HideInInspector ()
                {
                        for (int i = 0; i < action.Count; i++)
                        {
                                action[i].node.hideFlags = HideFlags.HideInInspector;
                        }
                }

                public void SetActiveFalse ()
                {
                        active = false;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        [System.Serializable]
        public class StateNode
        {
                [SerializeField, HideInInspector] public Node node;
                [SerializeField] public string onSuccess;
                [SerializeField] public string onFailure;
                [SerializeField] public bool goToSuccess;
                [SerializeField] public bool goToFailure;

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

        public enum AIStateType
        {
                Parallel,
                Sequence,
                SequenceSucceed
        }

}
