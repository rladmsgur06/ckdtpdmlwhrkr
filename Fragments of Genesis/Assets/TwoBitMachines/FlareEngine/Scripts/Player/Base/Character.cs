using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("")]
        public partial class Character : MonoBehaviour //* Has partial class Equipment
        {
                [SerializeField] public CharacterType type; // a character is simply something that interacts with the world
                [SerializeField] public WorldCollision world = new WorldCollision();
                [SerializeField] public AnimationSignals signals = new AnimationSignals();
                [SerializeField] public MovingPlatform movingPlatform = new MovingPlatform();

                [SerializeField] public bool turnOffSignals;
                [SerializeField] public bool pushBackActive;
                [SerializeField] public bool executeInLateUpdate;

                [System.NonSerialized] public Vector2 initialVelocity;
                [System.NonSerialized] public Vector2 externalVelocity;

                [System.NonSerialized] public static List<Character> aiCharacters = new List<Character>();
                [System.NonSerialized] public static List<Character> lateAICharacters = new List<Character>();
                [System.NonSerialized] public static List<Character> aiMovingPlatforms = new List<Character>();
                [System.NonSerialized] public static List<WorldCollision> characters = new List<WorldCollision>();
                [System.NonSerialized] public static List<WorldCollision> passengers = new List<WorldCollision>();
                [System.NonSerialized] public static Dictionary<Transform, MovingPlatform> movingPlatforms = new Dictionary<Transform, MovingPlatform>();

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀ 
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool mainFoldOut;
#pragma warning restore 0414
#endif
                #endregion

                public void Start ()
                {
                        Tool[] newTools = this.GetComponentsInChildren<Tool>(true);
                        for (int i = 0; i < newTools.Length; i++)
                        {
                                RegisterTool(newTools[i]);
                        }
                        OnStart();
                }

                public virtual void OnEnable ()
                {
                        if (type == CharacterType.MovingPlatform)
                        {
                                if (!aiMovingPlatforms.Contains(this))
                                {
                                        aiMovingPlatforms.Add(this);
                                }
                                if (!movingPlatforms.ContainsKey(transform))
                                {
                                        movingPlatforms.Add(transform, movingPlatform);
                                }
                        }
                        else if (type == CharacterType.Regular)
                        {
                                if (!executeInLateUpdate && !aiCharacters.Contains(this))
                                {
                                        aiCharacters.Add(this);
                                }
                                if (executeInLateUpdate && !lateAICharacters.Contains(this))
                                {
                                        lateAICharacters.Add(this);
                                }
                                if (!characters.Contains(world))
                                {
                                        characters.Add(world);
                                }
                                if (world.useMovingPlatform && !passengers.Contains(world))
                                {
                                        passengers.Add(world);
                                }
                        }
                        else
                        {
                                if (!executeInLateUpdate && !aiCharacters.Contains(this))
                                {
                                        aiCharacters.Add(this);
                                }
                                if (executeInLateUpdate && !lateAICharacters.Contains(this))
                                {
                                        lateAICharacters.Add(this);
                                }
                        }
                        OnEnabled(true);
                }

                public virtual void OnDisable ()
                {
                        OnEnabled(false);
                }

                public virtual void OnDestroy ()
                {
                        if (type == CharacterType.MovingPlatform)
                        {
                                if (aiMovingPlatforms.Contains(this))
                                {
                                        aiMovingPlatforms.Remove(this);
                                }
                                if (movingPlatforms.ContainsKey(transform))
                                {
                                        movingPlatforms.Remove(transform);
                                }
                        }
                        else if (type == CharacterType.Regular)
                        {
                                if (!executeInLateUpdate && aiCharacters.Contains(this))
                                {
                                        aiCharacters.Remove(this);
                                }
                                if (executeInLateUpdate && lateAICharacters.Contains(this))
                                {
                                        lateAICharacters.Remove(this);
                                }
                                if (characters.Contains(world))
                                {
                                        characters.Remove(world);
                                }
                                if (world.useMovingPlatform && passengers.Contains(world))
                                {
                                        passengers.Remove(world);
                                }
                        }
                        else
                        {
                                if (!executeInLateUpdate && aiCharacters.Contains(this))
                                {
                                        aiCharacters.Remove(this);
                                }
                                if (executeInLateUpdate && lateAICharacters.Contains(this))
                                {
                                        lateAICharacters.Remove(this);
                                }
                        }
                }

                public static void ResetMovingPlatforms ()
                {
                        for (int i = aiMovingPlatforms.Count - 1; i >= 0; i--)
                        {
                                if (aiMovingPlatforms[i] != null)
                                {
                                        aiMovingPlatforms[i].movingPlatform.ResetAll();
                                }
                        }
                }

                public static void AICharacters ()
                {
                        for (int i = aiCharacters.Count - 1; i >= 0; i--)
                        {
                                if (aiCharacters[i] == null)
                                {
                                        aiCharacters.RemoveAt(i);
                                }
                                else
                                {
                                        aiCharacters[i].Execute();
                                }
                        }
                }

                public static void LateAICharacters ()
                {
                        for (int i = lateAICharacters.Count - 1; i >= 0; i--)
                        {
                                if (lateAICharacters[i] == null)
                                {
                                        lateAICharacters.RemoveAt(i);
                                }
                                else
                                {
                                        lateAICharacters[i].Execute();
                                }
                        }
                }

                public static void AIMovingPlatforms ()
                {
                        for (int i = aiMovingPlatforms.Count - 1; i >= 0; i--)
                        {
                                if (aiMovingPlatforms[i] == null)
                                {
                                        aiMovingPlatforms.RemoveAt(i);
                                }
                                else
                                {
                                        aiMovingPlatforms[i].Execute();
                                }
                        }
                }

                public static void ResetAllAI ()
                {
                        for (int i = aiCharacters.Count - 1; i >= 0; i--)
                        {
                                if (aiCharacters[i] != null)
                                {
                                        aiCharacters[i].ResetAI();
                                }
                        }
                        for (int i = lateAICharacters.Count - 1; i >= 0; i--)
                        {
                                if (lateAICharacters[i] != null)
                                {
                                        lateAICharacters[i].ResetAI();
                                }
                        }
                        for (int i = aiMovingPlatforms.Count - 1; i >= 0; i--)
                        {
                                if (aiMovingPlatforms[i] != null)
                                {
                                        aiMovingPlatforms[i].ResetAI();
                                }
                        }
                }

                public void RemoveAI ()
                {
                        if (type == CharacterType.MovingPlatform)
                        {
                                if (aiMovingPlatforms.Contains(this))
                                {
                                        aiMovingPlatforms.Remove(this);
                                }
                        }
                        else
                        {
                                if (!executeInLateUpdate && aiCharacters.Contains(this))
                                {
                                        aiCharacters.Remove(this);
                                }
                                if (executeInLateUpdate && lateAICharacters.Contains(this))
                                {
                                        lateAICharacters.Remove(this);
                                }
                        }
                }

                public virtual void ResetAI () { }

                public virtual void OnStart () { }

                public virtual void Execute () { }

                public virtual void PostAIExecute () { }

                public virtual void OnEnabled (bool onEnable) { }

                public virtual Vector2 Velocity () { return Vector2.zero; }
        }

        public enum CharacterType
        {
                Regular,
                NoCollisionChecks,
                MovingPlatform,
        }
}
