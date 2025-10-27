using UnityEngine;
using UnityEngine.UI; 

namespace cowsins2D
{
    public class SeamlessScroll : MonoBehaviour
    {
        [SerializeField, Tooltip("Image to apply the effect on.")] private RawImage image;
        [SerializeField, Tooltip("Vector2 that represents the movement of the scroll.")] private Vector2 movement;

        private void Update()
        {
            image.uvRect = new Rect(image.uvRect.position + movement * Time.deltaTime, image.uvRect.size);
        }
    }

}