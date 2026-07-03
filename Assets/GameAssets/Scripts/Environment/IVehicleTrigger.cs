public interface IVehicleTrigger
{
    public void OnTrigger() { }
    public void OnTrigger(VehicleController vehicle) => OnTrigger();
}
