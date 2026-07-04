using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private string stepId;

    private void OnTriggerEnter(Collider other)
    {
        VehicleController vehicle = null;
        if (other.attachedRigidbody != null)
            vehicle = other.attachedRigidbody.GetComponent<VehicleController>();
        if (vehicle == null)
            vehicle = other.GetComponentInParent<VehicleController>();

        if (vehicle != null)
        {
            TutorialManager.Instance.RequestStep(stepId);
            gameObject.SetActive(false);
        }
    }
}