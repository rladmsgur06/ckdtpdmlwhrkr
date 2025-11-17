using UnityEngine;
using UnityEngine.UI;

namespace TwoBitMachines.TwoBitSprite
{
        [System.Serializable]
        public class SpritePlayer
        {
                [SerializeField] public SpriteRenderer renderer;
                [SerializeField] public bool looped;

                [System.NonSerialized] private SpritePacket currentSprite;
                [System.NonSerialized] private SpritePacket nextSprite;
                [System.NonSerialized] private int frameIndex;
                [System.NonSerialized] private int oneFrame;
                [System.NonSerialized] private float counter;

                public void Initialize (Transform transform)
                {
                        if (renderer == null)
                        {
                                renderer = transform.GetComponent<SpriteRenderer>();
                        }
                }

                public void Play ()
                {
                        if (currentSprite == null)
                        {
                                return;
                        }

                        if (Clock.Timer(ref counter, currentSprite.frame[frameIndex].rate) && oneFrame > 0)
                        {
                                frameIndex += 1;
                                if (frameIndex >= currentSprite.frame.Count) //  has looped
                                {
                                        looped = true;
                                        if (currentSprite.loopOnce)
                                        {
                                                currentSprite.onLoopOnce.Invoke();
                                        }
                                        if (nextSprite != null)
                                        {
                                                SetAnimation(nextSprite);
                                                nextSprite = null;
                                                return;
                                        }
                                        if (currentSprite.loopOnce) //            nextSprite has to be read
                                        {
                                                currentSprite.ResetProperties();
                                                currentSprite = null;
                                                return;
                                        }
                                        frameIndex = currentSprite.loopStartIndex;
                                        EnterFrame(frameIndex);
                                }
                                else
                                {
                                        EnterFrame(frameIndex, false);
                                }
                        }
                        currentSprite.InterpolateProperties(frameIndex, counter);
                        oneFrame++; // play at least one frame
                }

                public void SetAnimation (SpritePacket newSprite)
                {
                        currentSprite?.ResetProperties();

                        if (newSprite.frame.Count != 0)
                        {
                                counter = 0;
                                oneFrame = 0;
                                frameIndex = 0;
                                nextSprite = null;
                                currentSprite = newSprite;
                                // frameIndex = newSprite.rememberLastFrame ? Mathf.Clamp (newSprite.lastFrame + 1, 0, newSprite.frame.Count - 1) : 0; //-NEW
                                EnterFrame(frameIndex);
                        }
                }

                public void SetAnimationSync (SpritePacket newSprite)
                {
                        currentSprite?.ResetProperties();

                        if (newSprite.frame.Count != 0)
                        {
                                counter = 0;
                                oneFrame = 0;
                                nextSprite = null;
                                currentSprite = newSprite;
                                frameIndex = Mathf.Clamp(frameIndex + 1, 0, currentSprite.frame.Count - 1);
                                EnterFrame(frameIndex);
                        }
                }

                public void SetNextAnimation (SpritePacket nextSprite) // clear entire queue
                {
                        this.nextSprite = nextSprite;
                }

                private void EnterFrame (int index, bool firstFrame = true)
                {
                        //currentSprite.lastFrame = index;
                        currentSprite.frame[index].onEnterFrame.Invoke();
                        renderer.sprite = currentSprite.frame[index].sprite;
                        currentSprite.SetProperties(index, firstFrame: firstFrame);
                }
        }

        [System.Serializable]
        public class SpritePlayerUI
        {
                [SerializeField] public Image renderer;
                [SerializeField] public bool looped;

                [System.NonSerialized] private SpritePacket currentSprite;
                [System.NonSerialized] private SpritePacket nextSprite;
                [System.NonSerialized] private int frameIndex;
                [System.NonSerialized] private int oneFrame;
                [System.NonSerialized] private float counter;

                public void Initialize (Transform transform)
                {
                        if (renderer == null)
                        {
                                renderer = transform.GetComponent<Image>();
                        }
                }

                public void Play ()
                {
                        if (currentSprite == null)
                        {
                                return;
                        }

                        if (Clock.Timer(ref counter, currentSprite.frame[frameIndex].rate) && oneFrame > 0)
                        {
                                frameIndex += 1;
                                if (frameIndex >= currentSprite.frame.Count) //  has looped
                                {
                                        looped = true;
                                        if (currentSprite.loopOnce)
                                        {
                                                currentSprite.onLoopOnce.Invoke();
                                        }
                                        if (nextSprite != null)
                                        {
                                                SetAnimation(nextSprite);
                                                nextSprite = null;
                                                return;
                                        }
                                        if (currentSprite.loopOnce) //            nextSprite has to be read
                                        {
                                                currentSprite.ResetProperties();
                                                currentSprite = null;
                                                return;
                                        }
                                        frameIndex = currentSprite.loopStartIndex;
                                        EnterFrame(frameIndex);
                                }
                                else
                                {
                                        EnterFrame(frameIndex, false);
                                }
                        }
                        currentSprite.InterpolateProperties(frameIndex, counter);
                        oneFrame++; // play at least one frame
                }

                public void SetAnimation (SpritePacket newSprite)
                {
                        currentSprite?.ResetProperties();

                        if (newSprite.frame.Count != 0)
                        {
                                counter = 0;
                                oneFrame = 0;
                                frameIndex = 0;
                                nextSprite = null;
                                currentSprite = newSprite;
                                // frameIndex = newSprite.rememberLastFrame ? Mathf.Clamp (newSprite.lastFrame + 1, 0, newSprite.frame.Count - 1) : 0; //-NEW
                                EnterFrame(frameIndex);
                        }
                }

                public void SetAnimationSync (SpritePacket newSprite)
                {
                        currentSprite?.ResetProperties();

                        if (newSprite.frame.Count != 0)
                        {
                                counter = 0;
                                oneFrame = 0;
                                nextSprite = null;
                                currentSprite = newSprite;
                                frameIndex = Mathf.Clamp(frameIndex + 1, 0, currentSprite.frame.Count - 1);
                                EnterFrame(frameIndex);
                        }
                }

                public void SetNextAnimation (SpritePacket nextSprite) // clear entire queue
                {
                        this.nextSprite = nextSprite;
                }

                private void EnterFrame (int index, bool firstFrame = true)
                {
                        //currentSprite.lastFrame = index;
                        currentSprite.frame[index].onEnterFrame.Invoke();
                        renderer.sprite = currentSprite.frame[index].sprite;
                        currentSprite.SetProperties(index, firstFrame: firstFrame);
                }
        }
}
