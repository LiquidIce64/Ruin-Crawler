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

    public Winch frontWinch;
    public Winch backWinch;

    [HideInInspector] public Vector3 driverInput;
    [HideInInspector] public bool turbo;
    private float currentSteer;
    private float currentTorque;
    private float currentBrake;

    [Header("Movement Config")]
    [SerializeField][Range(0f, 90f)] private float maxSteerAngle = 30f;
    [SerializeField] private float motorTorque = 100f;
    [SerializeField] private float turboMultiplier = 2.5f;
    [SerializeField] private float brakingForce = 100f;
    [SerializeField] private float jumpVelocity = 9f;

    [Header("Stability Config")]
    [SerializeField] private float centerOfMassY = -0.3f;
    [SerializeField] private float antiRoll = 5000f;

    [Header("Sideways Friction")]
    [SerializeField] private float sidewaysExtremumSlip = 0.1f;
    [SerializeField] private float sidewaysExtremumValue = 1.5f;
    [SerializeField] private float sidewaysAsymptoteSlip = 0.4f;
    [SerializeField] private float sidewaysAsymptoteValue = 1.0f;
    [SerializeField] private float sidewaysStiffness = 1.5f;

    private Rigidbody rb;
    private bool isJumping = false;
    private float jumpTimer = 0f;
    private const float jumpDuration = 0.2f;

    private void Awake()
    {
        wheelMeshFL = wheelFL.gameObject.transform.GetChild(0);
        wheelMeshFR = wheelFR.gameObject.transform.GetChild(0);
        wheelMeshBL = wheelBL.gameObject.transform.GetChild(0);
        wheelMeshBR = wheelBR.gameObject.transform.GetChild(0);
        rb = GetComponent<Rigidbody>();

        rb.centerOfMass = new Vector3(0, centerOfMassY, 0);

        // Apply custom sideways friction to all wheels
        WheelFrictionCurve sidewaysFriction = new WheelFrictionCurve();
        sidewaysFriction.extremumSlip = sidewaysExtremumSlip;
        sidewaysFriction.extremumValue = sidewaysExtremumValue;
        sidewaysFriction.asymptoteSlip = sidewaysAsymptoteSlip;
        sidewaysFriction.asymptoteValue = sidewaysAsymptoteValue;
        sidewaysFriction.stiffness = sidewaysStiffness;

        wheelFL.sidewaysFriction = sidewaysFriction;
        wheelFR.sidewaysFriction = sidewaysFriction;
        wheelBL.sidewaysFriction = sidewaysFriction;
        wheelBR.sidewaysFriction = sidewaysFriction;
    }

    public void Jump()
    {
        if (wheelFL.isGrounded || wheelFR.isGrounded || wheelBL.isGrounded || wheelBR.isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
            isJumping = true;
            jumpTimer = jumpDuration;
        }
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
        if (isJumping)
        {
            jumpTimer -= Time.deltaTime;
            if (jumpTimer <= 0f) isJumping = false;
        }

        currentSteer = driverInput.x * maxSteerAngle;

        if (driverInput.z > 0.1f)
        {
            currentTorque = 0f;
        }
        else
        {
            float torque = driverInput.y * motorTorque;
            if (turbo) torque *= turboMultiplier;
            currentTorque = isJumping ? 0f : torque;
        }

        currentBrake = driverInput.z * brakingForce;

        UpdateWheel(wheelFL, wheelMeshFL, canSteer: true);
        UpdateWheel(wheelFR, wheelMeshFR, canSteer: true);
        UpdateWheel(wheelBL, wheelMeshBL);
        UpdateWheel(wheelBR, wheelMeshBR);
    }

    private void FixedUpdate()
    {
        ApplyAntiRoll(wheelFL, wheelFR);
        ApplyAntiRoll(wheelBL, wheelBR);
    }

    private void ApplyAntiRoll(WheelCollider leftWheel, WheelCollider rightWheel)
    {
        WheelHit hit;
        float travelL = 1.0f;
        float travelR = 1.0f;

        bool groundedL = leftWheel.GetGroundHit(out hit);
        if (groundedL)
            travelL = (-leftWheel.transform.InverseTransformPoint(hit.point).y - leftWheel.radius) / leftWheel.suspensionDistance;

        bool groundedR = rightWheel.GetGroundHit(out hit);
        if (groundedR)
            travelR = (-rightWheel.transform.InverseTransformPoint(hit.point).y - rightWheel.radius) / rightWheel.suspensionDistance;

        float antiRollForce = (travelL - travelR) * antiRoll;

        if (groundedL)
            rb.AddForceAtPosition(leftWheel.transform.up * -antiRollForce, leftWheel.transform.position);
        if (groundedR)
            rb.AddForceAtPosition(rightWheel.transform.up * antiRollForce, rightWheel.transform.position);
    }
}