using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class SpriteTrail : MonoBehaviour
        {
                [SerializeField] public float effectTime = 0.5f;
                [SerializeField] public float spawnRate = 0.05f;
                [SerializeField] public Gradient gradient;
                [SerializeField] public GameObject template;

                [System.NonSerialized] private float counter = 0;
                [System.NonSerialized] private SpriteRenderer spriteRenderer;
                [System.NonSerialized] private Vector2 oldPosition;
                [System.NonSerialized] private List<SpriteTrailEffect> list = new List<SpriteTrailEffect>();

                public void Start ()
                {
                        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                }

                public void OnEnable ()
                {
                        spawnRate = Mathf.Clamp(spawnRate, 0.01f, 100f);
                        counter = 1000f;
                }

                public void LateUpdate ()
                {
                        if (spriteRenderer == null)
                        {
                                enabled = false;
                                return;
                        }
                        for (int i = 0; i < list.Count; i++)
                        {
                                list[i].RunTrail();
                        }
                        if ((oldPosition - (Vector2) transform.position).sqrMagnitude < 0.1f)
                        {
                                return;
                        }
                        if (Clock.Timer(ref counter, spawnRate))
                        {
                                CreateTrail(this, spriteRenderer);
                        }
                        oldPosition = transform.position;
                }

                public void CreateTrail (SpriteTrail trail, SpriteRenderer spriteRenderer)
                {
                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i].SetTrail(trail, spriteRenderer))
                                {
                                        return;
                                }
                        }

                        SpriteTrailEffect newTrail = new SpriteTrailEffect(trail, WorldManager.get.gameObject);
                        newTrail.SetTrail(trail, spriteRenderer);
                        list.Add(newTrail);
                }
        }

        public class SpriteTrailEffect
        {
                [System.NonSerialized] public bool set;
                [System.NonSerialized] private float time;
                [System.NonSerialized] public float counter;
                [System.NonSerialized] private Gradient gradient;
                [System.NonSerialized] public GameObject gameObject;
                [System.NonSerialized] public SpriteRenderer renderer;

                public void Reset ()
                {
                        counter = 0;
                        set = false;
                        if (gameObject != null)
                        {
                                gameObject.SetActive(false);
                        }
                }

                public SpriteTrailEffect (SpriteTrail trail, GameObject parent)
                {
                        gameObject = trail.template != null ? MonoBehaviour.Instantiate(trail.template) : new GameObject();
                        gameObject.transform.localPosition = Vector3.zero;
                        gameObject.transform.SetParent(parent.transform);
                        gameObject.SetActive(true);
                        gameObject.hideFlags = HideFlags.HideInHierarchy;
                        SpriteTrailUnit unit = gameObject.AddComponent<SpriteTrailUnit>();
                        unit.effect = this;

                        if (trail.template == null)
                        {
                                renderer = gameObject.AddComponent<SpriteRenderer>();
                        }
                        else
                        {
                                renderer = gameObject.GetComponent<SpriteRenderer>();
                                if (renderer == null)
                                {
                                        renderer = gameObject.AddComponent<SpriteRenderer>();
                                }
                        }
                }

                public bool SetTrail (SpriteTrail trail, SpriteRenderer renderer)
                {
                        if (set)
                                return false;

                        set = true;
                        gameObject.SetActive(true);
                        gameObject.transform.position = renderer.transform.position;
                        gameObject.transform.rotation = renderer.transform.rotation;
                        gameObject.transform.localScale = renderer.transform.localScale;
                        gradient = trail.gradient;
                        time = trail.effectTime;

                        this.renderer.sprite = renderer.sprite;
                        this.renderer.sortingLayerID = renderer.sortingLayerID;
                        this.renderer.sortingOrder = renderer.sortingOrder - 1;
                        this.renderer.flipX = renderer.flipX;
                        this.renderer.flipY = renderer.flipY;
                        return true;
                }

                public void RunTrail ()
                {
                        if (!set)
                                return;

                        counter += Time.deltaTime;
                        if (renderer != null)
                        {
                                renderer.color = gradient.Evaluate(Mathf.Clamp01(counter / time));
                        }
                        if (counter >= time)
                        {
                                Reset();
                        }
                }
        }
}
