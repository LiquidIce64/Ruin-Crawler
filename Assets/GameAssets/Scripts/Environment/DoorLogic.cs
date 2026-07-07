using UnityEngine;

public class DoorLogic : MonoBehaviour
{
    [Header("Аниматор модели")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animParamName = "IsOpen";

    [Header("Текущее состояние (только для просмотра)")]
    [SerializeField] private bool isOpen = false;

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

        if (animator != null)
        {
            animator.SetBool(animParamName, true);
        }
        Debug.Log("Ворота: ОТКРЫВАЮТСЯ");
    }

    [ContextMenu("Закрыть ворота (Тест)")]
    public void CloseDoor()
    {
        if (!isOpen) return;
        isOpen = false;

        if (animator != null)
        {
            animator.SetBool(animParamName, false);
        }
        Debug.Log("Ворота: ЗАКРЫВАЮТСЯ");
    }

    public void ToggleDoor()
    {
        if (isOpen) CloseDoor();
        else OpenDoor();
    }
}