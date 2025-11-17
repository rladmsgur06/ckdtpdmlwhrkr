using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [System.Serializable]
        public class ZipInfo
        {
                [SerializeField] public float yOffset;
                [SerializeField] public float zipSpeed = 1f;
                [SerializeField] public float jumpForce = 15f;

                [SerializeField] public bool canRelatch;
                [SerializeField] public bool exitButton;
                [SerializeField] public bool useGravity;

                [SerializeField] public string exit;
                [SerializeField] public string onEndWE;
                [SerializeField] public string onStartWE;

                [SerializeField] public UnityEventEffect onEnd;
                [SerializeField] public UnityEventEffect onStart;

                [System.NonSerialized] public int zipIndex;
                [System.NonSerialized] public float counter;
                [System.NonSerialized] public float cancelled;
                [System.NonSerialized] public float playerDirection;
                [System.NonSerialized] public float gravityMomentum;
                [System.NonSerialized] public bool exitMomentum;
                [System.NonSerialized] public UserInputs inputs;
                [System.NonSerialized] public Dictionary<int, PlankState> state = new Dictionary<int, PlankState>();

                public bool exitOnInput => exitButton && inputs != null && inputs.Holding(exit);

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool eventsFoldOut;
                [SerializeField, HideInInspector] public bool onStartFoldOut;
                [SerializeField, HideInInspector] public bool onEndFoldOut;
#pragma warning restore 0414
#endif
                #endregion

                public bool Active ()
                {
                        foreach (KeyValuePair<int, PlankState> zipState in state)
                        {
                                if (zipState.Value == PlankState.Latched)
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                public void Enter (AbilityManager player, int index)
                {
                        zipIndex = index;
                        exitMomentum = false;
                        gravityMomentum = 0;
                        state.Clear();
                        state.Add(index, PlankState.Latched);
                        OnStartEvent(player);
                }

                public void AutoSlideToCenter (int slopeUp, float slideSpeed, ref Vector2 velocity, out bool goingUp)
                {
                        goingUp = Compute.SameSign(slopeUp, playerDirection);

                        if (useGravity && !goingUp)
                        {
                                counter = velocity.x != 0 ? 1f : counter;
                                gravityMomentum = 0;
                                if (velocity.x == 0)
                                {
                                        float fullSpeed = slideSpeed * playerDirection;
                                        velocity.x = Compute.Lerp(fullSpeed * 0.1f, fullSpeed, 1f, ref counter);
                                        gravityMomentum = velocity.x;
                                }
                                return;
                        }
                        else if (useGravity)
                        {
                                gravityMomentum = 0;
                                counter = 0;
                        }
                }

                public void OnStartEvent (AbilityManager player)
                {
                        onStart.Invoke(ImpactPacket.impact.Set(onStartWE, player.world.transform, player.world.boxCollider, player.world.transform.position, null, player.world.box.up, player.playerDirection, 0));
                }

                public void OnEndEvent (AbilityManager player)
                {
                        onEnd.Invoke(ImpactPacket.impact.Set(onEndWE, player.world.transform, player.world.boxCollider, player.world.transform.position, null, player.world.box.up, player.playerDirection, 0));
                }

        }

}
