using UnityEngine;
using UnityEngine.InputSystem;

public class MapMonitorInputController : MonoBehaviour
{
	private GameInputs _inputs;

	private MapMonitorEventBus _eventBus;

	private void Awake()
	{
		_inputs = new GameInputs();
	}

	public void Initialize()
	{
		ObjectResolver.Register(this);
		_eventBus = ObjectResolver.Resolve<MapMonitorEventBus>();
		gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		_inputs.MapMonitor.Enable();
		_inputs.MapMonitor.BackToMenu.performed += OnBackToMenuPressed;
		_inputs.MapMonitor.BuildPath.performed += OnBuildPathPressed;
	}

	private void OnDisable()
	{
		_inputs.MapMonitor.BackToMenu.performed -= OnBackToMenuPressed;
		_inputs.MapMonitor.BuildPath.performed -= OnBuildPathPressed;
		_inputs.MapMonitor.Disable();
	}

	private void OnBackToMenuPressed(InputAction.CallbackContext ctx) =>
		_eventBus.onBackPressed.Invoke();

	private void OnBuildPathPressed(InputAction.CallbackContext ctx) =>
		_eventBus.onBuildPathPressed.Invoke();
}
