using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite
{
        [AddComponentMenu("Flare Engine/一SpriteEngine/SpriteEngineUI")]
        [DisallowMultipleComponent]
        public class SpriteEngineUI : SpriteEngineBase
        {
                [SerializeField] public List<SpritePacket> sprites = new List<SpritePacket>();
                [SerializeField] public SpriteSwap spriteSwap;

                [System.NonSerialized] public SpritePlayerUI player = new SpritePlayerUI();
                [System.NonSerialized] private int currentIndex = -1;
                private SpritePacket sprite => sprites[currentIndex];

                public void Awake ()
                {
                        player.Initialize(transform);
                        SpriteManager.get.Register(this);
                        tree.Initialize(this);
                        spriteSwap?.Initialize(sprites);

                        if (sprites.Count > 0)
                        {
                                SetNewAnimation(sprites[0].name);
                        }
                }

                public override void SetFirstAnimation ()
                {
                        currentIndex = -1;
                        currentAnimation = "";
                        gameObject.SetActive(true);
                        if (sprites.Count > 0)
                        {
                                SetNewAnimation(sprites[0].name);
                        }
                }

                public override void Play ()
                {
                        if (pause || !enabled)
                        {
                                return;
                        }

                        tree.FindNextAnimation();
                        OnChangedDirection();
                        player.Play();
                        tree.ClearSignals();
                }

                public override void SetNewAnimation (string newAnimation)
                {
                        if (currentAnimation == newAnimation)
                        {
                                return;
                        }

                        for (int i = 0; i < sprites.Count; i++)
                        {
                                if (sprites[i].name == newAnimation)
                                {
                                        SpritePacket newSprite = sprites[i].isRandom ? GetRandom(sprites[i], newAnimation) : sprites[i];

                                        int oldIndex = currentIndex;
                                        currentIndex = i;
                                        if (newSprite.useTransition && newSprite.Transition(sprites, tree, currentAnimation, out SpritePacket transitionAnimation))
                                        {
                                                currentAnimation = newAnimation;
                                                player.SetAnimation(transitionAnimation);
                                                player.SetNextAnimation(newSprite);
                                        }
                                        else
                                        {
                                                currentAnimation = newAnimation;
                                                if (oldIndex >= 0 && oldIndex < sprites.Count && sprites[oldIndex].canSync && sprites[oldIndex].syncID == newSprite.syncID)
                                                {
                                                        player.SetAnimationSync(newSprite);
                                                        return;
                                                }
                                                player.SetAnimation(newSprite);
                                        }
                                        return;
                                }
                        }
                }

                private SpritePacket GetRandom (SpritePacket currentSprite, string newAnimation)
                {
                        string randomAnimation = RandomAnimation.Get(currentSprite.randomAnimations, newAnimation);
                        if (randomAnimation != newAnimation)
                        {
                                for (int i = 0; i < sprites.Count; i++)
                                {
                                        if (sprites[i].name == randomAnimation)
                                        {
                                                return sprites[i];
                                        }
                                }
                        }
                        return currentSprite;
                }

                private void OnChangedDirection ()
                {
                        if (currentIndex < 0 || currentIndex >= sprites.Count || !sprite.changedDirection)
                        {
                                return;
                        }
                        if (tree.SignalTrue("changedDirection") && sprite.Transition(sprites, tree, currentAnimation, out SpritePacket transitionAnimation))
                        {
                                player.SetAnimation(transitionAnimation);
                                player.SetNextAnimation(sprite);
                        }
                }

                public SpritePacket GetSprite (string animationName)
                {
                        for (int i = 0; i < sprites.Count; i++)
                        {
                                if (sprites[i].name == animationName)
                                {
                                        return sprites[i];
                                }
                        }
                        return null;
                }

                public override bool FlipAnimation (Dictionary<string, bool> signal, string signalName, string direction)
                {
                        if (signal.TryGetValue(signalName, out bool value) && value)
                        {
                                Vector3 l = transform.localScale;

                                if (direction == animationDirection[0])
                                {
                                        if (l.x > 0)
                                                transform.localScale = new Vector3(-Mathf.Abs(l.x), l.y, l.z);
                                }
                                else if (direction == animationDirection[1])
                                {
                                        if (l.x < 0)
                                                transform.localScale = new Vector3(Mathf.Abs(l.x), l.y, l.z);

                                }
                                else if (direction == animationDirection[2])
                                {
                                        if (l.y < 0)
                                                transform.localScale = new Vector3(l.y, Mathf.Abs(l.y), l.z);
                                }
                                else if (direction == animationDirection[3])
                                {
                                        if (l.y > 0)
                                                transform.localScale = new Vector3(l.y, -Mathf.Abs(l.y), l.z);
                                }
                        }
                        return value;
                }

                public void SpriteSwap (string skinName)
                {
                        spriteSwap?.Swap(skinName, sprites);
                }

                private void OnDrawGizmosSelected ()
                {
                        player.Initialize(this.transform);
                        // for (int i = 0; i < sprites.Count; i++)
                        // {
                        //         sprites[i].property.Clear(); // to clear serializeReference, nasty bug.
                        // }
                }
        }

}
