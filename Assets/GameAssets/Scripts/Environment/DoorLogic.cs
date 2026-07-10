using UnityEngine;

public class DoorLogic : MonoBehaviour
{
    [Header("Аниматор модели")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animParamName = "IsOpen";

    [Header("Текущее состояние")]
    [SerializeField] private bool isOpen = false;

    [Header("Звук")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip doorSound;

    private void Start()
    {
        if (animator != null)
        {
            animator.SetBool(animParamName, isOpen);
        }
    }

    [ContextMenu("Открыть ворота (Тест)")]
    public void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;
        if (animator != null) animator.SetBool(animParamName, true);
        PlayDoorSound();
        Debug.Log("Ворота: ОТКРЫВАЮТСЯ");
    }

    [ContextMenu("Закрыть ворота (Тест)")]
    public void CloseDoor()
    {
        if (!isOpen) return;
        isOpen = false;
        if (animator != null) animator.SetBool(animParamName, false);
        PlayDoorSound();
        Debug.Log("Ворота: ЗАКРЫВАЮТСЯ");
    }

    public void ToggleDoor()
    {
        if (isOpen) CloseDoor();
        else OpenDoor();
    }

    private void PlayDoorSound()
    {
        if (audioSource != null && doorSound != null)
        {
            audioSource.PlayOneShot(doorSound);
        }
    }
}