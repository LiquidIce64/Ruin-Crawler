using UnityEngine;
using UnityEngine.Events;

public class PressurePlateLogic : MonoBehaviour
{
    [Header("Аниматор модели")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animParamName = "IsPressed";

    [Header("Настройки активации")]
    [Tooltip("Сколько предметов нужно положить на плиту для активации")]
    [SerializeField] private int requiredObjects = 1;

    [Header("События")]
    public UnityEvent OnActivated;   // Сработает, когда наберется нужное количество
    public UnityEvent OnDeactivated; // Сработает, когда предметов станет меньше

    [Header("Текущее состояние (только для просмотра)")]
    [SerializeField] private int currentObjects = 0;
    [SerializeField] private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, находится ли объект на слое WinchInteractable
        if (other.gameObject.layer == LayerMask.NameToLayer("WinchInteractable"))
        {
            currentObjects++;
            CheckState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("WinchInteractable"))
        {
            currentObjects--;
            if (currentObjects < 0) currentObjects = 0; // Защита от багов
            CheckState();
        }
    }

    private void CheckState()
    {
        // Если предметов достаточно и плита ЕЩЕ НЕ была активирована
        if (currentObjects >= requiredObjects && !isActivated)
        {
            isActivated = true;

            if (animator != null)
            {
                animator.SetBool(animParamName, true);
            }

            OnActivated.Invoke();
            Debug.Log($"Плита АКТИВИРОВАНА! ({currentObjects}/{requiredObjects})");
        }
        // Если предметов стало меньше чем нужно, а плита БЫЛА активирована
        else if (currentObjects < requiredObjects && isActivated)
        {
            isActivated = false;

            if (animator != null)
            {
                animator.SetBool(animParamName, false);
            }

            OnDeactivated.Invoke();
            Debug.Log($"Плита ДЕАКТИВИРОВАНА! ({currentObjects}/{requiredObjects})");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("WinchInteractable"))
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                float dampingStrength = Time.deltaTime * 10f;

                // Гасим скольжение
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, dampingStrength);

                // Гасим вращение и кувырки
                rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, dampingStrength);
            }
        }
    }
}