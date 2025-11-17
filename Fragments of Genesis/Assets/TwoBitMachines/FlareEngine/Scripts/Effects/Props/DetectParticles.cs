using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class DetectParticles : MonoBehaviour
        {
                [SerializeField] public string worldEffect = "";
                [SerializeField] public UnityEventEffect onHit = new UnityEventEffect ( );
                [SerializeField] public float probability = 0.5f;
                [System.NonSerialized] private ParticleSystem particles;
                [System.NonSerialized] private ParticleSystem.Particle[] m_Particles;
                [System.NonSerialized] private List<ParticleCollisionEvent> hit = new List<ParticleCollisionEvent> ( );

                private void Start ( )
                {
                        particles = GetComponent<ParticleSystem> ( );
                }

                void OnParticleCollision (GameObject other)
                {
                        if (Random.Range (0, 1f) >= probability)
                        {
                                return;
                        }
                        if (m_Particles == null || m_Particles.Length < particles.main.maxParticles)
                        {
                                m_Particles = new ParticleSystem.Particle[particles.main.maxParticles];
                        }
                        int numParticlesAlive = particles.GetParticles (m_Particles);
                        if (numParticlesAlive == 0)
                        {
                                return;
                        }

                        Vector2 position = this.transform.position;
                        int count = particles.GetCollisionEvents (other, hit);
                        bool update = false;

                        for (int i = 0; i < 1; i++)
                        {
                                for (int j = 0; j < numParticlesAlive; j++)
                                {
                                        if (((position + (Vector2) m_Particles[j].position) - (Vector2) hit[i].intersection).sqrMagnitude < 0.25f)
                                        {
                                                update = true;
                                                m_Particles[j].remainingLifetime = 0;
                                                onHit.Invoke (ImpactPacket.impact.Set (worldEffect, hit[i].intersection, Vector2.zero));
                                        }
                                }
                        }

                        if (update)
                        {
                                particles.SetParticles (m_Particles, numParticlesAlive); // Update particle system
                        }
                }

        }
}