using System.Collections.Generic;
using TwoBitMachines.FlareEngine.ThePlayer;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class AnimationSignals
        {
                // signals must be cleared every frame
                [SerializeField] public TwoBitSprite.SpriteEngineBase spriteEngine;
                [System.NonSerialized] public Dictionary<string, bool> signals = new Dictionary<string, bool>(); // for external use, in case player does not use sprite engine
                [System.NonSerialized] public int characterDirection = 1;
                [System.NonSerialized] public int oldCharacterDirection = 1;
                [System.NonSerialized] public int forceDirection = 0;
                [System.NonSerialized] public bool movingX = false;
                [System.NonSerialized] public Vector2 forcedVelocity;

                public void InitializeToSpriteEngine (Transform transform)
                {
                        if (spriteEngine == null)
                        {
                                spriteEngine = transform.GetComponent<TwoBitSprite.SpriteEngineBase>();
                        }
                        if (spriteEngine != null)
                        {
                                spriteEngine.SetSignals(signals);
                        }
                        characterDirection = oldCharacterDirection = 1;
                }

                public void InitializeToPlayer (Transform transform)
                {
                        Player player = transform.GetComponent<Player>();
                        if (player != null)
                        {
                                signals = player.signals.signals;
                        }
                        characterDirection = oldCharacterDirection = 1;
                }

                public void ClearSignals ()
                {
                        signals.Clear();
                }

                public void SetDirection (int direction)
                {
                        oldCharacterDirection = direction;
                        characterDirection = direction;
                }

                public void ForceDirection (int direction)
                {
                        forceDirection = direction;
                }

                public void ForceVelocity (Vector2 velocity)
                {
                        forcedVelocity = velocity;
                }

                public void Set (string signal, bool value = true)
                {
                        signals[signal] = value; // will not throw an exception if you set a signal that does not exist
                }

                public void SetSignals (Vector2 velocity, bool onGround, bool onWallStop)
                {
                        velocity.x = onWallStop && Mathf.Abs(velocity.x) < 0.001f ? 0 : velocity.x; // round for signals, sometimes near wall, it can glitch from idle to run
                        oldCharacterDirection = characterDirection;

                        if (velocity.x != 0)
                        {
                                characterDirection = (int) Mathf.Sign(velocity.x);
                        }
                        if (forceDirection != 0)
                        {
                                characterDirection = forceDirection;
                                forceDirection = 0;
                        }
                        if (forcedVelocity != Vector2.zero)
                        {
                                velocity = forcedVelocity;
                                forcedVelocity = Vector2.zero;
                        }
                        //Debug.Log("char dir " + characterDirection);
                        //* core signals, always set
                        bool velX = movingX = velocity.x != 0;
                        bool velXLeft = characterDirection < 0;
                        bool velXRight = characterDirection > 0;
                        bool velXZero = velocity.x == 0;
                        bool velY = velocity.y != 0;
                        bool velYUp = velocity.y > 0;
                        bool velYDown = velocity.y < 0;
                        bool velYZero = velocity.y == 0;

                        Set("jumping", !onGround);
                        Set("onGround", onGround);
                        Set("velX", velX);
                        Set("velXLeft", velXLeft);
                        Set("velXRight", velXRight);
                        Set("velXZero", velXZero);
                        Set("velY", velY);
                        Set("velYUp", velYUp);
                        Set("velYDown", velYDown);
                        Set("velYZero", velYZero);
                        Set("alwaysTrue", true);
                        Set("alwaysFalse", false);
                        Set("changedDirection", oldCharacterDirection != characterDirection);
                        Set("sameDirection", oldCharacterDirection == characterDirection);
                        Set("onGroundWall", onGround && onWallStop);
                }

        }
}
