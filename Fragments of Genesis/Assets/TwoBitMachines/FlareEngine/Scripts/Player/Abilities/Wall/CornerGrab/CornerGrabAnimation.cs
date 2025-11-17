using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [System.Serializable]
        public class CornerGrabAnimation //anim signals: wall, wallCornerGrab
        {
                [SerializeField] public float animationTime = 0.5f;
                [SerializeField] public GrabFinalPosition finalPosition;
                [SerializeField] public UnityEvent onAnimationStart;
                [SerializeField] public UnityEvent onAnimationEnd;
                [SerializeField] public bool useGameObject = false;
                [SerializeField] public List<CornerGrabFrame> animation = new List<CornerGrabFrame> ( );

                [System.NonSerialized] private int wallDirectionRef;
                [System.NonSerialized] private int spriteIndex;
                [System.NonSerialized] private bool holdingMP; // moving platforms
                [System.NonSerialized] private float timer;
                [System.NonSerialized] private float cornerOffsetY;
                [System.NonSerialized] private Vector2 topCorner;
                [System.NonSerialized] private SpriteRenderer renderer;
                [System.NonSerialized] private TwoBitSprite.SpriteEngineBase spriteEngine;

                public bool setOnLastPosition => finalPosition == GrabFinalPosition.SetOnLastFrame;
                public bool isGrabbing { get; private set; }

                public void Initialize (Player player)
                {
                        renderer = player.GetComponent<SpriteRenderer> ( );
                        spriteEngine = player.GetComponent<TwoBitSprite.SpriteEngineBase> ( );
                }

                public void Reset ( )
                {
                        if (isGrabbing && renderer != null && spriteEngine != null)
                        {
                                spriteEngine.pause = false;
                        }
                        isGrabbing = false;
                        holdingMP = false;
                }

                public void StartAnimation (AbilityManager player, int wallDirection, Vector2 topCorner)
                {
                        if (!player.world.onCeiling)
                        {
                                holdingMP = player.world.mp.holding;
                                this.topCorner = topCorner;
                                wallDirectionRef = wallDirection;
                                timer = Mathf.Infinity;
                                cornerOffsetY = 0;
                                spriteIndex = -1;
                                isGrabbing = true;
                        }
                }

                public bool IsPlaying (Wall wall, AbilityManager player, WorldCollision world, ref Vector2 velocity)
                {
                        if (!isGrabbing)
                        {
                                return false;
                        }
                        if (holdingMP)
                        {
                                wall.cornerGrab.FindTopCorner (world.box, wallDirectionRef, velocity.y, ref topCorner);
                        }
                        if (player.world.onCeiling || AnimationComplete (player, ref velocity) || ExitOnCeiling (player.world.box))
                        {
                                Reset ( );
                                return false;
                        }
                        player.signals.Set ("wall");
                        player.signals.Set ("wallCornerGrab");
                        player.signals.Set ("wallLeft", wallDirectionRef < 0);
                        player.signals.Set ("wallRight", wallDirectionRef > 0);
                        player.signals.ForceDirection (wallDirectionRef);

                        velocity.x = world.onWall ? 0.25f * wallDirectionRef : 0;
                        velocity.y = Time.deltaTime <= 0 ? 0 : ((topCorner.y + cornerOffsetY) - world.box.top) / Time.deltaTime;
                        return true;
                }

                private bool AnimationComplete (AbilityManager player, ref Vector2 velocity)
                {
                        if (renderer != null && spriteEngine != null)
                        {
                                spriteEngine.pause = true;
                        }

                        if (Clock.Timer (ref timer, animationTime / animation.Count)) //    play animation
                        {
                                if (spriteIndex < 0)
                                {
                                        onAnimationStart.Invoke ( );
                                }
                                if (++spriteIndex >= animation.Count || spriteIndex < 0) // complete
                                {
                                        velocity.y = -40f;
                                        velocity.x = 0;
                                        player.world.onGround = true;
                                        player.signals.SetDirection (wallDirectionRef);
                                        SetFinalPosition (player.world, setOnLastPosition);
                                        onAnimationEnd.Invoke ( );
                                        return true;
                                }

                                cornerOffsetY += animation[spriteIndex].y;
                                if (renderer != null)
                                {
                                        renderer.sprite = animation[spriteIndex].sprite;
                                        renderer.flipX = wallDirectionRef < 0;
                                }
                                SetFinalPosition (player.world, spriteIndex == animation.Count - 1 && !setOnLastPosition);
                        }
                        return false;
                }

                public void SetFinalPosition (WorldCollision world, bool value)
                {
                        if (value)
                        {
                                world.transform.position = new Vector3 (world.box.TopExactCorner (wallDirectionRef).x + (world.box.sizeX * 0.5f) * wallDirectionRef, topCorner.y + 0.01f, 0);
                                //Debug.DrawRay (world.transform.position, Vector3.up * 5f, Color.black, 2f);
                                world.box.Update ( );
                                Safire2DCamera.Safire2DCamera.ForceFollowSmooth (world.transform);
                        }
                }

                public bool ExitOnCeiling (BoxInfo box)
                {
                        Vector2 origin = box.topCenter;
                        RaycastHit2D hit = Physics2D.Raycast (origin, box.up, 0.25f, WorldManager.collisionMask); //    Debug.DrawRay (origin, box.up * 0.25f, Color.green);
                        return hit && hit.distance > 0;
                }

        }

        [System.Serializable]
        public class CornerGrabFrame
        {
                public Sprite sprite;
                public float y;
        }

        public enum GrabFinalPosition
        {
                SetOnSecondToLastFrame,
                SetOnLastFrame
        }

}