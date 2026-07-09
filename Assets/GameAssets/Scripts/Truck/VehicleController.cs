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
    private float motorTorque = 440f;
    private float turboMultiplier = 1.8f;
    private float brakingForce = 800f;
    private float jumpVelocity = 11f;
    private float swingForce = 5.75f;

    [Header("Audio")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioSource jumpAudioSource;

    [Header("Steering Stability")]
    private float highSpeedSteerReduction = 0.45f;

    [Header("Low Speed Torque Boost")]
    private bool enableLowSpeedBoost = true;
    private float lowSpeedThreshold = 8f;
    private float lowSpeedTorqueFactor = 1.6f;

    [Header("Wall Riding Prevention")]
    private float slopeTorqueFalloffStart = 45f;
    private float slopeTorqueFalloffEnd = 60f;
    private float wallGripMultiplier = 0.1f;

    [Header("Stability Config")]
    private float centerOfMassY = -0.5f;
    private float antiRoll = 30000f;
    private float yawDamping = 120f;

    [Header("Upright Stabilization")]
    private float uprightStabilizationStrength = 35f;
    private float uprightStabilizationDamping = 6f;
    private float landingStabilizationMultiplier = 2.5f;
    private float landingAngularDrag = 6f;

    [Header("Rigidbody Damping")]
    private float linearDrag = 0.3f;
    private float angularDrag = 1.5f;

    [Header("Downforce")]
    private float downforceCoefficient = 3f;

    [Header("Suspension")]
    private float suspensionSpring = 25000f;
    private float suspensionDamper = 8000f;
    private float suspensionDistance = 0.3f;

    [Header("Friction Config (Forward)")]
    private float forwardExtremumSlip = 0.3f;
    private float forwardExtremumValue = 1.0f;
    private float forwardAsymptoteSlip = 0.7f;
    private float forwardAsymptoteValue = 0.8f;
    private float forwardStiffness = 2.0f;

    [Header("Friction Config (Sideways)")]
    private float sidewaysExtremumSlip = 0.1f;
    private float sidewaysExtremumValue = 1.2f;
    private float sidewaysAsymptoteSlip = 0.4f;
    private float sidewaysAsymptoteValue = 1.0f;
    private float sidewaysStiffness = 2.5f;

    [Header("Hill Climbing")]
    private float slopeAngleThreshold = 5f;

    public bool IsGrounded => AreAtLeastThreeWheelsGrounded();

    private float jumpTimer = 0f;
    private float jumpDuration = 0.5f;
    private float landingSmoothTime = 0.3f;
    private float landingTimer = 0f;

    [Header("Speed / Sound")]
    private float maxSpeedKmh = 90f;
    public float CurrentSpeedNormalized
    {
        get
        {
            if (rb == null) return 0f;
            float speedKmh = rb.linearVelocity.magnitude * 3.6f;
            return Mathf.Clamp01(speedKmh / maxSpeedKmh);
        }
    }

    private Rigidbody rb;
    private bool isJumping = false;
    private bool wasInAir = false;

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

            if (jumpClip != null)
            {
                if (jumpAudioSource != null)
                    jumpAudioSource.PlayOneShot(jumpClip);
                else
                    AudioSource.PlayClipAtPoint(jumpClip, transform.position);
            }

            onJumpPerformed?.Invoke();
        }
    }

    public bool IsAnyWheelGrounded()
    {
        return wheelFL.isGrounded || wheelFR.isGrounded || wheelBL.isGrounded || wheelBR.isGrounded;
    }


    public bool AreAtLeastThreeWheelsGrounded()
    {
        int groundedCount = 0;
        if (wheelFL.isGrounded) groundedCount++;
        if (wheelFR.isGrounded) groundedCount++;
        if (wheelBL.isGrounded) groundedCount++;
        if (wheelBR.isGrounded) groundedCount++;
        return groundedCount >= 3;
    }

    public bool AreAtLeastTwoWheelsGrounded()
    {
        int groundedCount = 0;
        if (wheelFL.isGrounded) groundedCount++;
        if (wheelFR.isGrounded) groundedCount++;
        if (wheelBL.isGrounded) groundedCount++;
        if (wheelBR.isGrounded) groundedCount++;
        return groundedCount >= 2;
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
            return;
        }

        float slopeAngle = GetSlopeAngle();
        float slopeRange = Mathf.Max(0.01f, slopeTorqueFalloffEnd - slopeTorqueFalloffStart);

        float maxSpeedMs = maxSpeedKmh / 3.6f;
        float speedFactor = maxSpeedMs > 0f ? Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeedMs) : 0f;
        float steerMultiplier = Mathf.Lerp(1f, highSpeedSteerReduction, speedFactor);

        currentSteer = driverInput.x * maxSteerAngle * steerMultiplier;

        if (driverInput.z > 0.1f)
        {
            currentTorque = 0f;
            currentBrake = driverInput.z * brakingForce;
            return;
        }

        float torque = driverInput.y * motorTorque;

        if (turbo && driverInput.y > 0.1f)
            torque *= turboMultiplier;

        if (driverInput.y > 0.1f)
        {
            if (slopeAngle > slopeAngleThreshold)
                torque *= (slopeAngle < 20f) ? 1.3f : 1.8f;
        }

        if (enableLowSpeedBoost && driverInput.y > 0.1f)
        {
            float speed = rb.linearVelocity.magnitude;
            float t = 1f - Mathf.Clamp01(speed / lowSpeedThreshold);
            float boost = Mathf.Lerp(1f, lowSpeedTorqueFactor, t);
            torque *= boost;
        }

        Vector3 driveDirection = transform.forward * driverInput.y;
        if (driveDirection.sqrMagnitude > 0.0001f && driveDirection.normalized.y > 0f)
        {
            float uphillTorqueFactor = 1f - Mathf.Clamp01((slopeAngle - slopeTorqueFalloffStart) / slopeRange);
            torque *= uphillTorqueFactor;
        }

        if (!AreAtLeastTwoWheelsGrounded())
            torque = 0f;

        currentTorque = (isJumping || landingTimer > 0f) ? 0f : torque;
        currentBrake = driverInput.z * brakingForce;
    }

    private void Update()
    {
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
        bool grounded = IsAnyWheelGrounded();
        HandleLandingState(grounded);

        UpdateWheelGrip(wheelFL);
        UpdateWheelGrip(wheelFR);
        UpdateWheelGrip(wheelBL);
        UpdateWheelGrip(wheelBR);

        ApplyAntiRoll(wheelFL, wheelFR);
        ApplyAntiRoll(wheelBL, wheelBR);
        ApplyDownforce();
        ApplyYawStabilization();
        ApplyUprightStabilization(grounded);

        bool winchAttached = frontWinch.IsAttached || backWinch.IsAttached;
        if (winchAttached && !grounded)
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

    private void HandleLandingState(bool grounded)
    {
        if (!grounded)
        {
            wasInAir = true;
        }
        else if (wasInAir)
        {
            wasInAir = false;
            landingTimer = landingSmoothTime;
        }

        if (landingTimer > 0f)
            landingTimer = Mathf.Max(0f, landingTimer - Time.fixedDeltaTime);

        rb.angularDamping = landingTimer > 0f ? landingAngularDrag : angularDrag;
    }

    private void ApplyUprightStabilization(bool grounded)
    {
        if (!grounded && landingTimer <= 0f)
            return;

        Vector3 targetUp = Vector3.up;
        if (grounded && TryGetAverageGroundNormal(out var groundNormal)
            && Vector3.Angle(Vector3.up, groundNormal) <= slopeTorqueFalloffEnd)
        {
            targetUp = groundNormal;
        }

        Vector3 correctionAxis = Vector3.Cross(transform.up, targetUp);

        Vector3 angularVelocity = rb.angularVelocity;
        Vector3 yawComponent = Vector3.Project(angularVelocity, transform.up);
        Vector3 rollPitchVelocity = angularVelocity - yawComponent;

        float strength = landingTimer > 0f
            ? uprightStabilizationStrength * landingStabilizationMultiplier
            : uprightStabilizationStrength;

        Vector3 stabilizingTorque = correctionAxis * strength - rollPitchVelocity * uprightStabilizationDamping;
        rb.AddTorque(stabilizingTorque, ForceMode.Acceleration);
    }

    private void UpdateWheelGrip(WheelCollider wheel)
    {
        float gripFactor = 1f;

        if (wheel.GetGroundHit(out WheelHit hit))
        {
            float contactAngle = Vector3.Angle(Vector3.up, hit.normal);
            float slopeRange = Mathf.Max(0.01f, slopeTorqueFalloffEnd - slopeTorqueFalloffStart);
            float t = Mathf.Clamp01((contactAngle - slopeTorqueFalloffStart) / slopeRange);
            gripFactor = Mathf.Lerp(1f, wallGripMultiplier, t);
        }

        WheelFrictionCurve forward = wheel.forwardFriction;
        forward.stiffness = forwardStiffness * gripFactor;
        wheel.forwardFriction = forward;

        WheelFrictionCurve sideways = wheel.sidewaysFriction;
        sideways.stiffness = sidewaysStiffness * gripFactor;
        wheel.sidewaysFriction = sideways;
    }

    private bool TryGetAverageGroundNormal(out Vector3 normal)
    {
        normal = Vector3.zero;
        int groundedCount = 0;
        WheelHit hit;

        if (wheelFL.GetGroundHit(out hit)) { normal += hit.normal; groundedCount++; }
        if (wheelFR.GetGroundHit(out hit)) { normal += hit.normal; groundedCount++; }
        if (wheelBL.GetGroundHit(out hit)) { normal += hit.normal; groundedCount++; }
        if (wheelBR.GetGroundHit(out hit)) { normal += hit.normal; groundedCount++; }

        if (groundedCount == 0)
        {
            normal = Vector3.up;
            return false;
        }

        normal = (normal / groundedCount).normalized;
        return true;
    }

    private void ApplyDownforce()
    {
        if (!IsAnyWheelGrounded())
            return;

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