using UnityEngine;
using UnityEngine.Events;

public class VehicleController : MonoBehaviour
{
    [Header("Object Linking")]
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

    public UnityEvent onVehicleDestroyed = new();

    [HideInInspector] public Vector3 driverInput;
    [HideInInspector] public bool turbo;
    private float currentSteer;
    private float currentTorque;
    private float currentBrake;

    [Header("Movement Config")]
    [SerializeField][Range(0f, 90f)] private float maxSteerAngle = 30f;
    [SerializeField] private float motorTorque = 250f;
    [SerializeField] private float turboMultiplier = 2.5f;
    [SerializeField] private float brakingForce = 100f;
    [SerializeField] private float jumpVelocity = 9f;

    [Header("Stability Config")]
    [SerializeField] private float centerOfMassY = -0.5f;
    [SerializeField] private float antiRoll = 6000f;

    [Header("Friction Config (Forward)")]
    [SerializeField] private float forwardExtremumSlip = 0.3f;
    [SerializeField] private float forwardExtremumValue = 2.2f;
    [SerializeField] private float forwardAsymptoteSlip = 0.7f;
    [SerializeField] private float forwardAsymptoteValue = 1.8f;
    [SerializeField] private float forwardStiffness = 2.5f;

    [Header("Friction Config (Sideways)")]
    [SerializeField] private float sidewaysExtremumSlip = 0.1f;
    [SerializeField] private float sidewaysExtremumValue = 1.5f;
    [SerializeField] private float sidewaysAsymptoteSlip = 0.4f;
    [SerializeField] private float sidewaysAsymptoteValue = 1.0f;
    [SerializeField] private float sidewaysStiffness = 2.0f;

    [Header("Hill Climbing")]
    [SerializeField] private float slopeTorqueMultiplier = 3.0f;
    [SerializeField] private float slopeAngleThreshold = 5f;
    [SerializeField] private float downforceCoefficient = 0.5f;

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

        ApplyFrictionToWheel(wheelFL);
        ApplyFrictionToWheel(wheelFR);
        ApplyFrictionToWheel(wheelBL);
        ApplyFrictionToWheel(wheelBR);
    }

    private void ApplyFrictionToWheel(WheelCollider wheel)
    {
        WheelFrictionCurve forwardFriction = new WheelFrictionCurve
        {
            extremumSlip = forwardExtremumSlip,
            extremumValue = forwardExtremumValue,
            asymptoteSlip = forwardAsymptoteSlip,
            asymptoteValue = forwardAsymptoteValue,
            stiffness = forwardStiffness
        };
        wheel.forwardFriction = forwardFriction;

        WheelFrictionCurve sidewaysFriction = new WheelFrictionCurve
        {
            extremumSlip = sidewaysExtremumSlip,
            extremumValue = sidewaysExtremumValue,
            asymptoteSlip = sidewaysAsymptoteSlip,
            asymptoteValue = sidewaysAsymptoteValue,
            stiffness = sidewaysStiffness
        };
        wheel.sidewaysFriction = sidewaysFriction;
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

            if (driverInput.y > 0.1f)
            {
                float slopeAngle = GetSlopeAngle();
                if (slopeAngle > slopeAngleThreshold)
                {
                    float t = Mathf.Clamp01((slopeAngle - slopeAngleThreshold) / 45f);
                    float multiplier = Mathf.Lerp(1f, slopeTorqueMultiplier, t);
                    torque *= multiplier;
                }
            }

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
        ApplyDownforce();
    }

    private void ApplyDownforce()
    {
        float speed = rb.linearVelocity.magnitude;
        float downforce = speed * speed * downforceCoefficient;
        rb.AddForce(-transform.up * downforce, ForceMode.Force);
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

    private float GetSlopeAngle()
    {
        Vector3 averageNormal = Vector3.zero;
        int groundedCount = 0;
        WheelHit hit;

        if (wheelFL.GetGroundHit(out hit)) { averageNormal += hit.normal; groundedCount++; }
        if (wheelFR.GetGroundHit(out hit)) { averageNormal += hit.normal; groundedCount++; }
        if (wheelBL.GetGroundHit(out hit)) { averageNormal += hit.normal; groundedCount++; }
        if (wheelBR.GetGroundHit(out hit)) { averageNormal += hit.normal; groundedCount++; }

        if (groundedCount == 0) return 0f;

        averageNormal /= groundedCount;
        return Vector3.Angle(Vector3.up, averageNormal);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IVehicleTrigger trigger))
            trigger.OnTrigger(this);
    }
}