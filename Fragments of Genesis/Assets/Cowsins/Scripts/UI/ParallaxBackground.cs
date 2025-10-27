using UnityEngine;

namespace cowsins2D
{
    public class ParallaxBackground : MonoBehaviour
    {
        private float length, startPos;

        [SerializeField, Tooltip("Reference to the player camera.")] private GameObject cam;

        [SerializeField, Tooltip("Amount of movement allowed for the background.")] private float parallaxStrength;

        private void Start()
        {
            startPos = transform.position.x;

            length = GetComponent<SpriteRenderer>().bounds.size.x;
        }

        private void Update()
        {
            float t = cam.transform.position.x * (1 - parallaxStrength);
            float distance = cam.transform.position.x * parallaxStrength;
            transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);

            if (t > startPos + length) startPos += length;
            else if (t < startPos - length) startPos -= length;
        }

    }

}