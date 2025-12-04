using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class CameraSwitcherInput : MonoBehaviour
{
    [Header("Main cameras")]
    public CinemachineCamera playerCamera;
    public PlayerController playerController;
    public float interactDistance = 5f;
    public LayerMask monitorLayer;

    [Header("Switching speed")]
    public float transitionSpeed = 3f;

    private bool isActive = false;
    private GameInputs playerControls;
    private CinemachineCamera activeMonitorCamera;
    private MonitorTarget activeMonitor;
    private LocalEventManager activeMonitorEvents;

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new GameInputs();
            playerControls.Interact.Focus.performed += OnActivate;
            playerControls.Interact.Blur.performed += OnExit;
        }

        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Start()
    {
        if (playerCamera != null)
            playerCamera.Priority = 10;
    }

    private void OnActivate(InputAction.CallbackContext context)
    {

        if (isActive)
            return;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, monitorLayer))
        {
            MonitorTarget monitor = hit.collider.GetComponentInParent<MonitorTarget>();
            if (monitor != null && monitor.monitorCamera != null)
            {
                activeMonitor = monitor;
                ActivateCamera(monitor.monitorCamera);
            }
        }
    }

    private void OnExit(InputAction.CallbackContext context)
    {
        if (isActive)
        {
            DeactivateCamera();
        }
    }

    private void ActivateCamera(CinemachineCamera monitorCam)
    {
        if (playerCamera == null || monitorCam == null)
            return;

        activeMonitorCamera = monitorCam;

        playerCamera.Priority = 10;
        activeMonitorCamera.Priority = 20;

        if (playerController) playerController.isFocusActive = true;
        isActive = true;

        activeMonitorEvents = activeMonitor.GetComponent<LocalEventManager>();
        activeMonitorEvents?.Invoke("OnFocus");
    }

    private void DeactivateCamera()
    {
        if (activeMonitorCamera == null)
            return;

        activeMonitorCamera.Priority = 0;
        playerCamera.Priority = 10;
        activeMonitorCamera = null;
        
        if (playerController) playerController.isFocusActive = false;
        isActive = false;

        activeMonitorEvents = activeMonitor.GetComponent<LocalEventManager>();
        activeMonitorEvents?.Invoke("OnBlur");
    }
}
