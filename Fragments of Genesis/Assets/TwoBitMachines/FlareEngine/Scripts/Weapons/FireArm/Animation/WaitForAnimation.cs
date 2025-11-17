using TwoBitMachines.TwoBitSprite;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class WaitForAnimation
        {
                [SerializeField] public bool wait;
                [SerializeField] public float waitTime;
                [SerializeField] public string weaponAnimation;
                [SerializeField] public string extraSignal;
                [SerializeField] public SpriteEngineBase spriteEngine;
                [System.NonSerialized] private AnimState state = AnimState.Ready;

                bool needToClear = false;
                public enum AnimState
                {
                        Ready,
                        Active,
                        ShootAndComplete,
                        ShootAndWait,
                        Wait,
                        Block
                }

                public void Reset ( )
                {
                        state = AnimState.Ready;
                        needToClear = false;
                }

                public bool AnimationNotPlaying (ref bool fire, ref float counter, AnimationSignals signals)
                {
                        if (!wait)
                        {
                                return true;
                        }

                        switch (state)
                        {
                                case AnimState.Ready:
                                        if (fire)
                                        {
                                                fire = false;
                                                SetSignal (signals);
                                                state = AnimState.Active;
                                                if (needToClear)
                                                {
                                                        ClearSignal (signals); // animation can get stuck if you shoot on last frame of animation since animation never gets reset and stays in loop once
                                                }
                                                needToClear = false;
                                                return false;
                                        }
                                        return true;
                                case AnimState.Active:
                                        SetSignal (signals);
                                        return false;
                                case AnimState.Wait:
                                        SetSignal (signals);
                                        return false;
                                case AnimState.ShootAndComplete:
                                        counter = float.MaxValue; //   make timer expire to go into shoot state immediately  
                                        state = AnimState.Ready;
                                        return fire = true;
                                case AnimState.ShootAndWait:
                                        SetSignal (signals);
                                        counter = float.MaxValue;
                                        state = AnimState.Wait;
                                        return fire = true;
                                        //block state is used for a charged projectile, we wait until firearm is charged to be able to play animation
                        }

                        return false;
                }

                public void Unblock ( )
                {
                        state = AnimState.Ready;
                }

                public void BlockIfReady ( )
                {
                        if (state == AnimState.Ready)
                        {
                                state = AnimState.Block;
                        }
                }

                public void AnimationComplete ( )
                {
                        state = AnimState.Ready;
                        needToClear = true;
                }

                public void ShootAndAnimationComplete ( ) // charging state should really only use ShootAndAC or else animation will block discharge
                {
                        if (state != AnimState.Ready)
                        {
                                state = AnimState.ShootAndComplete;
                                needToClear = true;
                        }
                }

                public void ShootAndWaitForAnimation ( )
                {
                        if (state != AnimState.Ready)
                        {
                                state = AnimState.ShootAndWait;
                                needToClear = true;
                        }
                }

                private void SetSignal (AnimationSignals signals)
                {
                        signals.Set (weaponAnimation);
                        if (spriteEngine != null)
                        {
                                spriteEngine.SetSignal (extraSignal);
                        }
                }

                private void ClearSignal (AnimationSignals signals)
                {
                        signals.Set (weaponAnimation, false);
                        if (spriteEngine != null)
                        {
                                spriteEngine.SetSignalFalse (extraSignal);
                        }
                }
        }

}