using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [CreateAssetMenu (menuName = "FlareEngine/AudioManagerSO")]
        public class AudioManagerSO : ScriptableObject
        {
                private AudioManager audioRef;

                public void Register (AudioManager audio)
                {
                        this.audioRef = audio;
                }

                public void PlayAudio (string audioName)
                {
                        if (audioRef == null) return;
                        audioRef.PlayAudio (audioName);
                }

                public void FadeInMusic (string musicName)
                {
                        if (audioRef == null) return;
                        audioRef.FadeInMusic (musicName);
                }

                public void FadeOutMusic ( )
                {
                        if (audioRef == null) return;
                        audioRef.FadeOutMusic ( );
                }

                public void FadeToNewMusic (string musicName)
                {
                        if (audioRef == null) return;
                        audioRef.FadeToNewMusic (musicName);
                }

                public void MusicVolume (float value)
                {
                        if (audioRef == null) return;
                        audioRef.MusicVolume (value);
                }

                public void SFXVolume (float value)
                {
                        if (audioRef == null) return;
                        audioRef.SFXVolume (value);
                }

                public void MasterMusicVolume (float value)
                {
                        if (audioRef == null) return;
                        audioRef.MasterMusicVolume (value);
                }

                public void MasterSFXVolume (float value)
                {
                        if (audioRef == null) return;
                        audioRef.MasterSFXVolume (value);
                }

                public void StopMusic ( )
                {
                        if (audioRef == null) return;
                        audioRef.StopMusic ( );
                }

                public void StopSFX ( )
                {
                        if (audioRef == null) return;
                        audioRef.StopSFX ( );
                }

                public void PauseAllAudio ( )
                {
                        if (audioRef == null) return;
                        audioRef.PauseAllAudio ( );
                }

                public void UnpauseAllAudio ( )
                {
                        if (audioRef == null) return;
                        audioRef.UnpauseAllAudio ( );
                }

                public void StopAllAudio ( )
                {
                        if (audioRef == null) return;
                        audioRef.StopAllAudio ( );
                }

        }
}