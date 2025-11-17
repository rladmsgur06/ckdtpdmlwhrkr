using UnityEngine;
using UnityEngine.UI;

namespace TwoBitMachines
{
        public class UIAnimation : MonoBehaviour
        {
                [SerializeField] public Image image;
                [SerializeField] public Sprite[] sprites;
                [SerializeField] public float fps = 10f;

                private float counter = 0;
                private int frameIndex = 0;

                void OnEnable ( )
                {
                        counter = 0;
                        frameIndex = 0;
                }

                void Update ( )
                {
                        if (image == null) return;

                        float rate = 1f / fps;

                        if (Clock.Timer (ref counter, rate))
                        {
                                frameIndex = frameIndex + 1 >= sprites.Length? 0 : frameIndex + 1;
                                image.sprite = sprites[frameIndex];
                        }

                }
        }
}