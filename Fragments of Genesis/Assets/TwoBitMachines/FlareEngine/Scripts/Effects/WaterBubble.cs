using TwoBitMachines.FlareEngine.Interactables;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        public class WaterBubble : MonoBehaviour
        {
                private ParticleSystem systemParticle;
                private ParticleSystem.Particle[] particles;
                private SimpleBounds bounds = new SimpleBounds ( );

                public void Execute (Water water, bool swimming)
                {
                        if (systemParticle == null)
                        {
                                systemParticle = GetComponent<ParticleSystem> ( );
                        }
                        if (particles == null || particles.Length < systemParticle.main.maxParticles)
                        {
                                particles = new ParticleSystem.Particle[systemParticle.main.maxParticles];
                        }
                        int count = systemParticle.GetParticles (particles);
                        bool changed = false;

                        if (water != null)
                        {
                                bounds.position = water.transform.position;
                                bounds.size = water.size;
                                bounds.Initialize ( );
                        }
                        if (swimming)
                        {
                                if (systemParticle.isStopped)
                                {
                                        systemParticle.Play ( );
                                }
                                this.gameObject.SetActive (true);
                        }
                        else
                        {
                                if (count > 0)
                                {
                                        systemParticle.Stop (true, ParticleSystemStopBehavior.StopEmitting);
                                }
                                else
                                {
                                        this.gameObject.SetActive (false);
                                }
                        }
                        for (int i = 0; i < count; i++)
                        {
                                if (!bounds.Contains (particles[i].position))
                                {
                                        particles[i].remainingLifetime = 0;
                                        changed = true;
                                }
                        }
                        if (changed)
                        {
                                systemParticle.SetParticles (particles, count); // Apply the particle changes to the Particle System
                        }
                }

        }
}