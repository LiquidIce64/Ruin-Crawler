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
    private bool isDestroyed;

    [HideInInspector] public System.Action onJumpPerformed;
    [HideInInspector] public System.Action<bool> onTurboChanged;

    [Header("Effects")]
    [SerializeField] private ParticleSystem turboParticles;
    [SerializeField] private ParticleSystem jumpParticles;

    [HideInInspector] public Vector3 driverInput;
    [HideInInspector] public bool turbo;
    private float currentSteer;
    private float currentTorque;
    private float currentBrake;

    [Header("Movement Config")]
    [SerializeField][Range(0f, 90f)] private float maxSteerAngle = 30f;
    private float motorTorque = 270f;
    private float turboMultiplier = 1.8f;
    private float brakingForce = 500f;
    private float jumpVelocity = 10f;
    private float swingForce = 5.7f;

    [Header("Low Speed Torque Boost")]
    private bool enableLowSpeedBoost = true;
    private float lowSpeedThreshold = 10f;
    private float lowSpeedTorqueFactor = 2.0f;

    [Header("Stability Config")]
    private float centerOfMassY = -0.5f;
    private float antiRoll = 12000f;
    private float yawDamping = 100f;

    [Header("Rigidbody Damping")]
    private float linearDrag = 0.3f;
    private float angularDrag = 1.0f;

    [Header("Downforce")]
    private float downforceCoefficient = 0.5f;
    private float lowSpeedDownforce = 5.0f;

    [Header("Suspension")]
    private float suspensionSpring = 35000f;
    private float suspensionDamper = 4500f;
    private float suspensionDistance = 0.3f;

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
    private float sidewaysStiffness = 1.2f;

    [Header("Hill Climbing")]
    private float slopeAngleThreshold = 5f;

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

        rb.linearDamping = linearDrag;
        rb.angularDamping = angularDrag;

        ConfigureSuspension(wheelFL);
        ConfigureSuspension(wheelFR);
        ConfigureSuspension(wheelBL);
        ConfigureSuspension(wheelBR);

        ApplyFrictionToWheel(wheelFL);
        ApplyFrictionToWheel(wheelFR);
        ApplyFrictionToWheel(wheelBL);
        ApplyFrictionToWheel(wheelBR);
    }

    private void ConfigureSuspension(WheelCollider wheel)
    {
        JointSpring spring = wheel.suspensionSpring;
        spring.spring = suspensionSpring;
        spring.damper = suspensionDamper;
        spring.targetPosition = 0.5f;
        wheel.suspensionSpring = spring;
        wheel.suspensionDistance = suspensionDistance;
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

    public void DestroyVehicle()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;
        onVehicleDestroyed.Invoke();
    }

    public void Jump()
    {
        if (IsAnyWheelGrounded())
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
            isJumping = true;
            jumpTimer = jumpDuration;

            if (jumpParticles != null)
            {
                jumpParticles.Play();
            }

            onJumpPerformed?.Invoke();
        }
    }

    public bool IsAnyWheelGrounded()
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

    private void UpdateTurboParticles()
    {
        if (turboParticles != null)
        {
            bool shouldPlay = turbo && driverInput.y > 0.1f;
            if (shouldPlay && !turboParticles.isPlaying)
                turboParticles.Play();
            else if (!shouldPlay && turboParticles.isPlaying)
                turboParticles.Stop();
        }
    }

    private void UpdateDrivingInput()
    {
        bool winchAttached = frontWinch.IsAttached || backWinch.IsAttached;
        bool grounded = IsAnyWheelGrounded();

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
                        torque *= (slopeAngle < 20f) ? 1.3f : 1.8f;
                    }
                }

                if (enableLowSpeedBoost && driverInput.y > 0.1f)
                {
                    float speed = rb.linearVelocity.magnitude;
                    float t = 1f - Mathf.Clamp01(speed / lowSpeedThreshold);
                    float boost = Mathf.Lerp(1f, lowSpeedTorqueFactor, t);
                    torque *= boost;
                }

                currentTorque = isJumping ? 0f : torque;
            }
            currentBrake = driverInput.z * brakingForce;
        }
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

        UpdateDrivingInput();

        UpdateWheel(wheelFL, wheelMeshFL, canSteer: true);
        UpdateWheel(wheelFR, wheelMeshFR, canSteer: true);
        UpdateWheel(wheelBL, wheelMeshBL);
        UpdateWheel(wheelBR, wheelMeshBR);
        UpdateTurboParticles();
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
        if (!IsAnyWheelGrounded())
            return;

        float speed = rb.linearVelocity.magnitude;
        float dynamicDownforce = speed * speed * downforceCoefficient;
        float staticDownforce = lowSpeedDownforce * rb.mass * Mathf.Max(0f, 1f - (speed / lowSpeedThreshold));
        rb.AddForce(-transform.up * (dynamicDownforce + staticDownforce), ForceMode.Force);
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