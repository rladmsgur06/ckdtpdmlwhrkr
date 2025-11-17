using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [AddComponentMenu("Flare Engine/一Interactables/HighJump")]
        public class HighJump : MonoBehaviour
        {
                [SerializeField] public HighJumpType type;
                [SerializeField] public float force = 30f;
                [SerializeField] public float timer = 1f;
                [SerializeField] public float windRate = 0.25f;
                [SerializeField] public float slowRate = 0.25f;
                [SerializeField] public string trampolineWE;
                [SerializeField] public string windWE;
                [SerializeField] public string speedBoostWE;
                [SerializeField] public string slowDownWE;
                [SerializeField] public bool moveWithParent = false;
                [SerializeField] public Vector2 windDirection = new Vector2(0, 1f);
                [SerializeField] public UnityEventEffect onTrampoline;
                [SerializeField] public UnityEventEffect onWind;
                [SerializeField] public UnityEventEffect onSlowDown;
                [SerializeField] public UnityEventEffect onSpeedBoost;
                [SerializeField] public Collider2D collider2DRef;
                [SerializeField] public SimpleBounds bounds = new SimpleBounds();

                [System.NonSerialized] private float windTimer = 0;
                [System.NonSerialized] private float slowTimer = 0;
                public static List<HighJump> highJumps = new List<HighJump>();

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool foldOut;
                [SerializeField, HideInInspector] private bool eventFoldOut;
                [SerializeField, HideInInspector] private bool trampolineFoldOut;
                [SerializeField, HideInInspector] private bool windFoldOut;
                [SerializeField, HideInInspector] private bool speedBoostFoldOut;
                [SerializeField, HideInInspector] private bool slowDownFoldOut;
                [SerializeField, HideInInspector] public Vector3 oldPosition;
#pragma warning restore 0414
#endif
                #endregion

                private void Start ()
                {
                        bounds.Initialize(transform);
                }
                private void OnEnable ()
                {
                        if (!highJumps.Contains(this))
                        {
                                highJumps.Add(this);
                        }
                }

                private void OnDisable ()
                {
                        if (highJumps.Contains(this))
                        {
                                highJumps.Remove(this);
                        }
                }

                public bool DetectCharacter (WorldCollision character)
                {
                        if (collider2DRef != null && character.boxCollider != null)
                        {
                                return character.boxCollider.IsTouching(collider2DRef);
                        }
                        return bounds.ContainsRaw(character.position) || bounds.ContainsRaw(character.oldPosition);
                }

                public bool WindTimer ()
                {
                        if (Clock.Timer(ref windTimer, windRate))
                        {
                                return true;
                        }
                        return false;
                }

                public bool SlowTimer ()
                {
                        if (Clock.Timer(ref slowTimer, slowRate))
                        {
                                return true;
                        }
                        return false;
                }

                public static bool Find (WorldCollision character, float characterVelocityY, ref int highJump, ref float timer, ref Vector2 force, ref Transform oldBoost)
                {
                        for (int i = 0; i < highJumps.Count; i++)
                        {
                                if (highJumps[i] != null && highJumps[i].DetectCharacter(character))
                                {
                                        if (highJumps[i].type == HighJumpType.Trampoline)
                                        {
                                                if (characterVelocityY > 0)
                                                {
                                                        return false;
                                                }
                                                character.hitInteractable = true;
                                                force = highJumps[i].force * highJumps[i].transform.up;
                                                highJumps[i].onTrampoline.Invoke(ImpactPacket.impact.Set(highJumps[i].trampolineWE, character.position, Vector2.zero));
                                                highJump = 1;
                                                return true;
                                        }
                                        else if (highJumps[i].type == HighJumpType.Wind)
                                        {
                                                character.hitInteractable = true;
                                                Vector2 windDirection = highJumps[i].transform.up; //highJumps[i].windDirection;
                                                float percentToTop = Mathf.Clamp01(character.transform.position.y / (highJumps[i].bounds.top - 0.5f));
                                                float windForce = Mathf.Lerp(0f, highJumps[i].force, 1f - percentToTop);
                                                force = windDirection * windForce * Time.deltaTime * 10f;
                                                if (highJumps[i].WindTimer())
                                                {
                                                        highJumps[i].onWind.Invoke(ImpactPacket.impact.Set(highJumps[i].windWE, character.position, Vector2.zero));
                                                }
                                                highJump = 2;
                                                return true;
                                        }
                                        else if (highJumps[i].type == HighJumpType.SpeedBoost)
                                        {
                                                if (highJumps[i].transform == oldBoost)
                                                {
                                                        return false;
                                                }
                                                force = highJumps[i].force * highJumps[i].transform.up;
                                                timer = highJumps[i].timer;
                                                highJumps[i].onSpeedBoost.Invoke(ImpactPacket.impact.Set(highJumps[i].speedBoostWE, character.position, Vector2.zero));
                                                oldBoost = highJumps[i].transform;
                                                highJump = 3;
                                                return true;
                                        }
                                        else
                                        {
                                                if (highJumps[i].SlowTimer())
                                                {
                                                        highJumps[i].onSlowDown.Invoke(ImpactPacket.impact.Set(highJumps[i].slowDownWE, character.position, Vector2.zero));
                                                }
                                                force = highJumps[i].force * Vector2.one;
                                                highJump = 4;
                                                return true;
                                        }
                                }
                        }
                        return false;
                }

        }

        public enum HighJumpType
        {
                Trampoline,
                Wind,
                SpeedBoost,
                SlowDown
        }
}

// if (slowTimer + slowRate >= Time.time)
// {
//         slowTimer = Time.time;
//         return true;
// }
