using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if SPINE_UNITY
using Spine.Unity;
#endif

namespace TwoBitMachines.TwoBitSprite
{
        [AddComponentMenu("Flare Engine/一SpriteEngine/SpriteEngineSpine")]
        public class SpriteEngineSpine : SpriteEngineBase
        {
#if SPINE_UNITY

                [SerializeField] public List<SpinePacket> animations = new List<SpinePacket>();
                [SerializeField] public SkeletonAnimation animator;

                [System.NonSerialized] private Spine.AnimationState spineAnimationState;
                [System.NonSerialized] private Spine.TrackEntry entry;
                [System.NonSerialized] private Spine.Skeleton skeleton;
                [System.NonSerialized] private int currentIndex = -1;
                [System.NonSerialized] private bool isVisible;
                [System.NonSerialized] private string playAnimation;
                private SpinePacket spine => animations[currentIndex];

                public void Awake ()
                {
                        if (animator == null)
                        {
                                Debug.LogWarning("Sprite Engine Spine requires a SkeletonAnimation. Failed to initialize.");
                                enabled = false;
                                return;
                        }

                        isVisible = true;
                        skeleton = animator.Skeleton;
                        spineAnimationState = animator.AnimationState;
                        SpriteManager.get.Register(this);
                        tree.Initialize(this, animator.transform);

                        if (setToFirst && animations.Count > 0)
                        {
                                SetNewAnimation(animations[0].name);
                        }
                }

                private void SetupSpineSettings ()
                {
                        animator.updateWhenInvisible = UpdateMode.Nothing;
                        animator.immutableTriangles = true;
                        animator.singleSubmesh = true;
                        animator.loop = true;
                }

                public void OnBecameVisible ()
                {
                        isVisible = true;
                }

                public void OnBecameInvisible ()
                {
                        isVisible = false;
                }

                private void OnValidate ()
                {
                        if (animator != null)
                        {
                                SetupSpineSettings();
                        }
                }

                public override void SetFirstAnimation ()
                {
                        currentIndex = -1;
                        currentAnimation = "";
                        this.gameObject.SetActive(true);
                        if (animations.Count > 0)
                        {
                                SetNewAnimation(animations[0].name);
                        }
                }

                public override void Play ()
                {
                        if (pause || !enabled || !isVisible)
                        {
                                return;
                        }

                        tree.FindNextAnimation();
                        OnChangedDirection();
                        OnLoopOnce();
                        tree.ClearSignals();
                }

                public override void SetNewAnimation (string newAnimation)
                {
                        if (currentAnimation == newAnimation || animations == null)
                        {
                                return;
                        }

                        for (int i = 0; i < animations.Count; i++)
                        {
                                if (animations[i].name == newAnimation)
                                {
                                        playAnimation = newAnimation;
                                        SpinePacket newSpine = animations[i].isRandom ? GetRandom(animations[i], newAnimation, ref playAnimation) : animations[i];

                                        int oldIndex = currentIndex;
                                        currentIndex = i;
                                        if (newSpine.hasTransition && newSpine.Transition(this, currentAnimation, out string transitionAnimation))
                                        {
                                                currentAnimation = newAnimation;
                                                spineAnimationState.SetAnimation(0, transitionAnimation, false);
                                                entry = spineAnimationState.AddAnimation(0, playAnimation, true, 0);
                                        }
                                        else
                                        {
                                                currentAnimation = newAnimation;
                                                if (oldIndex >= 0 && oldIndex < animations.Count && animations[oldIndex].canSync && animations[oldIndex].syncID == newSpine.syncID)
                                                {
                                                        float entryTime = entry.TrackTime;
                                                        entry = spineAnimationState.SetAnimation(0, playAnimation, newSpine.loop);
                                                        entry.TrackTime = entryTime;
                                                        return;
                                                }
                                                entry = spineAnimationState.SetAnimation(0, playAnimation, newSpine.loop);
                                        }
                                        return;
                                }
                        }
                }

                private SpinePacket GetRandom (SpinePacket currentSpine, string newAnimation, ref string playAnimation)
                {
                        playAnimation = RandomAnimation.Get(currentSpine.randomAnimations, newAnimation);
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
                        return currentSpine;
                }

                private void OnChangedDirection ()
                {
                        if (currentIndex < 0 || currentIndex >= animations.Count || !spine.changedDirection)
                        {
                                return;
                        }
                        if (tree.SignalTrue("changedDirection") && spine.Transition(this, currentAnimation, out string transitionAnimation))
                        {
                                spineAnimationState.SetAnimation(0, transitionAnimation, false);
                                entry = spineAnimationState.AddAnimation(0, currentAnimation, true, 0);
                        }
                }

                private void OnLoopOnce ()
                {
                        if (entry != null && !entry.Loop && entry.IsComplete && currentIndex >= 0 && currentIndex < animations.Count)
                        {
                                animations[currentIndex].onLoopOnce.Invoke();
                                entry = null;
                        }
                }

                public override bool FlipAnimation (Dictionary<string, bool> signal, string signalName, string direction)
                {
                        if (signal.TryGetValue(signalName, out bool value) && value)
                        {
                                if (direction == animationDirection[0])
                                {
                                        skeleton.ScaleX = -1;
                                }
                                else if (direction == animationDirection[1])
                                {
                                        skeleton.ScaleX = 1;
                                }
                                else if (direction == animationDirection[2])
                                {
                                        skeleton.ScaleY = 1;
                                }
                                else if (direction == animationDirection[3])
                                {
                                        skeleton.ScaleY = -1;
                                }
                        }
                        return value;
                }

#endif
        }

        [System.Serializable]
        public class SpinePacket
        {
                [SerializeField] public string name;
                [SerializeField] public bool loopOnce;
                [SerializeField] public bool hasTransition;
                [SerializeField] public bool hasChangedDirection; // set by editor if changed direction signal is being checked
                [SerializeField] public UnityEvent onLoopOnce;
                [SerializeField] public List<AnimationTransition> transition = new List<AnimationTransition>();
                [SerializeField] public List<RandomAnimation> randomAnimations = new List<RandomAnimation>();
                [SerializeField] public bool isRandom = false;
                [SerializeField] public bool canSync;
                [SerializeField] public int syncID;

                public bool loop => !loopOnce;
                public bool useTransition => hasTransition && transition.Count > 0;
                public bool changedDirection => hasTransition && hasChangedDirection;

                public bool Transition (SpriteEngineSpine spriteEngine, string currentAnimation, out string transitionAnimation)
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
                [SerializeField, HideInInspector] public bool loopFoldOut;
                [SerializeField, HideInInspector] public bool addTransition;
                [SerializeField, HideInInspector] public bool deleteTransition;
                [SerializeField, HideInInspector] public bool transitionFoldOut;
#pragma warning restore 0414
#endif
                #endregion
        }

}
