using UnityEngine;

public class WorldMapBuildingPreparationManager : MonoBehaviour
{
	[SerializeField] private WorldMapStationBuildingManager _worldMapStationBuildingManager;
	[SerializeField] private WorldMapBranchBuildingManager _worldMapBranchBuildingManager;
	[SerializeField] private WorldMapPathMapBuildingManager _worldMapPathMapBuildingManager;
	[SerializeField] private WorldMapBuildingManager _worldMapBuildingManager;

	public void Initialize()
	{
		ObjectResolver.Register(this);

		_worldMapStationBuildingManager.Initialize();
		_worldMapBranchBuildingManager.Initialize();
		_worldMapPathMapBuildingManager.Initialize();
		_worldMapBuildingManager.Initialize();
	}
}
