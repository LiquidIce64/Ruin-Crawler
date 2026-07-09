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
    public TMP_Dropdown languageDropdown;
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
    int currentLanguageIndex = LocalizationManager.RussianLanguageIndex;
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

        InitializeLanguageDropdown();
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

    public void SetLanguage(int languageIndex)
    {
        currentLanguageIndex = Mathf.Clamp(
            languageIndex,
            LocalizationManager.EnglishLanguageIndex,
            LocalizationManager.RussianLanguageIndex
        );

        LocalizationManager.SetLanguage(currentLanguageIndex);
        RefreshLanguageDropdownValue();
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
        LocalizationManager.SaveCurrentLanguage();

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

        int languageValue = PlayerPrefs.GetInt(
            LocalizationManager.LanguagePreferenceKey,
            LocalizationManager.RussianLanguageIndex
        );
        SetLanguage(languageValue);

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
        SetLanguage(currentLanguageIndex);
        SetCameraSensitivity(cameraSensitivitySlider.value);
        SetZoomSensitivity(zoomSensitivitySlider.value);
    }

    public static string GetLanguageCode()
    {
        return LocalizationManager.CurrentLanguageCode;
    }

    private void InitializeLanguageDropdown()
    {
        if (languageDropdown == null)
            languageDropdown = CreateLanguageDropdown();

        if (languageDropdown == null)
            return;

        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(new List<string> { "English", "Русский" });
        languageDropdown.SetValueWithoutNotify(PlayerPrefs.GetInt(
            LocalizationManager.LanguagePreferenceKey,
            LocalizationManager.RussianLanguageIndex
        ));
        languageDropdown.RefreshShownValue();
        languageDropdown.onValueChanged.RemoveListener(SetLanguage);
        languageDropdown.onValueChanged.AddListener(SetLanguage);
    }

    private TMP_Dropdown CreateLanguageDropdown()
    {
        if (resolutionDropdown == null)
            return null;

        Transform parent = resolutionDropdown.transform.parent;
        TMP_Dropdown dropdown = Instantiate(resolutionDropdown, parent);
        dropdown.name = "LanguageDropdown";
        dropdown.onValueChanged = new TMP_Dropdown.DropdownEvent();
        dropdown.GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, -14f);

        CreateLanguageLabel(parent);

        return dropdown;
    }

    private void CreateLanguageLabel(Transform parent)
    {
        TextMeshProUGUI resolutionLabel = null;

        foreach (TextMeshProUGUI text in parent.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.name == "ResolutionText")
            {
                resolutionLabel = text;
                break;
            }
        }

        if (resolutionLabel == null)
            return;

        TextMeshProUGUI label = Instantiate(resolutionLabel, parent);
        label.name = "LanguageText";
        label.text = "ЯЗЫК";
        label.rectTransform.anchoredPosition += new Vector2(0f, -14f);
    }

    private void RefreshLanguageDropdownValue()
    {
        if (languageDropdown == null)
            return;

        languageDropdown.SetValueWithoutNotify(currentLanguageIndex);
        languageDropdown.RefreshShownValue();
    }

    public bool GetHintsShown()
    {
        if (PlayerPrefs.HasKey("HintsShownPreference"))
            return System.Convert.ToBoolean(PlayerPrefs.GetInt("HintsShownPreference"));

        return true;
    }
}
