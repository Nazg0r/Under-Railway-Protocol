using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Look")]
    public float mouseSensitivity = 0.1f;

    [HideInInspector] public bool isFocusActive = false;
    private float _xRotation = 0f;

    [SerializeField] private Transform cameraRoot;

    private GameInputs _playerControls;
    private Vector2 _lookInput;


    void Awake()
    {
        _playerControls = new GameInputs();

        _playerControls.Camera.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
        _playerControls.Camera.Look.canceled += ctx => _lookInput = Vector2.zero;
    }

    void OnEnable() => _playerControls.Enable();
    void OnDisable() => _playerControls.Disable();

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
        float mouseX = _lookInput.x * currentSensitivity;
        float mouseY = _lookInput.y * currentSensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        cameraRoot.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }
}
