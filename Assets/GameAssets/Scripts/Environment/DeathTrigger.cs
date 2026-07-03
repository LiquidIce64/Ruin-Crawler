using UnityEngine;

public class DeathTrigger : MonoBehaviour, IVehicleTrigger
{
    public void OnTrigger(VehicleController vehicle)
    {
        vehicle.onVehicleDestroyed.Invoke();
    }
}
