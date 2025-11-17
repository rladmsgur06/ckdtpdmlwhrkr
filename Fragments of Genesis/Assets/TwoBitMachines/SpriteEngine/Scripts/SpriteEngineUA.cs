using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite
{
        [AddComponentMenu("Flare Engine/一SpriteEngine/SpriteEngineUA")]
        public class SpriteEngineUA : SpriteEngineBase
        {
                [SerializeField] public Animator animator;
                [SerializeField] public FlipType flipX;
                [SerializeField] public float flipAngle;
                [SerializeField] public List<UAPacket> animations = new List<UAPacket>();

                [System.NonSerialized] private string playAnimation;
                [System.NonSerialized] private int currentIndex = -1;
                private UAPacket uaPacket => animations[currentIndex];
                private bool transition;
                private bool wasRandom;
                public enum FlipType { ScaleX, Angle }

                public void Awake ()
                {
                        if (animator == null)
                        {
                                Debug.LogWarning("Sprite Engine UA requires an animator. Failed to initialize.");
                                return;
                        }

                        SpriteManager.get.Register(this);
                        tree.Initialize(this, animator.transform);

                        if (setToFirst && animations.Count > 0)
                        {
                                SetNewAnimation(animations[0].name);
                        }
                }

                public override void SetFirstAnimation ()
                {
                        currentIndex = -1;
                        currentAnimation = "";
                        transition = false;
                        gameObject.SetActive(true);
                        if (animations.Count > 0)
                        {
                                SetNewAnimation(animations[0].name);
                        }
                }

                public override void Play ()
                {
                        if (pause || !enabled)
                        {
                                return;
                        }

                        Transition();
                        tree.FindNextAnimation();
                        OnChangedDirection();
                        tree.ClearSignals();
                }

                public override void SetNewAnimation (string newAnimation)
                {
                        if (currentAnimation == newAnimation)
                                return;

                        transition = false;
                        for (int i = 0; i < animations.Count; i++)
                        {
                                if (animations[i].name == newAnimation)
                                {
                                        playAnimation = newAnimation;
                                        UAPacket newAnim = animations[i].isRandom ? GetRandom(animations[i], newAnimation, ref playAnimation) : animations[i];

                                        int oldIndex = currentIndex;
                                        currentIndex = i;
                                        if (newAnim.hasTransition && newAnim.Transition(this, currentAnimation, out string transitionAnimation))
                                        {
                                                currentAnimation = newAnimation;
                                                animator?.Play(transitionAnimation);
                                                transition = true;
                                                wasRandom = newAnim.isRandom;
                                        }
                                        else
                                        {
                                                currentAnimation = newAnimation;
                                                if (oldIndex >= 0 && oldIndex < animations.Count && animations[oldIndex].canSync && animations[oldIndex].syncID == newAnim.syncID)
                                                {
                                                        float entryTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                                                        animator?.Play(playAnimation, 0, entryTime);
                                                        return;
                                                }
                                                animator?.Play(playAnimation);
                                        }
                                        return;
                                }
                        }
                }

                private UAPacket GetRandom (UAPacket currentSprite, string newAnimation, ref string playAnimation)
                {
                        playAnimation = RandomAnimation.Get(currentSprite.randomAnimations, newAnimation);
                        if (playAnimation != newAnimation)
                        {
                                for (int i = 0; i < animations.Count; i++)
                                {
                                        if (animations[i].name == playAnimation)
                                        {
                                                return animations[i];
                                        }
                                }
                        }
                        return currentSprite;
                }

                private void Transition ()
                {
                        if (transition && animator != null)
                        {
                                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                                if (stateInfo.normalizedTime >= 0.97f)
                                {
                                        transition = false;
                                        animator?.Play(wasRandom ? playAnimation : currentAnimation);
                                }
                        }
                }

                private void OnChangedDirection ()
                {
                        if (currentIndex < 0 || currentIndex >= animations.Count || !uaPacket.changedDirection)
                        {
                                return;
                        }
                        if (tree.SignalTrue("changedDirection") && uaPacket.Transition(this, currentAnimation, out string transitionAnimation))
                        {
                                animator?.Play(transitionAnimation);
                                transition = true;
                        }
                }

                public override bool FlipAnimation (Dictionary<string, bool> signal, string signalName, string direction)
                {
                        if (signal.TryGetValue(signalName, out bool value) && value && animator != null)
                        {
                                Transform transform = animator.transform;
                                Vector3 l = transform.localScale;
                                Vector3 a = transform.localEulerAngles;
                                if (flipX == FlipType.ScaleX && direction == animationDirection[0])
                                {
                                        if (l.x > 0)
                                                transform.localScale = new Vector3(-Mathf.Abs(l.x), l.y, l.z);
                                }
                                else if (flipX == FlipType.ScaleX && direction == animationDirection[1])
                                {
                                        if (l.x < 0)
                                                transform.localScale = new Vector3(Mathf.Abs(l.x), l.y, l.z);
                                }
                                if (flipX == FlipType.Angle && direction == animationDirection[0])
                                {
                                        transform.localEulerAngles = new Vector3(a.x, -flipAngle, a.z);
                                }
                                else if (flipX == FlipType.Angle && direction == animationDirection[1])
                                {
                                        transform.localEulerAngles = new Vector3(a.x, flipAngle, a.z);
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

        }

        [System.Serializable]
        public class UAPacket
        {
                [SerializeField] public string name;
                [SerializeField] public bool hasTransition;
                [SerializeField] public bool hasChangedDirection; // set by editor if changed direction signal is being checked
                [SerializeField] public List<AnimationTransition> transition = new List<AnimationTransition>();
                [SerializeField] public List<RandomAnimation> randomAnimations = new List<RandomAnimation>();
                [SerializeField] public bool isRandom = false;
                [SerializeField] public bool canSync;
                [SerializeField] public int syncID;

                public bool useTransition => hasTransition && transition.Count > 0;
                public bool changedDirection => hasTransition && hasChangedDirection;

                public bool Transition (SpriteEngineUA spriteEngine, string currentAnimation, out string transitionAnimation)
                {
                        for (int i = 0; i < transition.Count; i++)
                        {
                                if (transition[i].from != currentAnimation)
                                {
                                        continue;
                                }
                                if (spriteEngine.tree.SignalTrue(transition[i].condition))
                                {
                                        transitionAnimation = transition[i].to;
                                        return true;
                                }
                        }
                        transitionAnimation = "";
                        return false;
                }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public int frameIndex;
                [SerializeField, HideInInspector] public int signalIndex;
                [SerializeField, HideInInspector] public bool active;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool addTransition;
                [SerializeField, HideInInspector] public bool deleteTransition;
                [SerializeField, HideInInspector] public bool transitionFoldOut;
#pragma warning restore 0414
#endif
                #endregion
        }
}
