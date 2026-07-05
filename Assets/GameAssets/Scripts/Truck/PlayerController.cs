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
    [SerializeField] private DeathScreen deathScreen;
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private bool manualDestroyEnabled = true;

    public System.Action onFrontWinchAttached;
    public System.Action onBackWinchAttached;
    public System.Action onFrontWinchDetached;
    public System.Action onBackWinchDetached;
    public System.Action onAnyWinchPull;

    private Vector2 moveControlSpeed = new(2.5f, 10f);
    private float moveResetSpeedMult = 3f;
    private Vector2 moveVector = Vector2.zero;

    [Header("Camera Settings")]
    private float cameraSensitivity = 0.5f;
    private float cameraSpeed = 10.0f;
    private float zoomSensitivity = 1.0f;
    private float zoomSpeed = 5.0f;
    private float minCameraDistance = 4f;
    private float maxCameraDistance = 16f;
    private float cameraDistance;
    private Vector2 cameraRotation;

    private bool brake = false;
    private bool isVehicleDestroyed;

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

    private void OnDestroy()
    {
        if (vehicleController != null)
            vehicleController.onVehicleDestroyed.RemoveListener(OnVehicleDestroyed);
    }

    private void OnBrakeStarted(InputAction.CallbackContext context) => brake = true;
    private void OnBrakeCancelled(InputAction.CallbackContext context) => brake = false;

    private void OnBoostStarted(InputAction.CallbackContext context)
    {
        vehicleController.turbo = true;
        vehicleController.onTurboChanged?.Invoke(true);
    }
    private void OnBoostCancelled(InputAction.CallbackContext context)
    {
        vehicleController.turbo = false;
        vehicleController.onTurboChanged?.Invoke(false);
    }
    private void OnJump(InputAction.CallbackContext context) => vehicleController.Jump();

    private void OnFrontWinch(InputAction.CallbackContext context)
    {
        if (context.interaction is TapInteraction)
        {
            if (!context.performed) return;
            if (vehicleController.frontWinch.IsAttached)
            {
                vehicleController.frontWinch.Detach();
                onFrontWinchDetached?.Invoke();
                return;
            }
            if (winchTarget != null && winchTarget.FrontWinchAvailable)
            {
                vehicleController.frontWinch.Attach(winchTarget.AttachmentPoint);
                onFrontWinchAttached?.Invoke();
            }
        }
        else
        {
            bool ctrl = Keyboard.current != null && Keyboard.current.ctrlKey.isPressed;
            if (ctrl)
            {
                vehicleController.frontWinch.extend = context.started;
                vehicleController.frontWinch.pull = false;
            }
            else
            {
                vehicleController.frontWinch.pull = context.started;
                vehicleController.frontWinch.extend = false;
                if (context.started)
                {
                    onAnyWinchPull?.Invoke();
                }
            }
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
                onBackWinchDetached?.Invoke();
                return;
            }
            if (winchTarget != null && winchTarget.BackWinchAvailable)
            {
                vehicleController.backWinch.Attach(winchTarget.AttachmentPoint);
                onBackWinchAttached?.Invoke();
            }
        }
        else
        {
            bool ctrl = Keyboard.current != null && Keyboard.current.ctrlKey.isPressed;
            if (ctrl)
            {
                vehicleController.backWinch.extend = context.started;
                vehicleController.backWinch.pull = false;
            }
            else
            {
                vehicleController.backWinch.pull = context.started;
                vehicleController.backWinch.extend = false;
                if (context.started)
                {
                    onAnyWinchPull?.Invoke();
                }
            }
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
        if (isVehicleDestroyed)
            return;

        isVehicleDestroyed = true;
        brake = false;
        moveVector = Vector2.zero;

        input.Player.Disable();

        if (vehicleController != null)
        {
            vehicleController.driverInput = Vector3.zero;
            vehicleController.turbo = false;
            vehicleController.onTurboChanged?.Invoke(false);
        }

        if (pauseMenu != null)
            pauseMenu.LockPause();

        Time.timeScale = 0f;

        if (deathScreen != null)
            deathScreen.Show();
        else
            Debug.LogWarning("Death screen is not assigned in PlayerController.");
    }

    private void HandleDriverInput()
    {
        Vector2 targetMoveVector = input.Player.Move.ReadValue<Vector2>();
        Vector2 speed = moveControlSpeed;
        if (targetMoveVector.x == 0) speed.x *= moveResetSpeedMult;
        if (targetMoveVector.y == 0) speed.y *= moveResetSpeedMult;
        moveVector.x = Mathf.Lerp(moveVector.x, targetMoveVector.x, speed.x * Time.deltaTime);
        moveVector.y = Mathf.Lerp(moveVector.y, targetMoveVector.y, speed.y * Time.deltaTime);
        vehicleController.driverInput = new Vector3(moveVector.x, moveVector.y, brake ? 1f : 0f);
    }

    private void HandleManualDestroyInput()
    {
        if (!manualDestroyEnabled || isVehicleDestroyed || vehicleController == null)
            return;

        if (pauseMenu != null && pauseMenu.PauseGame)
            return;

        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
            vehicleController.DestroyVehicle();
    }

    private void Update()
    {
        if (followTarget != null) transform.position = followTarget.position;
        HandleManualDestroyInput();
        if (vehicleController != null && !isVehicleDestroyed) HandleDriverInput();

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
