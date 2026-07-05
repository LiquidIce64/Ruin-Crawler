using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialTrigger : MonoBehaviour, IVehicleTrigger
{
    [SerializeField] private string stepId;

    public void OnTrigger(VehicleController vehicle)
    {
        TutorialManager.Instance.RequestStep(stepId);
        gameObject.SetActive(false);
    }
}