using UnityEngine;

public class MapMonitorPreparationManager : MonoBehaviour
{
	[SerializeField] private MapMonitorEventBus _mapMonitorEventBus;
	[SerializeField] private MapMonitorChoiceMenuManager _mapMonitorChoiceMenuManager;
	[SerializeField] private MapMonitorHudManager _mapMonitorHudManager;
	[SerializeField] private MapMonitorRoadsManager _mapMonitorRoadsManager;
	[SerializeField] private MapMonitorPointsManager _mapMonitorPointsManager;
	[SerializeField] private MapMonitorManager _mapMonitorManager;
	[SerializeField] private MapMonitorInputController _mapMonitorInputController;
	[SerializeField] private MapMonitorAnimationController _mapMonitorAnimationController;
	[SerializeField] private MapMonitorController _mapMonitorController;

	public void Initialize()
	{
		ObjectResolver.Register(this);

		_mapMonitorEventBus.Initialize();

		_mapMonitorChoiceMenuManager.Initialize();
		_mapMonitorHudManager.Initialize();
		_mapMonitorRoadsManager.Initialize();
		_mapMonitorPointsManager.Initialize();
		_mapMonitorManager.Initialize();

		_mapMonitorManager.CreateMonitorContent();

		_mapMonitorInputController.Initialize();
		_mapMonitorAnimationController.Initialize();
		_mapMonitorController.Initialize();
	}
}
