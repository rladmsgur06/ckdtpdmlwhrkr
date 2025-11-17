using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("Flare Engine/一Audio/AudioManager")]
        public class AudioManager : MonoBehaviour
        {
                [SerializeField] private float musicMasterVolume = 1f;
                [SerializeField] private float sfxMasterVolume = 1f;
                [SerializeField] private float fadeInTime = 1f;
                [SerializeField] private float fadeOutTime = 1f;
                [SerializeField] private bool playOnStart;
                [SerializeField] private bool isMainManager = true;
                [SerializeField] private string startMusic = "Music Name";
                [SerializeField] private AudioSource music;
                [SerializeField] private AudioSource sfx;
                [SerializeField] private AudioManagerSO audioManagerSO;
                [SerializeField] private List<AudioCategory> categories = new List<AudioCategory>();

                [System.NonSerialized] private Dictionary<string, Audio> playList = new Dictionary<string, Audio>();
                [System.NonSerialized] private bool fadeIn = false;
                [System.NonSerialized] private bool fadeOut = false;
                [System.NonSerialized] private bool crossFade = false;
                [System.NonSerialized] private bool lerpVolumeUp = false;
                [System.NonSerialized] private bool lerpVolumeDown = false;
                [System.NonSerialized] private float oldValueMusicDown;
                [System.NonSerialized] private float oldValueSFXDown;
                [System.NonSerialized] private float oldValueMusicUp;
                [System.NonSerialized] private float oldValueSFXUp;
                [System.NonSerialized] private float targetMusicUp;
                [System.NonSerialized] private float targetSFXDown;
                [System.NonSerialized] private float targetSFXUp;
                [System.NonSerialized] private float targetMusicDown;
                [System.NonSerialized] private float lerpCounter;
                [System.NonSerialized] private float fade = 0;
                [System.NonSerialized] private float fadeCounter = 0;
                [System.NonSerialized] private float musicVolumeRef = 1f;
                [System.NonSerialized] private string newMusicName = "";
                [System.NonSerialized] public static AudioManager get;
                [System.NonSerialized] public static Audio tempAudio = new Audio();

                public float musicVolume => musicMasterVolume;
                public float sfxVolume => sfxMasterVolume;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] public bool loadAudio = false;
                [SerializeField] public bool active = false;
                [SerializeField] public int signalIndex = -1;
#pragma warning restore 0414
#endif
                #endregion

                public void Awake ()
                {
                        if (isMainManager)
                        {
                                get = this;
                        }
                        LoadAudio();
                        if (music == null)
                        {
                                music = gameObject.AddComponent<AudioSource>();
                        }
                        if (sfx == null)
                        {
                                sfx = gameObject.AddComponent<AudioSource>();
                        }

                        music.loop = true;
                        music.pitch = 1f;
                        music.priority = 0;
                        musicMasterVolume = PlayerPrefs.GetFloat("MusicVolume", musicMasterVolume);
                        sfxMasterVolume = PlayerPrefs.GetFloat("SFXVolume", sfxMasterVolume);
                        if (audioManagerSO != null)
                        {
                                audioManagerSO.Register(this);
                        }
                }

                private void Start ()
                {
                        if (playOnStart)
                        {
                                PlayAudio(startMusic);
                        }
                }
                public void Reset ()
                {
                        fadeIn = false;
                        fadeOut = false;
                        crossFade = false;
                        fadeCounter = 0;
                }
                public void Update ()
                {
                        if (fadeOut)
                        {
                                FadeOutMusicLerp();
                                if (!fadeOut && crossFade)
                                {
                                        crossFade = false;
                                        FadeInMusic(newMusicName);
                                }
                        }
                        if (fadeIn)
                        {
                                FadeInMusicLerp();
                        }
                        if (lerpVolumeDown)
                        {
                                lerpCounter += Time.deltaTime;
                                SetMusicVolume(Mathf.Lerp(oldValueMusicDown, targetMusicDown, lerpCounter / 1f));
                                SFXVolume(Mathf.Lerp(oldValueSFXDown, targetSFXDown, lerpCounter / 1f));
                                if (lerpCounter >= 1)
                                {
                                        lerpVolumeDown = false;
                                }
                        }
                        if (lerpVolumeUp)
                        {
                                lerpCounter += Time.deltaTime;
                                SetMusicVolume(Mathf.Lerp(oldValueMusicUp, targetMusicUp, lerpCounter / 1f));
                                SFXVolume(Mathf.Lerp(oldValueSFXUp, targetSFXUp, lerpCounter / 1f));
                                if (lerpCounter >= 1)
                                {
                                        lerpVolumeUp = false;
                                }
                        }
                }

                public void PlayAudio (string audioName)
                {
                        if (playList.ContainsKey(audioName))
                        {
                                Audio audio = playList[audioName];
                                if (audio.type == AudioType.Music)
                                {
                                        music.clip = audio.clip;
                                        SetMusicVolume(audio.volume);
                                        music.Play();
                                }
                                else
                                {
                                        audio = Probability(audio);
                                        sfx.PlayOneShot(audio.clip, sfxMasterVolume * audio.volume);
                                }
                        }
                }

                public void PlaySFX (Audio audio)
                {
                        audio = Probability(audio);
                        sfx.PlayOneShot(audio.clip, sfxMasterVolume * audio.volume);
                }

                public void PlaySFX (Audio audio, float percent)
                {
                        audio = Probability(audio);
                        sfx.PlayOneShot(audio.clip, sfxMasterVolume * audio.volume * percent);
                }

                public void FadeInMusic (string musicName)
                {
                        if (playList.ContainsKey(musicName))
                        {
                                Audio audio = playList[musicName];
                                if (audio.type == AudioType.Music)
                                {
                                        music.clip = audio.clip;
                                        SetMusicVolume(0);
                                        music.Play();

                                        fade = audio.volume;
                                        fadeCounter = 0;
                                        fadeIn = true;
                                        fadeOut = false;
                                }
                        }
                }

                public void FadeOutMusic ()
                {
                        if (music.isPlaying)
                        {
                                fade = musicVolumeRef;
                                fadeCounter = 0;
                                fadeOut = true;
                                fadeIn = false;
                        }
                }

                public void FadeToNewMusic (string musicName)
                {
                        if (music.isPlaying)
                        {
                                fade = musicVolumeRef;
                                fadeCounter = 0;
                                fadeOut = true;
                                fadeIn = false;
                                crossFade = true;
                                newMusicName = musicName;
                        }
                        else
                        {
                                FadeInMusic(musicName);
                        }
                }

                private void SetMusicVolume (float value)
                {
                        musicVolumeRef = value;
                        music.volume = musicMasterVolume * musicVolumeRef;
                }

                public void DecreaseVolume75Percent ()
                {
                        lerpVolumeDown = true;
                        lerpCounter = 0;
                        oldValueMusicDown = music.volume;
                        oldValueSFXDown = sfx.volume;
                        targetMusicDown = music.volume * 0.25f;
                        targetSFXDown = sfx.volume * 0.25f;
                }

                public void RevertAndIncreaseValue ()
                {
                        lerpVolumeUp = true;
                        lerpCounter = 0;
                        oldValueMusicUp = music.volume;
                        oldValueSFXUp = sfx.volume;
                        targetMusicUp = Mathf.Clamp01(music.volume * 4f);
                        targetSFXUp = Mathf.Clamp01(sfx.volume * 4f);
                }

                private void FadeInMusicLerp ()
                {
                        SetMusicVolume(Compute.LerpRunning(0, fade, fadeInTime, ref fadeCounter, ref fadeIn));
                }

                private void FadeOutMusicLerp ()
                {
                        SetMusicVolume(Compute.LerpRunning(fade, 0, fadeOutTime, ref fadeCounter, ref fadeOut));
                        if (!fadeOut)
                                StopMusic();
                }

                public void MusicVolume (float value)
                {
                        SetMusicVolume(Mathf.Clamp(value, 0, 1f));
                }

                public void SFXVolume (float value)
                {
                        sfxMasterVolume = Mathf.Clamp(value, 0, 1f);
                        sfx.volume = sfxMasterVolume;
                }

                public void MasterMusicVolume (float value)
                {
                        musicMasterVolume = Mathf.Clamp(value, 0, 1f);
                        SetMusicVolume(musicVolumeRef);
                        PlayerPrefs.SetFloat("MusicVolume", musicMasterVolume);
                        PlayerPrefs.Save();
                }

                public void MasterSFXVolume (float value)
                {
                        sfxMasterVolume = Mathf.Clamp(value, 0, 1f);
                        PlayerPrefs.SetFloat("SFXVolume", sfxMasterVolume);
                        PlayerPrefs.Save();
                }

                public void StopMusic ()
                {
                        music.Stop();
                }

                public void StopSFX ()
                {
                        sfx.Stop();
                }

                public void PauseAllAudio ()
                {
                        music.Pause();
                        sfx.Pause();
                }

                public void UnpauseAllAudio ()
                {
                        music.UnPause();
                        sfx.UnPause();
                }

                public void StopAllAudio ()
                {
                        music.Stop();
                        sfx.Stop();
                }

                private void LoadAudio ()
                {
                        playList.Clear();
                        for (int i = 0; i < categories.Count; i++)
                        {
                                List<Audio> audio = categories[i].audio;
                                for (int j = 0; j < audio.Count; j++)
                                {
                                        if (audio[j].clip == null)
                                                continue;
                                        if (!playList.ContainsKey(audio[j].clip.name))
                                        {
                                                playList.Add(audio[j].clip.name, audio[j]);
                                        }
                                }
                        }
                }

                public static Audio Probability (Audio audio)
                {
                        for (int i = 0; i < audio.childList.Count; i++)
                        {
                                float probability = Random.Range(0, 1f);
                                if (probability <= audio.childList[i].probability)
                                {
                                        return tempAudio.Set(audio.childList[i].clip, audio.childList[i].volume);
                                }
                        }
                        return audio;
                }

        }

        [System.Serializable]
        public class AudioCategory
        {
                [SerializeField] public List<Audio> audio = new List<Audio>();
                [SerializeField] public string name = "";

#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] private bool foldOut = false;
                [SerializeField] private bool deleteAsk = false;
                [SerializeField] private bool delete = false;
                [SerializeField] private bool add = false;
                [SerializeField] private bool active = false;
                [SerializeField] private int signalIndex = -1;
#pragma warning restore 0414
#endif
        }

        [System.Serializable]
        public class Audio
        {
                [SerializeField] public AudioClip clip;
                [SerializeField] public AudioType type;
                [SerializeField] public float volume = 0.5f;
                [SerializeField] public List<AudioChild> childList = new List<AudioChild>();

                public Audio Set (AudioClip clip, float volume)
                {
                        this.clip = clip;
                        this.volume = volume;
                        return this;
                }

#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] private bool foldOut = false;
#pragma warning restore 0414
#endif
        }

        [System.Serializable]
        public class AudioChild
        {
                [SerializeField] public AudioClip clip;
                [SerializeField] public float volume = 0.5f;
                [SerializeField] public float probability = 0.5f;
        }

        public enum AudioType
        {
                Music,
                SFX
        }
}
