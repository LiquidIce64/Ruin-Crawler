using UnityEngine;

public class VehicleController : MonoBehaviour
{
    [SerializeField] private WheelCollider wheelFL;
    [SerializeField] private WheelCollider wheelFR;
    [SerializeField] private WheelCollider wheelBL;
    [SerializeField] private WheelCollider wheelBR;
    private Transform wheelMeshFL;
    private Transform wheelMeshFR;
    private Transform wheelMeshBL;
    private Transform wheelMeshBR;

    [HideInInspector] public Vector3 driverInput;
    private float currentSteer;
    private float currentTorque;
    private float currentBrake;

    [Header("Config")]
    [SerializeField][Range(0f, 90f)] private float maxSteerAngle = 30f;
    [SerializeField] private float motorTorque = 100f;
    [SerializeField] private float brakingForce = 100f;

    private void Awake()
    {
        wheelMeshFL = wheelFL.gameObject.transform.GetChild(0);
        wheelMeshFR = wheelFR.gameObject.transform.GetChild(0);
        wheelMeshBL = wheelBL.gameObject.transform.GetChild(0);
        wheelMeshBR = wheelBR.gameObject.transform.GetChild(0);
    }

    private void UpdateWheel(WheelCollider wheel, Transform wheelMesh, bool canSteer = false, bool canDrive = true, bool canBrake = true)
    {
        if (canSteer) wheel.steerAngle = currentSteer;
        if (canDrive) wheel.motorTorque = currentTorque;
        if (canBrake) wheel.brakeTorque = currentBrake;

        wheel.GetWorldPose(out var pos, out var quat);
        wheelMesh.SetPositionAndRotation(pos, quat);
    }

    private void Update()
    {
        currentSteer = driverInput.x * maxSteerAngle;
        currentTorque = driverInput.y * motorTorque;
        currentBrake = driverInput.z * brakingForce;

        UpdateWheel(wheelFL, wheelMeshFL, canSteer: true);
        UpdateWheel(wheelFR, wheelMeshFR, canSteer: true);
        UpdateWheel(wheelBL, wheelMeshBL);
        UpdateWheel(wheelBR, wheelMeshBR);
    }
}
