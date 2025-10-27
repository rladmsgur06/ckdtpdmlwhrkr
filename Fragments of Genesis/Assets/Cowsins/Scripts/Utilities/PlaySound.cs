using UnityEngine;

namespace cowsins2D
{
    public class PlaySound : MonoBehaviour
    {
        [SerializeField, Tooltip("SFX to play.")] private AudioClip clip;

        [SerializeField, Range(0, 2), Tooltip("Volume to play the Sound")] private float volume;

        private void Start()
        {
            // Play sound on start
            SoundManager.Instance.PlaySound(clip, 1);
        }
    }
}