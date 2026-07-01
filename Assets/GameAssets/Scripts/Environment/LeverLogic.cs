using UnityEngine;
using UnityEngine.Events;

public class LeverLogic : MonoBehaviour
{
    [Header("Аниматор модели")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animParamName = "IsOn";

    [Header("События")]
    public UnityEvent OnActivated;   // Что делаем при включении
    public UnityEvent OnDeactivated; // Что делаем при выключении

    [Header("Текущее состояние (только для просмотра)")]
    [SerializeField] private bool isOn = false;

    [ContextMenu("Дернуть рычаг (Тест)")]
    // Вызывается из PlayerController
    public void Interact()
    {
        isOn = !isOn; // Переключаем состояние

        if (animator != null)
        {
            animator.SetBool(animParamName, isOn);
        }

        if (isOn)
        {
            OnActivated.Invoke();
            Debug.Log("Рычаг: ВКЛ");
        }
        else
        {
            OnDeactivated.Invoke();
            Debug.Log("Рычаг: ВЫКЛ");
        }
    }
}