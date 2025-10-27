using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

namespace cowsins2D
{
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private AudioMixer masterMixer;
        [SerializeField] private TMP_Dropdown framerateDropdown;
        [SerializeField] private List<int> framerates;
        [SerializeField] private Toggle vsyncToggle;
        [SerializeField] private Toggle fullscreenToggle; 

        private Resolution[] resolutions;

        public static SettingsManager instance;

        private void Start()
        {
            // We need to mantain references, thats why we delete the existing instance and override it with this one.
            if(instance != null) Destroy(instance.gameObject);

            instance = this;
            DontDestroyOnLoad(this);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Initialize the resolution dropdown
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();
            int currentResolutionIndex = 0;

            List<string> resolutionOptions = new List<string>();
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + "x" + resolutions[i].height;
                resolutionOptions.Add(option);

                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            resolutionDropdown.AddOptions(resolutionOptions);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();

            framerateDropdown.ClearOptions();
            List<string> framerateOptions = new List<string>();
            foreach (int framerate in framerates)
            {
                framerateOptions.Add(framerate.ToString());
            }
            framerateDropdown.AddOptions(framerateOptions);

            // Set the volume slider's initial value
            volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1.0f);

            // Load saved settings
            LoadSettings();
        }

        private void Update()
        {
            // Set the volume slider's value based on the current audio volume
            float masterVolume;
            if (masterMixer && masterMixer.GetFloat("Volume", out masterVolume))
                SetVolume(volumeSlider.value);
        }

        public void SetResolution(int resolutionIndex)
        {
            Resolution selectedResolution = resolutions[resolutionIndex];
            Screen.SetResolution(selectedResolution.width, selectedResolution.height, fullscreenToggle.isOn); // Apply fullscreen setting
            PlayerPrefs.SetInt("ResolutionWidth", selectedResolution.width);
            PlayerPrefs.SetInt("ResolutionHeight", selectedResolution.height);
            PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0); // Save fullscreen setting
        }

        public void SetFullscreen(bool isFullscreen)
        {
            // Set fullscreen mode
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0); // Save fullscreen setting
        }

        public void SetVolume(float volume)
        {
            // Update the audio volume
            masterMixer.SetFloat("Volume", Mathf.Log10(volume) * 20);

            // Save the volume setting
            PlayerPrefs.SetFloat("MasterVolume", volume);
        }

        public void SetFramerate(int framerateIndex)
        {
            int targetFramerate = int.Parse(framerateDropdown.options[framerateIndex].text);
            Application.targetFrameRate = targetFramerate;
            PlayerPrefs.SetInt("TargetFramerate", targetFramerate);
        }

        public void SetVSync(bool vsync)
        {
            QualitySettings.vSyncCount = vsync ? 1 : 0;
            PlayerPrefs.SetInt("VSync", vsync ? 1 : 0);
        }

        public void SaveSettings()
        {
            // Save resolution settings
            PlayerPrefs.SetInt("ResolutionWidth", resolutions[resolutionDropdown.value].width);
            PlayerPrefs.SetInt("ResolutionHeight", resolutions[resolutionDropdown.value].height);
            PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);

            // Save volume setting
            PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);

            // Save target framerate setting
            PlayerPrefs.SetInt("TargetFramerate", int.Parse(framerateDropdown.options[framerateDropdown.value].text));

            // Save VSync setting
            PlayerPrefs.SetInt("VSync", vsyncToggle.isOn ? 1 : 0);

            PlayerPrefs.Save();

            LoadSettings();
        }


        public void LoadSettings()
        {
            int width = PlayerPrefs.GetInt("ResolutionWidth", Screen.currentResolution.width);
            int height = PlayerPrefs.GetInt("ResolutionHeight", Screen.currentResolution.height);
            SetResolution(GetResolutionIndex(width, height));
            bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            fullscreenToggle.isOn = isFullscreen;
            SetFullscreen(isFullscreen);

            float volume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
            volumeSlider.value = volume;
            SetVolume(volume);

            int targetFramerate = PlayerPrefs.GetInt("TargetFramerate", 60);
            framerateDropdown.value = GetFramerateIndex(targetFramerate);
            SetFramerate(framerateDropdown.value);

            bool vsync = PlayerPrefs.GetInt("VSync", 1) == 1;
            vsyncToggle.isOn = vsync;
            SetVSync(vsync);
        }

        private int GetResolutionIndex(int width, int height)
        {
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == width && resolutions[i].height == height)
                {
                    return i;
                }
            }
            return 0;
        }

        private int GetFramerateIndex(int framerate)
        {
            for (int i = 0; i < framerateDropdown.options.Count; i++)
            {
                if (int.Parse(framerateDropdown.options[i].text) == framerate)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
