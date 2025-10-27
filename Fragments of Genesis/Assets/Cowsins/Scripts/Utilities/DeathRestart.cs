using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace cowsins2D
{
    public class DeathRestart : MonoBehaviour
    {
        [SerializeField] private Animator container;

        private void Start()
        {
            container.gameObject.SetActive(false);
        }

        private void Update()
        {
            // Avoid showing the death screen if there exists a checkpoint already
            // Avoid running the reload code if container is not active either
            if (CheckPointManager.Instance.lastCheckpoint != null || !container.gameObject.activeInHierarchy) return;

            if (Keyboard.current.rKey.wasPressedThisFrame) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ShowDeathScreen()
        {
            if(CheckPointManager.Instance.lastCheckpoint == null)
            {
                container.gameObject.SetActive(true);
                container.SetTrigger("PlayDeath"); 
            }
        }
    }
}