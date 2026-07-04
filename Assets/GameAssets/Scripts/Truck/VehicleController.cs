using UnityEngine;
using UnityEngine.Events;

public class VehicleController : MonoBehaviour
{
    [Header("Object Linking")]
    [SerializeField] private WheelCollider wheelFL;
    [SerializeField] private WheelCollider wheelFR;
    [SerializeField] private WheelCollider wheelBL;
    [SerializeField] private WheelCollider wheelBR;
    [SerializeField] private Transform wheelMeshFL;
    [SerializeField] private Transform wheelMeshFR;
    [SerializeField] private Transform wheelMeshBL;
    [SerializeField] private Transform wheelMeshBR;

    [SerializeField] public Winch frontWinch;
    [SerializeField] public Winch backWinch;

    public UnityEvent onVehicleDestroyed = new();

    [HideInInspector] public Vector3 driverInput;
    [HideInInspector] public bool turbo;
    private float currentSteer;
    private float currentTorque;
    private float currentBrake;

    [Header("Movement Config")]
    [Range(0f, 90f)] private float maxSteerAngle = 30f;
    private float motorTorque = 200f;
    private float turboMultiplier = 2.0f;
    private float brakingForce = 100f;
    private float jumpVelocity = 9f;
    private float swingForce = 2.2f;

    [Header("Stability Config")]
    private float centerOfMassY = -0.5f;
    private float antiRoll = 6000f;
    private float yawDamping = 50f;

    [Header("Friction Config (Forward)")]
    private float forwardExtremumSlip = 0.3f;
    private float forwardExtremumValue = 2.2f;
    private float forwardAsymptoteSlip = 0.7f;
    private float forwardAsymptoteValue = 1.8f;
    private float forwardStiffness = 2.5f;

    [Header("Friction Config (Sideways)")]
    private float sidewaysExtremumSlip = 0.1f;
    private float sidewaysExtremumValue = 1.5f;
    private float sidewaysAsymptoteSlip = 0.4f;
    private float sidewaysAsymptoteValue = 1.0f;
    private float sidewaysStiffness = 2.0f;

    [Header("Hill Climbing")]
    private float slopeAngleThreshold = 5f;
    private float downforceCoefficient = 0.5f;

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
        if (IsAnyWheelGrounded())
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
            isJumping = true;
            jumpTimer = jumpDuration;
        }
    }

    private bool IsAnyWheelGrounded()
    {
        return wheelFL.isGrounded || wheelFR.isGrounded || wheelBL.isGrounded || wheelBR.isGrounded;
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
        bool winchAttached = frontWinch.IsAttached || backWinch.IsAttached;
        bool grounded = IsAnyWheelGrounded();

        if (isJumping)
        {
            jumpTimer -= Time.deltaTime;
            if (jumpTimer <= 0f) isJumping = false;
        }

        if (winchAttached && !grounded)
        {
            currentSteer = 0f;
            currentTorque = 0f;
            currentBrake = 0f;
        }
        else
        {
            currentSteer = driverInput.x * maxSteerAngle;

            if (driverInput.z > 0.1f)
            {
                currentTorque = 0f;
            }
            else
            {
                float torque = driverInput.y * motorTorque;
                if (turbo && driverInput.y > 0.1f)
                    torque *= turboMultiplier;

                if (driverInput.y > 0.1f)
                {
                    float slopeAngle = GetSlopeAngle();
                    if (slopeAngle > slopeAngleThreshold)
                    {
                        if (slopeAngle < 20f)
                            torque *= 2f;
                        else
                            torque *= 3f;
                    }
                }

                currentTorque = isJumping ? 0f : torque;
            }

            currentBrake = driverInput.z * brakingForce;
        }

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
        ApplyYawStabilization();

        bool winchAttached = frontWinch.IsAttached || backWinch.IsAttached;
        if (winchAttached && !IsAnyWheelGrounded())
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 camForward = cam.transform.forward;
                camForward.y = 0f;
                camForward.Normalize();

                Vector3 camRight = cam.transform.right;
                camRight.y = 0f;
                camRight.Normalize();

                Vector3 inputDir = (camForward * driverInput.y + camRight * driverInput.x).normalized;
                if (inputDir.sqrMagnitude > 0.01f)
                    rb.AddForce(inputDir * swingForce, ForceMode.Acceleration);
            }
        }
    }

    private void ApplyYawStabilization()
    {
        float yawVelocity = Vector3.Dot(rb.angularVelocity, transform.up);
        float dampingTorque = -yawVelocity * yawDamping;
        rb.AddTorque(transform.up * dampingTorque, ForceMode.Force);
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