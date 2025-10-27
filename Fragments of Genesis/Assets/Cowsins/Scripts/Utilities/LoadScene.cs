using UnityEngine;
using UnityEngine.SceneManagement;

namespace cowsins2D
{
    public class LoadScene : MonoBehaviour
    {
        [SerializeField, Tooltip("Order ID of the scene you want to load. Notice that this scene should be included in the Build Settings. " +
            "You will find the order ID of each scene in the Build Settings. ")]
        private int sceneOrderID;
        public void LoadTheScene()
        {
            // Load the right scene
            SceneManager.LoadScene(sceneOrderID);
        }
    }
}