using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("Flare Engine/一Audio/AudioSFXGroup")]
        public class AudioSFXGroup : ReactionBehaviour
        {
                [SerializeField] public List<Audio> sfx = new List<Audio>();
                [SerializeField] public AudioManager audioManager;
                [SerializeField] public bool attenuate = false;
                [SerializeField] public float distance = 20f;
                [SerializeField] public float timeRate = 1f;

                [SerializeField] public bool active;
                [SerializeField] public int singalIndex;

                [System.NonSerialized] private float playStamp;
                private bool hasReference => audioManager != null || AudioManager.get != null;

                public void PlaySFXTimeRate (string name)
                {
                        if (!hasReference || Attenuate(out float percent) || Time.time < playStamp)
                        {
                                return;
                        }
                        playStamp = Time.time + timeRate;

                        for (int i = 0; i < sfx.Count; i++)
                        {
                                if (sfx[i].clip.name == name)
                                {
                                        PlayAudio(sfx[i], percent);
                                        return;
                                }
                        }
                }

                public void PlaySFX (string name)
                {
                        if (!hasReference || Attenuate(out float percent))
                        {
                                return;
                        }
                        for (int i = 0; i < sfx.Count; i++)
                        {
                                if (sfx[i].clip.name == name)
                                {
                                        PlayAudio(sfx[i], percent);
                                        return;
                                }
                        }
                }

                public void PlaySFX (int index)
                {
                        if (!hasReference || Attenuate(out float percent))
                        {
                                return;
                        }
                        for (int i = 0; i < sfx.Count; i++)
                        {
                                if (i == index)
                                {
                                        PlayAudio(sfx[i], percent);
                                        return;
                                }
                        }
                }

                public void PlaySFXProbability50 (int index)
                {
                        if (Random.Range(0.0f, 1f) >= 0.5f)
                        {
                                return;
                        }
                        if (!hasReference || Attenuate(out float percent))
                        {
                                return;
                        }
                        for (int i = 0; i < sfx.Count; i++)
                        {
                                if (i == index)
                                {
                                        PlayAudio(sfx[i], percent);
                                        return;
                                }
                        }
                }

                public override void Activate (ImpactPacket packet)
                {
                        for (int i = 0; i < sfx.Count; i++)
                        {
                                PlayAudio(sfx[i], 1f);
                        }
                }

                private bool Attenuate (out float percent)
                {
                        percent = 1f;
                        if (!attenuate)
                        {
                                return false;
                        }

                        float squareDistance = distance * distance;
                        float diff = (WorldManager.gameCam.transform.position - this.transform.position).sqrMagnitude;

                        if (diff > squareDistance)
                        {
                                return true;
                        }
                        if (diff < 144f)
                        {
                                return false;
                        }

                        percent = 1f - (diff - 144f) / squareDistance;
                        percent *= percent;
                        return false;
                }

                private void PlayAudio (Audio audio, float percent)
                {
                        if (audioManager != null)
                        {
                                audioManager.PlaySFX(audio, percent);
                        }
                        else
                        {
                                AudioManager.get.PlaySFX(audio, percent);
                        }
                }

        }
}
