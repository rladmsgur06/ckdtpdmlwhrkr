using UnityEngine;
using UnityEngine.SceneManagement;

namespace cowsins2D
{
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }

        public static bool isPaused { get; private set; }

        [SerializeField] private PlayerControl playerControl;

        [SerializeField] private CanvasGroup menu;

        [SerializeField] private float fadeSpeed;

        private void OnEnable()
        {
            InputManager.Instance.onPause += TogglePause;
        }

        private void OnDisable()
        {
            InputManager.Instance.onPause -= TogglePause;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(this);
            else Instance = this;
        }

        private void Start()
        {
            isPaused = false;
            menu.gameObject.SetActive(false);
            menu.alpha = 0;
            UnPause();
        }
        private void Update()
        {
            if (isPaused)
            {
                if (!menu.gameObject.activeSelf)
                {
                    menu.gameObject.SetActive(true);
                    menu.alpha = 0;
                }
                if (menu.alpha < 1) menu.alpha += Time.deltaTime * fadeSpeed;
            }
            else
            {
                menu.alpha -= Time.deltaTime * fadeSpeed;
                if (menu.alpha <= 0) menu.gameObject.SetActive(false);
            }
        }

        public void QuitGame() => Application.Quit();

        public void TogglePause()
        {
            if (!isPaused) Pause();
            else UnPause(); 
        }

        public void Pause()
        {
            isPaused = true;
            playerControl.LoseControl();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Crosshair.Instance.Hide(true);
        }

        public void UnPause()
        {
            isPaused = false;
            playerControl.CheckIfCanGrantControl();
            if (Crosshair.Instance.CheckIfCanShow()) Crosshair.Instance.Show();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }

        public void LoadScene(int id)
        {
            SceneManager.LoadScene(id);
        }
    }
}
