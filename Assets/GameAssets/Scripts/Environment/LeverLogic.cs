using UnityEngine;
using UnityEngine.Events;

public class LeverLogic : MonoBehaviour, IWinchInteractable
{
    [Header("Аниматор модели")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animParamName = "IsOn";

    [Header("Настройки")]
    [Tooltip("Если включено, рычаг работает как кнопка (отключается при отпускании лебедки)")]
    [SerializeField] private bool isButtonMode = false;

    [Header("События")]
    public UnityEvent OnActivated;   // Что делаем при включении
    public UnityEvent OnDeactivated; // Что делаем при выключении

    [Header("Текущее состояние (только для просмотра)")]
    [SerializeField] private bool isOn = false;

    // Свойство из интерфейса.
    // Если это кнопка, мы должны держать лебедку (AutoDetach = false).
    // Если обычный тумблер - отцепляемся сразу (AutoDetach = true).
    public bool AutoDetach => !isButtonMode;

    [ContextMenu("Дернуть рычаг (Тест)")]
    // Вызывается из лебедки (или PlayerController)
    public void Interact()
    {
        if (isButtonMode)
        {
            // Режим кнопки: включаем (если еще не включен)
            if (!isOn) ChangeState(true);
        }
        else
        {
            // Режим тумблера: просто переключаем состояние туда-сюда
            ChangeState(!isOn);
        }
    }

    public void OnDetach()
    {
        if (isButtonMode && isOn)
        {
            ChangeState(false);
        }
    }

    private void ChangeState(bool newState)
    {
        isOn = newState;

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