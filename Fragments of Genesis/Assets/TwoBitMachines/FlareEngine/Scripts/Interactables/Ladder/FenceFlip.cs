using TwoBitMachines.FlareEngine.ThePlayer;
using TwoBitMachines.TwoBitSprite;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [System.Serializable]
        public class FenceFlip // animation signals: fenceFlip, fenceFlipReverse
        {
                [SerializeField] public bool canFlip;
                [SerializeField] public float flipTime = 2f;
                [SerializeField] public SpriteEngine spriteEngine;

                [System.NonSerialized] private State state;
                [System.NonSerialized] private float radius;
                [System.NonSerialized] private float direction;
                [System.NonSerialized] private float halfCounter;
                [System.NonSerialized] private bool isFlipping;
                [System.NonSerialized] private bool flipReverse;
                [System.NonSerialized] private Vector2 startPoint;

                private enum State { FirstHalf, SecondHalf }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField, HideInInspector] private bool fenceFoldOut;
                #pragma warning restore 0414
                #endif
                #endregion

                public void Reset ( )
                {
                        halfCounter = 0;
                        isFlipping = false;
                        state = State.FirstHalf;
                }

                public bool Flip (LadderInstance ladder, LadderClimb ladderClimb, AbilityManager player, string flipButton, ref Vector2 velocity)
                {
                        if (!canFlip || spriteEngine == null)
                        {
                                return false;
                        }

                        if (!isFlipping && player.inputs.Pressed (flipButton))
                        {
                                flipReverse = ladderClimb.hasFlipped;
                                startPoint = player.world.transform.position;
                                float distance = ladder.CenterX ( ) - startPoint.x;
                                direction = Mathf.Sign (distance);
                                radius = Mathf.Abs (distance);
                                state = State.FirstHalf;
                                isFlipping = true;
                        }
                        if (isFlipping)
                        {
                                velocity = Vector2.zero;
                                SetSignals (player);

                                if (state == State.FirstHalf && Switch (ladder, player, 0))
                                {
                                        state = State.SecondHalf;
                                        ladderClimb.hasFlipped = !ladderClimb.hasFlipped;
                                        startPoint = player.world.transform.position;
                                        ladderClimb.fenceReverse.SetLayerOrder (player, ladder, ladderClimb.hasFlipped);
                                }
                                if (state == State.SecondHalf && Switch (ladder, player, direction * radius))
                                {
                                        isFlipping = false;
                                }
                        }

                        spriteEngine.SetSignal ("idle");
                        return isFlipping;
                }

                private bool Switch (LadderInstance ladder, AbilityManager player, float offset)
                {
                        Vector3 position = player.world.transform.position;
                        bool complete = Clock.Timer (ref halfCounter, flipTime * 0.5f);
                        float target = ladder.CenterX ( ) + offset;
                        float percent = halfCounter / (flipTime * 0.5f);
                        position.x = complete ? position.x : Mathf.Lerp (startPoint.x, target, percent);
                        player.world.transform.position = position;
                        return complete;
                }

                private void SetSignals (AbilityManager player)
                {
                        player.signals.Set ("fenceFlipping");
                        spriteEngine.SetSignal ("fenceFlipping");

                        player.signals.Set ("fenceFlip", !flipReverse);
                        player.signals.Set ("fenceFlipReverse", flipReverse);
                        if (flipReverse)
                        {
                                spriteEngine.SetSignal ("fenceFlipReverse");
                        }
                        else
                        {
                                spriteEngine.SetSignal ("fenceFlip");
                        }
                }
        }
}