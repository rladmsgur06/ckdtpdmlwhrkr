using System;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [System.Serializable]
        public class WaveProperties
        {
                [SerializeField] public float amplitude = 0.2f;
                [SerializeField] public float frequency = 5f;
                [SerializeField] public float speed = 1f;
                [SerializeField] public float spring = 0.05f;
                [SerializeField] public float damping = 0.04f;
                [SerializeField] public float turbulence = 0f;
                [SerializeField] public float randomCurrent = 0f;
                [SerializeField] public Point[] dynamicWave;
                [SerializeField] public Point[] wave;

                [SerializeField] private float[] leftDeltas;
                [SerializeField] private float[] rightDeltas;

                [NonSerialized] public float currentStrength;
                [NonSerialized] private float extraTimeLength;
                [NonSerialized] private float currentCounter;
                [NonSerialized] private float turbulenceTimer;
                [NonSerialized] private float speedRate;
                [NonSerialized] private bool round;

                public void Create (Water waves, int length, float particleLength)
                {
                        wave = new Point[length];
                        leftDeltas = new float[length];
                        rightDeltas = new float[length];
                        dynamicWave = new Point[length];

                        for (int i = 0; i < length; i++)
                        {
                                Vector3 position = waves.transform.position + Vector3.up * waves.size.y + Vector3.right * particleLength * i;
                                dynamicWave[i] = new Point ( ) { x = position.x, y = position.y };
                                wave[i] = new Point ( ) { x = position.x, y = position.y };
                        }
                }

                public void Reset (float baseY)
                {
                        if (dynamicWave == null) return;

                        for (int i = 0; i < dynamicWave.Length; i++)
                        {
                                dynamicWave[i].velocity = 0;
                                dynamicWave[i].y = baseY;
                        }
                }

                public float Execute (Water water, float topY, float height)
                {
                        if (dynamicWave == null) return 0;

                        WaveCurrent (water);

                        for (int i = 0; i < dynamicWave.Length; i++)
                        {
                                float force = spring * (dynamicWave[i].y - topY) + dynamicWave[i].velocity * damping;
                                dynamicWave[i].velocity += -force;
                                dynamicWave[i].y += dynamicWave[i].velocity;
                        }
                        for (int i = 0; i < dynamicWave.Length; i++)
                        {
                                if (i > 0)
                                {
                                        leftDeltas[i] = 0.2f * (dynamicWave[i].y - dynamicWave[i - 1].y); //                spread = 0.2f
                                        dynamicWave[i - 1].velocity += leftDeltas[i];
                                }
                                if (i < dynamicWave.Length - 1)
                                {
                                        rightDeltas[i] = 0.2f * (dynamicWave[i].y - dynamicWave[i + 1].y);
                                        dynamicWave[i + 1].velocity += rightDeltas[i];
                                }
                        }
                        for (int i = 0; i < dynamicWave.Length; i++)
                        {
                                if (i > 0)
                                        dynamicWave[i - 1].y += leftDeltas[i];
                                if (i < dynamicWave.Length - 1)
                                        dynamicWave[i + 1].y += rightDeltas[i];
                        }

                        float phase = dynamicWave.Length == 0 ? 0 : frequency;
                        float baseLine = topY - height;

                        for (int i = 0; i < dynamicWave.Length; i++)
                        {
                                float staticWaveY = topY + Mathf.Sin (speedRate + phase * i) * amplitude;
                                wave[i].y = JoinWaves (topY, baseLine, staticWaveY, dynamicWave[i].position.y); //     combine both waves
                        }
                        return phase;
                }

                private void WaveCurrent (Water waves)
                {
                        if (randomCurrent > 0)
                        {
                                if (Clock.Timer (ref currentCounter, randomCurrent + extraTimeLength))
                                {
                                        extraTimeLength = randomCurrent * UnityEngine.Random.Range (-0.5f, 0.5f);
                                        speed = -speed;
                                }
                                currentStrength = Mathf.MoveTowards (currentStrength, speed, Time.deltaTime * Mathf.Abs (speed) * 1.25f);
                                speedRate += currentStrength * Time.deltaTime;
                        }
                        else
                        {
                                currentStrength = 0;
                                speedRate = Time.time * speed;
                        }

                        if (turbulence > 0 && Clock.Timer (ref turbulenceTimer, 0.05f))
                        {
                                int randomWave = UnityEngine.Random.Range (0, dynamicWave.Length - 1);
                                ApplyImpact (randomWave, turbulence * 0.1f);
                        }
                }

                public float JoinWaves (float baseY, float baseLine, float staticWave, float dynamicWave)
                {
                        float dSW = round ? Compute.Round (staticWave - baseY, 0.125f) : staticWave - baseY;
                        float dDW = round ? Compute.Round (dynamicWave - baseY, 0.125f) : dynamicWave - baseY;
                        float waveTop = baseY + dSW + dDW;
                        return waveTop < baseLine ? baseLine : waveTop;
                }

                public void ApplyImpact (int index, float impact, int splashRange = 4)
                {
                        if (dynamicWave == null) return;

                        for (int i = index - splashRange; i < index + splashRange; i++)
                        {
                                if (i >= 0 && i < dynamicWave.Length)
                                {
                                        dynamicWave[i].velocity = Mathf.Clamp (dynamicWave[i].velocity + impact, -1f, 1f);
                                }
                        }
                }
        }

        [System.Serializable]
        public class Point
        {
                public float x = 0;
                public float y = 0;
                public float velocity = 0;
                public Vector2 position => new Vector2 (x, y);
        }

}