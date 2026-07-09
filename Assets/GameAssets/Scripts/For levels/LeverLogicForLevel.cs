using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct LeverCondition
{
    public LeverLogicForLevel lever;
    public bool requiredState;
}

public class LeverLogicForLevel : MonoBehaviour, IWinchInteractable
{
    [Header("Аниматор модели")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animParamName = "IsOn";

    [Header("Условия переключения")]
    [Tooltip("Рычаг можно переключить, только если ВСЕ указанные рычаги находятся в требуемом состоянии.")]
    [SerializeField] private LeverCondition[] conditions;

    [Header("События")]
    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    [Header("Звук")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip soundOn;
    [SerializeField] private AudioClip soundOff;

    [Header("Текущее состояние")]
    [SerializeField] private bool isOn = false;

    public bool IsOn => isOn;

    public bool AutoDetach => true;

    [ContextMenu("Дернуть рычаг (Тест)")]
    public void Interact()
    {
        // Проверяем все условия
        if (conditions != null)
        {
            foreach (var cond in conditions)
            {
                if (cond.lever == null)
                    continue;

                if (cond.lever.IsOn != cond.requiredState)
                {
                    Debug.Log($"Невозможно переключить: {cond.lever.name} должен быть {(cond.requiredState ? "вкл" : "выкл")}");
                    return; // Условие не выполнено — переключение невозможно
                }
            }
        }

        ChangeState(!isOn);
    }

    private void ChangeState(bool newState)
    {
        isOn = newState;

        if (animator != null)
            animator.SetBool(animParamName, isOn);

        if (audioSource != null)
        {
            if (isOn && soundOn != null)
                audioSource.PlayOneShot(soundOn);
            else if (!isOn && soundOff != null)
                audioSource.PlayOneShot(soundOff);
        }

        if (isOn)
            OnActivated.Invoke();
        else
            OnDeactivated.Invoke();
    }
}