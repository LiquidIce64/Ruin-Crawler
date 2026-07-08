using UnityEngine;
using System.Collections;

public class DoorLogic : MonoBehaviour
{
    [Header("Аниматор модели")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animParamName = "IsOpen";

    [Header("Текущее состояние")]
    [SerializeField] private bool isOpen = false;

    [Header("Время движения")]
    [SerializeField] private float animationDuration = 1.5f;

    private bool isMoving = false;

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
        // Если дверь уже открыта или она сейчас в процессе движения - игнорируем клик
        if (isOpen || isMoving) return;

        StartCoroutine(DoorMovementRoutine(true));
    }

    [ContextMenu("Закрыть ворота (Тест)")]
    public void CloseDoor()
    {
        if (!isOpen || isMoving) return;

        StartCoroutine(DoorMovementRoutine(false));
    }

    public void ToggleDoor()
    {
        if (isOpen) CloseDoor();
        else OpenDoor();
    }

    private IEnumerator DoorMovementRoutine(bool targetState)
    {
        isMoving = true;
        isOpen = targetState;

        if (animator != null)
        {
            animator.SetBool(animParamName, targetState);
        }

        Debug.Log(targetState ? "Ворота: ОТКРЫВАЮТСЯ" : "Ворота: ЗАКРЫВАЮТСЯ");

        // Ждем ровно столько секунд, сколько длится анимация
        yield return new WaitForSeconds(animationDuration);

        isMoving = false;
    }
}