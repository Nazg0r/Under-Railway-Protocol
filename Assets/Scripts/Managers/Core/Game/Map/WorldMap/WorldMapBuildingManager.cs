using UnityEngine;

public class WorldMapBuildingManager : MonoBehaviour
{
	[SerializeField] private GameObject Container;

	private GameManager _gameManger;
	private WorldMapStationBuildingManager  _stationBuildingManager;
	private WorldMapPathMapBuildingManager _pathMapBuildingManager;

	public void Initialize()
	{
		ObjectResolver.Register(this);
		_gameManger = ObjectResolver.Resolve<GameManager>();
		_stationBuildingManager = ObjectResolver.Resolve<WorldMapStationBuildingManager>();
		_pathMapBuildingManager = ObjectResolver.Resolve<WorldMapPathMapBuildingManager>();

		BuildInitialStation();
	}

	private void BuildInitialStation() =>
		_gameManger.Delta = _stationBuildingManager.CreateStation(Vector2.zero, Container.transform);

	public void BuildNextPathMap(PathMap pathMap)
	{
		_gameManger.Delta = _pathMapBuildingManager.CreatePathMap(_gameManger.Delta, pathMap, Container.transform);
		_gameManger.Delta = _stationBuildingManager.CreateStation(_gameManger.Delta, Container.transform);
	}
}
