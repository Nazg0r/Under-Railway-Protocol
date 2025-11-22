using UnityEngine;
using UnityEngine.Events;

public class MapMonitorEventBus : MonoBehaviour
{
	public UnityEvent onBackPressed;
	public UnityEvent onBuildPathPressed;


	public UnityEvent<Station> onStationChosen;
	public UnityEvent<Station> onStationSelected;
	public UnityEvent<Station> onStationDeselected;

	public UnityEvent<float> onRequiredFuelCalculated;


	public void Initialize()
	{
		ObjectResolver.Register(this);
	}
}
