using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerController : MonoBehaviour
{
    private static int layerMask;
    private InputSystem_Actions input;
    private Camera orbitCamera;
    public VehicleController vehicleController;
    public Transform followTarget;
    public WinchTarget winchTarget;

    [Header("Camera Settings")]
    [SerializeField] private float cameraSensitivity = 1.0f;
    [SerializeField] private float cameraSpeed = 1.0f;
    [SerializeField] private float zoomSensitivity = 1.0f;
    [SerializeField] private float zoomSpeed = 1.0f;
    [SerializeField] private float minCameraDistance = 4f;
    [SerializeField] private float maxCameraDistance = 16f;
    private float cameraDistance;
    private Vector2 cameraRotation;

    private bool brake = false;

    private void Awake()
    {
        layerMask = LayerMask.GetMask("Default");
        input = new InputSystem_Actions();
        orbitCamera = GetComponentInChildren<Camera>();

        var euler = transform.localRotation.eulerAngles;
        cameraRotation.x = euler.y;
        cameraRotation.y = euler.x;
        cameraDistance = Mathf.Clamp(-orbitCamera.transform.localPosition.z, minCameraDistance, maxCameraDistance);

        vehicleController.onVehicleDestroyed.AddListener(OnVehicleDestroyed);
    }

    private void OnEnable()
    {
        input.Enable();

        input.Player.Brake.started += OnBrakeStarted;
        input.Player.Brake.canceled += OnBrakeCancelled;

        input.Player.Look.performed += OnLook;
        input.Player.Zoom.performed += OnZoom;

        input.Player.Boost.started += OnBoostStarted;
        input.Player.Boost.canceled += OnBoostCancelled;
        input.Player.Jump.performed += OnJump;

        input.Player.FrontWinch.started += OnFrontWinch;
        input.Player.FrontWinch.performed += OnFrontWinch;
        input.Player.FrontWinch.canceled += OnFrontWinch;

        input.Player.BackWinch.started += OnBackWinch;
        input.Player.BackWinch.performed += OnBackWinch;
        input.Player.BackWinch.canceled += OnBackWinch;
    }

    private void OnDisable()
    {
        input.Disable();

        input.Player.Brake.started -= OnBrakeStarted;
        input.Player.Brake.canceled -= OnBrakeCancelled;

        input.Player.Look.performed -= OnLook;
        input.Player.Zoom.performed -= OnZoom;

        input.Player.Boost.started -= OnBoostStarted;
        input.Player.Boost.canceled -= OnBoostCancelled;
        input.Player.Jump.performed -= OnJump;

        input.Player.FrontWinch.started -= OnFrontWinch;
        input.Player.FrontWinch.performed -= OnFrontWinch;
        input.Player.FrontWinch.canceled -= OnFrontWinch;

        input.Player.BackWinch.started -= OnBackWinch;
        input.Player.BackWinch.performed -= OnBackWinch;
        input.Player.BackWinch.canceled -= OnBackWinch;
    }

    private void OnBrakeStarted(InputAction.CallbackContext context) => brake = true;
    private void OnBrakeCancelled(InputAction.CallbackContext context) => brake = false;

    private void OnBoostStarted(InputAction.CallbackContext context) => vehicleController.turbo = true;
    private void OnBoostCancelled(InputAction.CallbackContext context) => vehicleController.turbo = false;
    private void OnJump(InputAction.CallbackContext context) => vehicleController.Jump();

    private void OnFrontWinch(InputAction.CallbackContext context)
    {
        if (context.interaction is TapInteraction)
        {
            if (!context.performed) return;
            if (vehicleController.frontWinch.IsAttached)
            {
                vehicleController.frontWinch.Detach();
                return;
            }
            if (winchTarget != null && winchTarget.FrontWinchAvailable)
                vehicleController.frontWinch.Attach(winchTarget.AttachmentPoint);
        }
        else // hold interaction
        {
            vehicleController.frontWinch.pull = context.started;
        }
    }

    private void OnBackWinch(InputAction.CallbackContext context)
    {
        if (context.interaction is TapInteraction)
        {
            if (!context.performed) return;
            if (vehicleController.backWinch.IsAttached)
            {
                vehicleController.backWinch.Detach();
                return;
            }
            if (winchTarget != null && winchTarget.BackWinchAvailable)
                vehicleController.backWinch.Attach(winchTarget.AttachmentPoint);
        }
        else // hold interaction
        {
            vehicleController.backWinch.pull = context.started;
        }
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        Vector2 lookVector = context.ReadValue<Vector2>() * cameraSensitivity;
        if (context.control.device is Gamepad) lookVector *= Time.deltaTime;
        cameraRotation.x += lookVector.x;
        cameraRotation.y = Mathf.Clamp(cameraRotation.y - lookVector.y, -90f, 90f);
    }

    private void OnZoom(InputAction.CallbackContext context)
    {
        float zoomDelta = context.ReadValue<float>() * zoomSensitivity;
        if (context.control.device is Gamepad) zoomDelta *= Time.deltaTime;
        cameraDistance = Mathf.Clamp(cameraDistance - zoomDelta, minCameraDistance, maxCameraDistance);
    }

    private void OnVehicleDestroyed()
    {
        // TODO: Show death screen
    }

    private void HandleDriverInput()
    {
        Vector2 moveVector = input.Player.Move.ReadValue<Vector2>();
        vehicleController.driverInput = new Vector3(moveVector.x, moveVector.y, brake ? 1f : 0f);
    }

    private void Update()
    {
        if (followTarget != null) transform.position = followTarget.position;
        if (vehicleController != null) HandleDriverInput();

        Quaternion targetRotation = Quaternion.Euler(cameraRotation.y, cameraRotation.x, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, cameraSpeed * Time.deltaTime);

        float newDist = Mathf.Lerp(-orbitCamera.transform.localPosition.z, cameraDistance, zoomSpeed * Time.deltaTime);
        if (Physics.SphereCast(transform.position, orbitCamera.nearClipPlane, -transform.forward, out var hit, newDist + orbitCamera.nearClipPlane, layerMask))
            newDist = Mathf.Min(newDist, hit.distance - orbitCamera.nearClipPlane);
        orbitCamera.transform.localPosition = Vector3.back * newDist;

        if (winchTarget != null)
            winchTarget.HandleUpdate(vehicleController.frontWinch, vehicleController.backWinch);
    }
}