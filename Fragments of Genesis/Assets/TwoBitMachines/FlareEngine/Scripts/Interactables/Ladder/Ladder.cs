using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [AddComponentMenu("Flare Engine/一Interactables/Ladder")]
        public class Ladder : MonoBehaviour
        {
                [SerializeField] public float reverseTime = 0.5f;
                [SerializeField] public bool canFlip;
                [SerializeField] public bool canFlipX;
                [SerializeField] public bool canFlipY;
                [SerializeField] public bool foldOut;
                [SerializeField] public bool reverseFoldOut;

                [SerializeField] public FenceFlip fenceFlip = new FenceFlip();
                [SerializeField] public LadderInstance ladder = new LadderInstance();
                public static List<Ladder> ladders = new List<Ladder>();
                public bool test; // delete

                private void Awake ()
                {
                        ladder.InitializeToTarget(transform);
                }

                private void OnEnable ()
                {
                        if (!ladders.Contains(this))
                        {
                                ladders.Add(this);
                        }
                }

                private void OnDisable ()
                {
                        if (ladders.Contains(this))
                        {
                                ladders.Remove(this);
                        }
                }

                public static bool Find (WorldCollision character, Vector2 velocity, ref Ladder ladderRef)
                {
                        for (int i = ladders.Count - 1; i >= 0; i--)
                        {
                                if (ladders[i] == null)
                                {
                                        continue;
                                }
                                LadderInstance ladder = ladders[i].ladder;
                                if (ladder.target != null && ladder.ContainX(character.position.x))
                                {
                                        float extra = ladder.standOnLadder ? velocity.y * Time.deltaTime : 0;
                                        if (ladder.ContainsY(character.box.top) || ladder.ContainsY(character.box.bottom + extra))
                                        {
                                                ladderRef = ladders[i];
                                                return true;
                                        }
                                }
                        }
                        return false;
                }

#if UNITY_EDITOR
                private void OnDrawGizmos ()
                {
                        if (Application.isPlaying)
                        {
                                if (ladder.target != null)
                                {
                                        ladder.Draw(ladder.rawPosition);
                                }
                        }
                        else
                        {
                                transform.position = Compute.Round(transform.position, 0.25f);
                                ladder.SetPositionAndDraw(transform.position);
                        }
                }
#endif

        }

}
