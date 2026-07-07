using UnityEngine;

public class Hints : MonoBehaviour
{
    public GameObject hintsPanel;

    void Start()
    {
        bool hintsShown = GetHintsShown();

        if (hintsPanel != null)
            hintsPanel.SetActive(hintsShown);
    }

    private bool GetHintsShown()
    {
        if (PlayerPrefs.HasKey("HintsShownPreference"))
            return System.Convert.ToBoolean(PlayerPrefs.GetInt("HintsShownPreference"));

        return true;
    }
}