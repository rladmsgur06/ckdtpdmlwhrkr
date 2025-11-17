using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [AddComponentMenu ("Flare Engine/一Interactables/FoliageSway")]
        public class FoliageSway : MonoBehaviour
        {
                [SerializeField] public SwayDirection direction;
                [SerializeField] public bool isRandom;

                [SerializeField] public float amplitudeMin = 0.25f;
                [SerializeField] public float amplitudeMax = 1f;
                [SerializeField] public float frequencyMin = 1f;
                [SerializeField] public float frequencyMax = 5f;
                [SerializeField] public float speedMin = 1f;
                [SerializeField] public float speedMax = 3f;

                [System.NonSerialized] private SpriteRenderer render;
                [System.NonSerialized] private MaterialPropertyBlock block;

                private void Start ( )
                {
                        block = new MaterialPropertyBlock ( );
                        render = GetComponent<SpriteRenderer> ( );
                        Set ( );
                }

                private void Set ( )
                {
                        block.SetTexture ("_MainTex", (Texture2D) render.sprite.texture); // need to set or sprite will turn into a colored block
                        block.SetFloat ("_Direction", (float) direction);
                        block.SetFloat ("_Amplitude", isRandom ? Random.Range (amplitudeMin, amplitudeMax) : amplitudeMin);
                        block.SetFloat ("_Frequency", isRandom ? Random.Range (frequencyMin, frequencyMax) : frequencyMin);
                        block.SetFloat ("_Speed", isRandom ? Random.Range (speedMin, speedMax) : speedMin);
                        render.SetPropertyBlock (block);
                }

                public void ChangeAmplitude (float value)
                {
                        amplitudeMin = value;
                        Set ( );
                }

                public void ChangeFrequency (float value)
                {
                        frequencyMin = value;
                        Set ( );
                }

                public void ChangeSpeed (float value)
                {
                        speedMin = value;
                        Set ( );
                }

                public void ChangeDirection (int value)
                {
                        direction = (SwayDirection) value;
                        Set ( );
                }
        }

        public enum SwayDirection
        {
                bottom = 0,
                top = 1,
                left = 2,
                right = 3
        }
}