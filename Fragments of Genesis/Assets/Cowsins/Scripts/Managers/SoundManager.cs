using UnityEngine;
using System.Collections;
namespace cowsins2D
{
    public class SoundManager : MonoBehaviour
    {
        // Singleton pattern to ensure that there is only one SoundManager instance in the game.
        public static SoundManager Instance;

        // The AudioSource that will be used to play all sounds.
        private AudioSource source;

        private void Awake()
        {
            // If there is no existing SoundManager instance, then set this instance as the singleton.
            if (Instance == null)
            {
                Instance = this;
            }
            // Otherwise, destroy this instance, since there can only be one SoundManager.
            else
            {
                Destroy(this.gameObject);
            }

            // Get the AudioSource component on this GameObject.
            source = GetComponent<AudioSource>();
        }

        public void PlaySound(AudioClip clip, float volume)
        {
            if(clip != null) StartCoroutine(Play(clip, volume));
        }

        private IEnumerator Play(AudioClip clip, float volume)
        {
            yield return new WaitForSeconds(.001f);
            source.volume = volume;
            source.PlayOneShot(clip);
            yield return null;
        }
    }
}
