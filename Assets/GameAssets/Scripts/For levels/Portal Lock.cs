using UnityEngine;

[RequireComponent(typeof(Portal))]
public class PortalLock : MonoBehaviour
{
    [SerializeField] private int levelsRequired = 0;
    [SerializeField] private GameObject lockedText;

    private void Start()
    {
        bool unlocked = SaveManager.LevelsCompleted >= levelsRequired;
        GetComponent<Portal>().enabled = unlocked;
        lockedText.SetActive(!unlocked);
    }
}
