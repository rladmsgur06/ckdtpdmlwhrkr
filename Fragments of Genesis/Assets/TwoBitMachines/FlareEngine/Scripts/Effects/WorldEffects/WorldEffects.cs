using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("Flare Engine/WorldEffects")]
        public class WorldEffects : MonoBehaviour
        {
                [SerializeField] public List<WorldEffectPool> effect = new List<WorldEffectPool>();
                [System.NonSerialized] private Dictionary<string, WorldEffectPool> particleList = new Dictionary<string, WorldEffectPool>();
                [System.NonSerialized] private GameObject container;
                [System.NonSerialized] public static List<WorldEffects> effects = new List<WorldEffects>();
                public static WorldEffects get;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool foldOut;
#pragma warning restore 0414
#endif
                #endregion

                private void Awake ()
                {
                        get = this;
                        container = new GameObject("Container");
                        container.transform.parent = this.transform;
                        for (int i = 0; i < effect.Count; i++)
                        {
                                effect[i].Initialize(container.transform);
                                particleList.Add(effect[i].gameObject.name, effect[i]);
                        }
                }

                private void OnEnable ()
                {
                        if (!effects.Contains(this))
                                effects.Add(this);
                }

                private void OnDisable ()
                {
                        if (effects.Contains(this))
                                effects.Remove(this);
                        get = null;
                }

                public static void ResetEffects ()
                {
                        for (int i = 0; i < effects.Count; i++)
                        {
                                if (effects[i] == null)
                                        continue;
                                for (int j = 0; j < effects[i].effect.Count; j++)
                                {
                                        if (effects[i].effect[j] == null)
                                                continue;
                                        effects[i].effect[j].ResetAll();
                                }
                        }
                }

                // Sprites should be pointing updward
                public void Activate (ImpactPacket impact)
                {
                        if (particleList.TryGetValue(impact.name, out WorldEffectPool effect))
                        {
                                effect.Activate(impact, impact.bottomPosition);
                        }
                }

                public void ActivateWithDirection (ImpactPacket impact)
                {
                        if (particleList.TryGetValue(impact.name, out WorldEffectPool effect))
                        {
                                effect.Activate(impact, impact.bottomPosition, Quaternion.LookRotation(Vector3.forward, impact.direction));//
                        }
                }

                public void ActivateWithInvertedDirection (ImpactPacket impact)
                {
                        if (particleList.TryGetValue(impact.name, out WorldEffectPool effect))
                        {
                                effect.Activate(impact, impact.bottomPosition, Quaternion.LookRotation(Vector3.forward, -impact.direction));
                        }
                }

                public void ActivateAndClearDirection (ImpactPacket impact)
                {
                        if (particleList.TryGetValue(impact.name, out WorldEffectPool effect))
                        {
                                effect.Activate(impact, impact.bottomPosition);
                                WorldEffectPool.currentGameObject.transform.localEulerAngles = Vector3.zero;
                        }
                }

                // private void FlipScaleY (ImpactPacket impact)
                // {
                //         // make sure sprite's orientation is correct in they axis
                //         Vector3 ls = WorldEffectPool.currentGameObject.transform.localScale;
                //         if (impact.transform.up.y >= 0)
                //         {
                //                 WorldEffectPool.currentGameObject.transform.localScale = new Vector3(ls.x, impact.direction.x > 0 ? Mathf.Abs(ls.y) : -Mathf.Abs(ls.y), ls.z);
                //         }
                //         else
                //         {
                //                 WorldEffectPool.currentGameObject.transform.localScale = new Vector3(ls.x, impact.direction.x > 0 ? -Mathf.Abs(ls.y) : Mathf.Abs(ls.y), ls.z);
                //         };
                // }

                // private void FlipScaleX (ImpactPacket impact)
                // {
                //         Vector3 ls = WorldEffectPool.currentGameObject.transform.localScale;
                //         if (impact.transform.right.y < 0)
                //         {
                //                 WorldEffectPool.currentGameObject.transform.localScale = new Vector3(-Mathf.Abs(ls.x), Mathf.Abs(ls.y), ls.z);
                //         }
                //         else
                //         {
                //                 WorldEffectPool.currentGameObject.transform.localScale = new Vector3(Mathf.Abs(ls.x), Mathf.Abs(ls.y), ls.z);
                //         };
                // }

        }

        [System.Serializable]
        public class WorldEffectPool
        {
                [SerializeField] public GameObject gameObject;
                [System.NonSerialized] private Transform parent;
                [System.NonSerialized] private List<GameObject> list = new List<GameObject>();

                public static GameObject currentGameObject;
                public static ImpactPacket currentImpact;

                public void Initialize (Transform parent)
                {
                        this.parent = parent;
                        list.Add(gameObject);
                }

                public void ResetAll ()
                {
                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i] != null)
                                {
                                        list[i].SetActive(false);
                                }
                        }
                }

                public void Activate (ImpactPacket impact, Vector3 position, Quaternion rotation)
                {
                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i] != null && !list[i].activeInHierarchy)
                                {
                                        Transform transform = list[i].transform;
                                        currentGameObject = transform.gameObject;
                                        currentImpact = impact;
                                        transform.position = position;
                                        transform.rotation = rotation;
                                        transform.gameObject.SetActive(true);
                                        return;
                                }
                        }

                        GameObject newEffect = MonoBehaviour.Instantiate(gameObject, position, rotation, parent);
                        currentGameObject = newEffect;
                        currentImpact = impact;
                        list.Add(newEffect);
                        newEffect.gameObject.SetActive(true);
                }

                public void Activate (ImpactPacket impact, Vector3 position)
                {
                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i] != null && !list[i].activeInHierarchy)
                                {
                                        Transform transform = list[i].transform;
                                        currentGameObject = transform.gameObject;
                                        currentImpact = impact;
                                        transform.position = position;
                                        transform.gameObject.SetActive(true);
                                        return;
                                }
                        }

                        GameObject newEffect = MonoBehaviour.Instantiate(gameObject, position, Quaternion.identity, parent);
                        currentGameObject = newEffect;
                        currentImpact = impact;
                        list.Add(newEffect);
                        newEffect.gameObject.SetActive(true);
                }
        }
}
