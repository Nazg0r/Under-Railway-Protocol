using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Look")]
    public float mouseSensitivity = 0.1f;

    [HideInInspector] public bool isFocusActive = false;
    private float xRotation = 0f;

    private PlayerControls playerControls;
    private Vector2 moveInput;
    public Vector2 MoveInput => moveInput;
    private Vector2 lookInput;

    [SerializeField] private Transform cameraRoot;

    void Awake()
    {
        playerControls = new PlayerControls();

        playerControls.Camera.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        playerControls.Camera.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    void OnEnable() => playerControls.Enable();
    void OnDisable() => playerControls.Disable();

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
    }

    private void HandleLook()
    {
        float currentSensitivity = isFocusActive ? 0f : mouseSensitivity;
        float mouseX = lookInput.x * currentSensitivity;
        float mouseY = lookInput.y * currentSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraRoot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }
}
