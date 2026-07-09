using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public AudioMixer audioMixer;
    public TMP_Dropdown resolutionDropdown;
    public Slider masterVolumeSlider;
    public Slider soundVolumeSlider;
    public Slider musicVolumeSlider;
    public Toggle fullscreenToggle;
    public Toggle hintsShownToggle;
    public Slider cameraSensitivitySlider;
    public Slider zoomSensitivitySlider;

    public GameObject hintsPanel;
    public PlayerController playerController;

    float currentCameraSensitivity;
    float currentZoomSensitivity;
    Resolution[] resolutions;

    void Start()
    {
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        resolutions = Screen.resolutions;
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height + " " + resolutions[i].refreshRate + "Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();

        LoadSettings(currentResolutionIndex);
    }

    private void SetVolume(float volume, string param)
    {
        if (audioMixer != null)
            audioMixer.SetFloat(param, volume);
    }
    public void SetMasterVolume(float volume) => SetVolume(volume, "Master Volume");
    public void SetSoundVolume(float volume) => SetVolume(volume, "Sound Volume");
    public void SetMusicVolume(float volume) => SetVolume(volume, "Music Volume");

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = isFullscreen;
    }

    public void SetHintsShown(bool hintsShown)
    {
        if (hintsShownToggle != null)
            hintsShownToggle.isOn = hintsShown;

        if (hintsPanel != null)
            hintsPanel.SetActive(hintsShown);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];

        Screen.SetResolution(
            resolution.width,
            resolution.height,
            Screen.fullScreen
        );
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetCameraSensitivity(float sensitivity)
    {
        currentCameraSensitivity = sensitivity;

        if (playerController != null)
            playerController.SetCameraSensitivity(sensitivity);
    }

    public void SetZoomSensitivity(float sensitivity)
    {
        currentZoomSensitivity = sensitivity;

        if (playerController != null)
            playerController.SetZoomSensitivity(sensitivity);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionPreference", resolutionDropdown.value);
        PlayerPrefs.SetInt("FullscreenPreference", System.Convert.ToInt32(fullscreenToggle.isOn));
        PlayerPrefs.SetInt("HintsShownPreference", System.Convert.ToInt32(hintsShownToggle.isOn));

        audioMixer.GetFloat("Master Volume", out float volume);
        PlayerPrefs.SetFloat("MasterVolumePreference", volume);
        audioMixer.GetFloat("Sound Volume", out volume);
        PlayerPrefs.SetFloat("SoundVolumePreference", volume);
        audioMixer.GetFloat("Music Volume", out volume);
        PlayerPrefs.SetFloat("MusicVolumePreference", volume);

        PlayerPrefs.SetFloat("CameraSensitivityPreference", currentCameraSensitivity);
        PlayerPrefs.SetFloat("ZoomSensitivityPreference", currentZoomSensitivity);

        PlayerPrefs.Save();
    }

    public void LoadSettings(int currentResolutionIndex)
    {
        if (PlayerPrefs.HasKey("ResolutionPreference"))
            resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionPreference");
        else
            resolutionDropdown.value = currentResolutionIndex;

        bool fullscreenValue;

        if (PlayerPrefs.HasKey("FullscreenPreference"))
            fullscreenValue = System.Convert.ToBoolean(PlayerPrefs.GetInt("FullscreenPreference"));
        else
            fullscreenValue = true;

        Screen.fullScreen = fullscreenValue;

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = fullscreenValue;

        bool hintsShownValue;

        if (PlayerPrefs.HasKey("HintsShownPreference"))
            hintsShownValue = System.Convert.ToBoolean(PlayerPrefs.GetInt("HintsShownPreference"));
        else
            hintsShownValue = true;

        SetHintsShown(hintsShownValue);

        if (PlayerPrefs.HasKey("MasterVolumePreference"))
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolumePreference");
        else
            masterVolumeSlider.value = 0f;

        SetMasterVolume(masterVolumeSlider.value);

        if (PlayerPrefs.HasKey("SoundVolumePreference"))
            soundVolumeSlider.value = PlayerPrefs.GetFloat("SoundVolumePreference");
        else
            soundVolumeSlider.value = 0f;

        SetSoundVolume(soundVolumeSlider.value);

        if (PlayerPrefs.HasKey("MusicVolumePreference"))
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolumePreference");
        else
            musicVolumeSlider.value = 0f;

        SetMusicVolume(musicVolumeSlider.value);

        if (PlayerPrefs.HasKey("CameraSensitivityPreference"))
            cameraSensitivitySlider.value = PlayerPrefs.GetFloat("CameraSensitivityPreference");
        else
            cameraSensitivitySlider.value = 0.5f;

        SetCameraSensitivity(cameraSensitivitySlider.value);

        if (PlayerPrefs.HasKey("ZoomSensitivityPreference"))
            zoomSensitivitySlider.value = PlayerPrefs.GetFloat("ZoomSensitivityPreference");
        else
            zoomSensitivitySlider.value = 1.0f;
        
        SetZoomSensitivity(zoomSensitivitySlider.value);
    }

    public void RevertUnsavedSettings()
    {
        int currentResolutionIndex = 0;

        if (resolutions != null && resolutions.Length > 0)
        {
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                    break;
                }
            }
        }

        LoadSettings(currentResolutionIndex);

        SetResolution(resolutionDropdown.value);
        SetMasterVolume(masterVolumeSlider.value);
        SetSoundVolume(soundVolumeSlider.value);
        SetMusicVolume(musicVolumeSlider.value);
        SetFullscreen(fullscreenToggle.isOn);
        SetHintsShown(hintsShownToggle.isOn);
        SetCameraSensitivity(cameraSensitivitySlider.value);
        SetZoomSensitivity(zoomSensitivitySlider.value);
    }

    public bool GetHintsShown()
    {
        if (PlayerPrefs.HasKey("HintsShownPreference"))
            return System.Convert.ToBoolean(PlayerPrefs.GetInt("HintsShownPreference"));

        return true;
    }
}