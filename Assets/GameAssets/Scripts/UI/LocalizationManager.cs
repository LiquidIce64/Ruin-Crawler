using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LocalizationManager
{
    public const int EnglishLanguageIndex = 0;
    public const int RussianLanguageIndex = 1;
    public const string LanguagePreferenceKey = "LanguagePreference";

    private static readonly Dictionary<string, string> EnglishByRussianText = new()
    {
        { "ИГРАТЬ", "PLAY" },
        { "НАСТРОЙКИ", "SETTINGS" },
        { "ВЫХОД", "EXIT" },
        { "ПРОДОЛЖИТЬ", "RESUME" },
        { "ГЛАВНОЕ МЕНЮ", "MAIN MENU" },
        { "ВЫБОР УРОВНЯ", "LEVEL SELECT" },
        { "РЕСТАРТ", "RESTART" },
        { "Машина уничтожена", "Vehicle destroyed" },
        { "ЗАНОВО", "RESTART" },
        { "перемещение", "movement" },
        { "Соединена", "Attached" },
        { "Отсоединена", "Detached" },
        { "Задняя", "Rear" },
        { "Передняя", "Front" },
        { "НАСТРОЙКИ ГРАФИКИ", "GRAPHICS SETTINGS" },
        { "РАЗРЕШЕНИЕ ЭКРАНА", "SCREEN RESOLUTION" },
        { "ПОЛНОЭКРАННЫЙ РЕЖИМ", "FULLSCREEN" },
        { "ПОКАЗЫВАТЬ ПОДСКАЗКИ", "SHOW HINTS" },
        { "ГРОМКОСТЬ", "VOLUME" },
        { "ГРОМКОСТЬ ЗВУКОВ", "SOUND VOLUME" },
        { "ГРОМКОСТЬ МУЗЫКИ", "MUSIC VOLUME" },
        { "ЧУВСТВИТЕЛЬНОСТЬ КАМЕРЫ", "CAMERA SENSITIVITY" },
        { "ЧУВСТВИТЕЛЬНОСТЬ ПРИБЛИЖЕНИЯ", "ZOOM SENSITIVITY" },
        { "СОХРАНИТЬ", "SAVE" },
        { "ОТКАТИТЬ", "REVERT" },
        { "НАЗАД", "BACK" },
        { "ЯЗЫК", "LANGUAGE" },
        { "Туториал", "Tutorial" },
        { "Недоступно", "Unavailable\n\n\n\n" },
        { "Уровень 0 Платформа", "Level 0\n\nPlatform" },
        { "Уровень 1 Нажимная плита", "Level 1\n\nPressure Plate" },
        { "Уровень 2 Движение на лебёдках", "Level 2\n\nWinch Movement" },
        { "Уровень 3 Полёт на лебёдке", "Level 3\n\nWinch Flight" },
        { "Уровень 4 Сброс коробки", "Level 4\n\nBox Drop" },
        { "Уровень 5 Дверной замок", "Level 5\n\nDoor Lock" },
        { "Уровень 6 Лабиринт (сложный уровень)", "Level 6\n\nLabyrinth\n(Hard Level)" },
        {
            "торможение ускорение прыжок передняя лебёдка задняя лебёдка",
            "      brake\n       accelerate\n        jump\n     front winch\n     rear winch\n"
        },
    };

    private static readonly Dictionary<string, string> RussianByEnglishText = new();
    private static readonly Dictionary<int, string> SourceRussianTextByInstanceId = new();
    private static int currentLanguageIndex = PlayerPrefs.GetInt(LanguagePreferenceKey, EnglishLanguageIndex);

    static LocalizationManager()
    {
        foreach (KeyValuePair<string, string> pair in EnglishByRussianText)
            RussianByEnglishText[Normalize(pair.Value)] = pair.Key;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        RefreshAllTexts();
    }

    public static int CurrentLanguageIndex => currentLanguageIndex;

    public static string CurrentLanguageCode => CurrentLanguageIndex == EnglishLanguageIndex ? "en" : "ru";

    public static void SetLanguage(int languageIndex)
    {
        currentLanguageIndex = Mathf.Clamp(languageIndex, EnglishLanguageIndex, RussianLanguageIndex);
        RefreshAllTexts();
    }

    public static void SaveCurrentLanguage()
    {
        PlayerPrefs.SetInt(LanguagePreferenceKey, currentLanguageIndex);
    }

    public static string Translate(string russianText)
    {
        return Translate(russianText, CurrentLanguageIndex);
    }

    public static string Translate(string russianText, int languageIndex)
    {
        if (languageIndex == RussianLanguageIndex)
            return russianText;

        return EnglishByRussianText.TryGetValue(Normalize(russianText), out string englishText)
            ? englishText
            : russianText;
    }

    public static void RefreshAllTexts()
    {
        TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();

        foreach (TMP_Text text in texts)
        {
            if (text == null || !text.gameObject.scene.IsValid())
                continue;

            if (IsDropdownInternalText(text))
                continue;

            LocalizeText(text);
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshAllTexts();
    }

    private static void LocalizeText(TMP_Text text)
    {
        int instanceId = text.GetInstanceID();

        if (!SourceRussianTextByInstanceId.TryGetValue(instanceId, out string sourceRussianText))
        {
            sourceRussianText = GetSourceRussianText(text.text);
            SourceRussianTextByInstanceId[instanceId] = sourceRussianText;
        }

        text.text = Translate(sourceRussianText);
    }

    private static bool IsDropdownInternalText(TMP_Text text)
    {
        return text.GetComponentInParent<TMP_Dropdown>(true) != null;
    }

    private static string GetSourceRussianText(string currentText)
    {
        string normalizedText = Normalize(currentText);

        if (EnglishByRussianText.ContainsKey(normalizedText))
            return currentText;

        if (RussianByEnglishText.TryGetValue(normalizedText, out string russianText))
            return russianText;

        return currentText;
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return string.Join(" ", value.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries));
    }
}
