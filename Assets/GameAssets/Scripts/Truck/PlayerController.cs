using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private static int layerMask;
    private InputSystem_Actions input;
    public VehicleController vehicleController;
    public Transform followTarget;
    private Camera orbitCamera;

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
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Brake.started += OnBrakeStarted;
        input.Player.Brake.canceled += OnBrakeCancelled;
        input.Player.Look.performed += OnLook;
        input.Player.Zoom.performed += OnZoom;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.Brake.started -= OnBrakeStarted;
        input.Player.Brake.canceled -= OnBrakeCancelled;
        input.Player.Look.performed -= OnLook;
        input.Player.Zoom.performed -= OnZoom;
    }

    private void OnBrakeStarted(InputAction.CallbackContext context) => brake = true;
    private void OnBrakeCancelled(InputAction.CallbackContext context) => brake = false;

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
        if(Physics.SphereCast(transform.position, orbitCamera.nearClipPlane, -transform.forward, out var hit, newDist + orbitCamera.nearClipPlane, layerMask))
            newDist = Mathf.Min(newDist, hit.distance - orbitCamera.nearClipPlane);
        orbitCamera.transform.localPosition = Vector3.back * newDist;
    }
}
