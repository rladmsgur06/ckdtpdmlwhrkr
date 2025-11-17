using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu ("Flare Engine/一Weapons/FlameThrower")]
        public class FlameThrower : MonoBehaviour
        {
                [SerializeField] public int particles = 50;
                [SerializeField] public Sprite sprite;
                [SerializeField] public Material material;
                [SerializeField] public FlameParticleProperties properties;

                [System.NonSerialized] private Mesh mesh;
                [System.NonSerialized] private Vector4[] colors;
                [System.NonSerialized] private ProjectileBase projectile;
                [System.NonSerialized] private MaterialPropertyBlock propertyBlock;
                [System.NonSerialized] private List<FlameParticle> projectiles = new List<FlameParticle> ( );
                [System.NonSerialized] private List<Matrix4x4> tempData = new List<Matrix4x4> ( );
                [System.NonSerialized] private float counter;

                public void Awake ( ) // create mesh quad and material instance
                {
                        mesh = QuadMesh.Create ( );
                        if (material != null)
                        {
                                material.mainTexture = sprite.texture;
                                material.enableInstancing = true;
                        }

                        propertyBlock = new MaterialPropertyBlock ( );
                        colors = new Vector4[particles];
                        for (int i = 0; i < particles; i++)
                        {
                                FlameParticle newParticle = new FlameParticle ( );
                                newParticle.sleep = true;
                                newParticle.Set (properties);
                                projectiles.Add (newParticle);
                        }
                }

                private void Start ( )
                {
                        projectile = this.gameObject.GetComponent<ProjectileBase> ( );
                }

                private void OnEnable ( )
                {
                        for (int i = 0; i < projectiles.Count; i++)
                                if (!projectiles[i].sleep)
                                {
                                        projectiles[i].sleep = true;
                                }
                }

                public void Update ( )
                {
                        if (projectiles.Count == 0)
                        {
                                return;
                        }

                        if (projectile != null && projectile.triggerReleased)
                        {
                                if (!projectile.gameObject.activeInHierarchy) projectile.gameObject.SetActive (true);
                                UpdateProjectiles ( );
                                bool allSleeping = true;
                                for (int i = 0; i < projectiles.Count; i++)
                                        if (!projectiles[i].sleep)
                                        {
                                                allSleeping = false;
                                                break;
                                        }
                                if (allSleeping)
                                {
                                        this.gameObject.SetActive (false);
                                        projectile.triggerReleased = false;
                                }
                                return;
                        }

                        if (Clock.Timer (ref counter, 0.031f))
                        {
                                for (int i = 0; i < projectiles.Count; i++)
                                        if (projectiles[i].sleep)
                                        {
                                                Vector2 startingVel = projectile != null ? projectile.playerVelocity : Vector2.zero;
                                                projectiles[i].Reset (transform.position, transform, startingVel);
                                                break;
                                        }
                        }

                        UpdateProjectiles ( );
                }

                private void UpdateProjectiles ( )
                {
                        tempData.Clear ( );
                        float deltaTime = Time.deltaTime;
                        for (int i = 0; i < projectiles.Count; i++)
                        {
                                colors[i] = projectiles[i].Execute (this.transform, deltaTime, i);
                                tempData.Add (projectiles[i].transformData); // recheck if sleeping
                        }
                        propertyBlock.SetVectorArray ("colors", colors);
                        if (mesh != null && material != null) Graphics.DrawMeshInstanced (mesh, 0, material, tempData, propertyBlock);
                }

        }

        [System.Serializable]
        public class FlameParticle
        {
                public bool sleep;
                private float counter;
                private float variance;
                private float velAngle;
                private Vector2 startVel;
                private Vector3 position;
                private Vector3 scale = Vector3.one;
                private Quaternion rotation = Quaternion.identity;
                private FlameParticleProperties properties;

                public Matrix4x4 transformData
                {
                        get
                        {
                                Vector3 offset = Compute.RotateVector (scale * 0.5f, rotation.eulerAngles.z + 180f);
                                return Matrix4x4.TRS (position + offset, rotation, scale);
                        }
                }

                public void Set (FlameParticleProperties properties)
                {
                        this.properties = properties;
                }

                public void Reset (Vector3 position, Transform transform, Vector3 startVel)
                {
                        this.position = position + transform.up * Random.Range (-0.25f, 0.25f) + transform.right * Random.Range (-0.25f, 0.25f);
                        this.variance = Random.Range (-properties.lifeTime * 0.2f, properties.lifeTime * 0.2f);
                        this.velAngle = Compute.AngleDirection (Vector2.right, transform.right);
                        this.startVel = transform.rotation * Compute.Abs (startVel);
                        sleep = false;
                        counter = 0;
                }

                public Color Execute (Transform transform, float deltaTime, int i)
                {
                        float life = properties.lifeTime + variance;
                        if (Clock.TimerExpired (ref counter, life))
                        {
                                sleep = true;
                                return Color.clear;
                        }

                        scale.x = scale.y = properties.scaleCurve.Evaluate (counter / life) * properties.scale;
                        rotation = Quaternion.Euler (0, 0, properties.angleCurve.Evaluate (counter / life) * properties.angle);
                        float velocity = properties.velocityCurve.Evaluate (counter / life) * properties.velocity * deltaTime;
                        Vector2 v = Compute.RotateVector (new Vector2 (velocity, 0), velAngle);
                        position.x += v.x + (Compute.SameSign (v.x, startVel.x) ? startVel.x : 0);
                        position.y += v.y;
                        return properties.colorGradient.Evaluate (Mathf.Clamp (counter / life, 0f, 1f));
                }

        }

        [System.Serializable]
        public class FlameParticleProperties
        {
                public float lifeTime;
                public float velocity;
                public float scale;
                public float angle;
                public AnimationCurve velocityCurve;
                public AnimationCurve scaleCurve;
                public AnimationCurve angleCurve;

                [SerializeField]
                public Gradient colorGradient = new Gradient
                {
                        alphaKeys = new []
                        {
                        new GradientAlphaKey (0, 0f),
                        new GradientAlphaKey (1, 1f)
                        },

                        colorKeys = new []
                        {
                        new GradientColorKey (Color.red, 0f),
                        new GradientColorKey (Color.cyan, 0.5f),
                        new GradientColorKey (Color.green, 1f)
                        }
                };

        }

}