using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Settings : MonoBehaviour
{
    public AudioMixer audioMixer;
    public TMP_Dropdown resolutionDropdown;
    public Slider volumeSlider;
    public Toggle fullscreenToggle;
    public Toggle hintsShownToggle;
    public GameObject hintsPanel;

    float currentVolume;
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

    public void SetVolume(float volume)
    {
        currentVolume = volume;
        audioMixer.SetFloat("Volume", volume);
    }

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

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionPreference", resolutionDropdown.value);
        PlayerPrefs.SetInt("FullscreenPreference", System.Convert.ToInt32(fullscreenToggle.isOn));
        PlayerPrefs.SetInt("HintsShownPreference", System.Convert.ToInt32(hintsShownToggle.isOn));
        PlayerPrefs.SetFloat("VolumePreference", currentVolume);

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

        if (PlayerPrefs.HasKey("VolumePreference"))
            volumeSlider.value = PlayerPrefs.GetFloat("VolumePreference");
        else
            volumeSlider.value = 100f;
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
        SetVolume(volumeSlider.value);
        SetFullscreen(fullscreenToggle.isOn);
        SetHintsShown(hintsShownToggle.isOn);
    }

    public bool GetHintsShown()
    {
        if (PlayerPrefs.HasKey("HintsShownPreference"))
            return System.Convert.ToBoolean(PlayerPrefs.GetInt("HintsShownPreference"));

        return true;
    }
}