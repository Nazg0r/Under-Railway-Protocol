using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapMonitorController : MonoBehaviour
{
	private MainConfig _mainConfig;

	private GameManager _gameManager;
	private MapMonitorManager _mapMonitorManager;
	private MapMonitorRoadsManager _mapMonitorRoadsManager;
	private MapMonitorPointsManager _mapMonitorPointsManager;
	private MapMonitorHudManager _mapMonitorHudManager;
	private MapMonitorChoiceMenuManager _mapMonitorChoiceMenuManager;
	private MapMonitorInputController _mapMonitorInputController;
	private MapMonitorAnimationController _mapMonitorAnimationController;
	private MapMonitorEventBus _eventBus;


	private readonly List<PointConfig> _activePoints = new();
	private readonly List<RailroadConfig> _activeRoads = new();
	private readonly List<RailroadConfig> _anomalyRoads = new();
	private readonly List<PathMap> _configuredPathMaps = new();


	private readonly Dictionary<GameObject, PointConfig> _pointsToConfigs = new();
	private readonly Dictionary<Branch, PointConfig> _branchesToConfig = new();


	private Dictionary<PathMap, GameObject> _pathMapContainers;
	private Dictionary<PathMap, List<PointConfig>> _pointsConfigs;
	private Dictionary<PathMap, List<RailroadConfig>> _railroadsConfigs;


	private IEnumerable<PointConfig> _currentPointsConfig;
	private IEnumerable<RailroadConfig> _currentRailroadsConfig;
	private IEnumerable<GameObject> _currentPoints;
	private PathMap _currentPathMap;
	private GameObject _currentPathMapContainer;
	private PointConfig _currentPointConfig;

	public void Initialize()
    {
		ObjectResolver.Register(this);
		_gameManager = ObjectResolver.Resolve<GameManager>();
		_mapMonitorManager = ObjectResolver.Resolve<MapMonitorManager>();
	    _mapMonitorRoadsManager = ObjectResolver.Resolve<MapMonitorRoadsManager>();
	    _mapMonitorPointsManager = ObjectResolver.Resolve<MapMonitorPointsManager>();
	    _mapMonitorHudManager = ObjectResolver.Resolve<MapMonitorHudManager>();
	    _mapMonitorChoiceMenuManager = ObjectResolver.Resolve<MapMonitorChoiceMenuManager>();
	    _mapMonitorInputController = ObjectResolver.Resolve<MapMonitorInputController>();
	    _mapMonitorAnimationController = ObjectResolver.Resolve<MapMonitorAnimationController>();
	    _eventBus = ObjectResolver.Resolve<MapMonitorEventBus>();

	    _mainConfig = Resources.Load<MainConfig>("Configs/MainConfig");

		_pathMapContainers = _mapMonitorManager.PathMapContainers;
		_railroadsConfigs = _mapMonitorRoadsManager.RailroadsConfigs;
		_pointsConfigs = _mapMonitorPointsManager.PointsConfigs;

		_eventBus.onStationSelected.AddListener((station) => StationSelectionHandler(station, true));
		_eventBus.onStationDeselected.AddListener((station) => StationSelectionHandler(station, false));
		_eventBus.onStationChosen.AddListener(StationChosenHandler);
		_eventBus.onBackPressed.AddListener(BackToMenuPressedHandler);
		_eventBus.onBuildPathPressed.AddListener(BuildPathHandler);
	}

	private void BuildPathHandler()
	{
		var lastPathMapBranch = _currentPathMap.GetBranches().Last();
		var lastActivePointBranch = _activePoints.Last().Branch;
		if (lastPathMapBranch == lastActivePointBranch)
			Debug.Log("Builded");
	}

	private void StationSelectionHandler(Station station, bool isSelected)
	{
		var currentPathMap = _gameManager.CurrentStation.GetPathMapToNeighborStation(station);
		var currentPathMapContainer = _pathMapContainers[currentPathMap];
		currentPathMapContainer.SetActive(isSelected);
	}

	private void StationChosenHandler(Station station)
	{
		_mapMonitorChoiceMenuManager.ChoiceMenu.SetActive(false);
		_mapMonitorHudManager.Hud.SetActive(true);
		_mapMonitorInputController.gameObject.SetActive(true);
		_mapMonitorManager.SetPathMapOnChosenPosition();


		_currentPathMap = _gameManager.CurrentStation.GetPathMapToNeighborStation(station);
		_currentPointsConfig = _pointsConfigs[_currentPathMap];
		_currentRailroadsConfig = _railroadsConfigs[_currentPathMap];
		_currentPathMapContainer = _pathMapContainers[_currentPathMap];
		_currentPoints = _currentPointsConfig.Select(p => p.Point);

		var isConfigured = _configuredPathMaps.Contains(_currentPathMap);

		if (!isConfigured)
		{
			_mapMonitorAnimationController.AddAnimationsToPoints(_currentPointsConfig);
			_pointsToConfigs.AddRange(_currentPointsConfig.ToDictionary(p => p.Point, p => p));
			_branchesToConfig.AddRange(_currentPointsConfig.ToDictionary(p => p.Branch, p => p));
			AddSelectListenerComponentToPoints();
			_configuredPathMaps.Add(_currentPathMap);
		}

		EventSystem.current.SetSelectedGameObject(null);
		EventSystem.current.SetSelectedGameObject(_currentPointsConfig.First().Point.gameObject);
	}

	private void BackToMenuPressedHandler()
	{
		_mapMonitorChoiceMenuManager.ChoiceMenu.SetActive(true);
		_mapMonitorHudManager.Hud.SetActive(false);
		_currentPathMapContainer.SetActive(false);
		_mapMonitorInputController.gameObject.SetActive(false);
		_mapMonitorManager.SetPathMapOnChoicePosition();

		EventSystem.current.SetSelectedGameObject(null);
		EventSystem.current.SetSelectedGameObject(_mapMonitorChoiceMenuManager.ChoiceMenuButtons.First().gameObject);

		_activePoints.Clear();
		_activeRoads.Clear();

		ResetPointsStatus();
		ResetRoadsStatus();

		UpdateRailroadSprites();
		UpdatePointsSprites();
	}

	private void PointDeselectedHandler()
	{
		UpdatePreviousPointsStatus();
		_currentPointConfig.SetStatus(PointStatus.Previous);
	}

	private void PointSelectedHandler(GameObject point)
	{
		_currentPointConfig = _pointsToConfigs[point];

		if (_currentPointConfig is { Status: PointStatus.Active })
		{
			if (_activePoints.Count > 0)
				_activePoints.RemoveAt(_activePoints.Count - 1);
			if(_activeRoads.Count > 0)
				_activeRoads.RemoveAt(_activeRoads.Count - 1);

			DropPreviousPointsStatuses();
		}

		if (!_activePoints.Contains(_currentPointConfig))
			_activePoints.Add(_currentPointConfig);

		var currentRailroadConfig = GetCurrentRailroadConfig();

		if (currentRailroadConfig is not null && !_activeRoads.Contains(currentRailroadConfig))
				_activeRoads.Add(currentRailroadConfig);

		_eventBus.onRequiredFuelCalculated.Invoke(CalculateRequiredFuel());

		UpdateRailroadStatus();
		CheckAnomalyRailroads();

		UpdatePointsActiveStatus();
		UpdateNextChoicePointsStatus();

		SetNavigation();

		_currentPointConfig.SetStatus(PointStatus.OnFocus);

		UpdateRailroadSprites();
		UpdatePointsSprites();
	}

	private void AddSelectListenerComponentToPoints()
	{
		foreach (var point in _currentPoints)
		{
			var localPoint = point;
			var selectListener = localPoint.AddComponent<ButtonSelectListener>();

			selectListener.OnDeselected += PointDeselectedHandler;
			selectListener.OnSelected += () => PointSelectedHandler(point);
		}
	}

	private void SetNavigation()
    {
	    HashSet<Branch> neighborBranches = _currentPointConfig.Branch.GetNeighborBranches();

		foreach (Branch neighborBranch in neighborBranches)
		{
			if (!_branchesToConfig.TryGetValue(neighborBranch, out var neighborPoint))
				continue;

			if (neighborPoint.Status == PointStatus.Active)
				continue;

			var direction = DefineNavigationDirection(neighborPoint);
			if (direction == null)
				continue;

			AssignNavigationDirection(direction, neighborPoint);
		}
    }

	private DirectionType? DefineNavigationDirection(PointConfig neighborPoint)
	{
		var currentBranch = _currentPointConfig.Branch;
		var neighborBranch = neighborPoint.Branch;

		var road = currentBranch.Roads
			.FirstOrDefault(r =>
				(r.FirstBranchNode == currentBranch && r.LastBranchNode == neighborBranch) ||
				(r.FirstBranchNode == neighborBranch && r.LastBranchNode == currentBranch));

		var isIngoing = road.LastBranchNode == currentBranch;

		if (isIngoing && neighborPoint.Status != PointStatus.Previous) return null;

		return !isIngoing ? road.RailroadStructure.First().Type :
			road.RailroadStructure.Last().Type;
	}

	private void AssignNavigationDirection(DirectionType? direction, PointConfig nextPoint)
	{
		var pointButton = _currentPointConfig.Button;
		var navigation = pointButton.navigation;

		if (navigation.mode != Navigation.Mode.Explicit)
			navigation.mode = Navigation.Mode.Explicit;

		if (nextPoint.Status == PointStatus.Previous)
			navigation.selectOnDown = nextPoint.Button;
	
		else if (direction == DirectionType.Forward)
			navigation.selectOnUp = nextPoint.Button;
	
		else if (direction == DirectionType.Left)
			navigation.selectOnLeft = nextPoint.Button;
	
		else if (direction == DirectionType.Right)
			navigation.selectOnRight = nextPoint.Button;

		pointButton.navigation = navigation;
	}

	private RailroadConfig GetCurrentRailroadConfig()
	{
		if (_activePoints.Count < 2) return null;

		var currentBranch = _currentPointConfig.Branch;
		var previousBranch = _activePoints[^2].Branch;

		var roadConfig = _currentRailroadsConfig.FirstOrDefault(c =>
			c.RailroadBranches.Contains(currentBranch) && c.RailroadBranches.Contains(previousBranch));

		if (roadConfig is null)
			Debug.LogError($"No railroad found between {previousBranch.Name} and {currentBranch.Name}");

		return roadConfig;
	}

	private float CalculateRequiredFuel() =>
		 Mathf.Ceil(_activeRoads.Sum(conf =>
			 conf.Railroad.CalculateRailroadLength()) * _mainConfig.FuelPerDistanceUnit);

	private void UpdateRailroadSprites()
	{
		foreach (var config in _currentRailroadsConfig)
			_mapMonitorRoadsManager.UpdateRailroadSprite(config);
	}

	private void UpdateRailroadStatus()
	{
		foreach (var config in _currentRailroadsConfig)
		{
			config.SetStatus(
				_activeRoads.Contains(config) ?
				RailroadStatus.Active :
				RailroadStatus.Passive);
		}
	}

	private void CheckAnomalyRailroads()
	{
		foreach (var config in _anomalyRoads.Where(config => !_activeRoads.Contains(config)))
			config.SetStatus(RailroadStatus.Anomaly);
	}

	private void UpdatePointsSprites()
	{
		foreach (var config in _currentPointsConfig)
			_mapMonitorPointsManager.UpdatePointSprite(config);
	}

	private void UpdatePointsActiveStatus()
    {
	    foreach (var config in _currentPointsConfig.Where(c => c.Status == PointStatus.Active && !_activePoints.Contains(c)))
		    config.SetStatus(PointStatus.Passive);
    }

	private void UpdatePreviousPointsStatus()
	{
		foreach (var config in _currentPointsConfig.Where(c => c.Status == PointStatus.Previous))
			config.SetStatus(PointStatus.Active);
	}

	private void DropPreviousPointsStatuses()
	{
		foreach (var config in _currentPointsConfig.Where(c => c.Status == PointStatus.Previous))
			config.SetStatus(PointStatus.Passive);
	}

	private void ResetPointsStatus()
	{
		foreach (var config in _currentPointsConfig)
			config.SetStatus(PointStatus.Passive);
	}

	private void ResetRoadsStatus()
	{
		foreach (var config in _currentRailroadsConfig)
			config.SetStatus(RailroadStatus.Passive);
	}

	private void UpdateNextChoicePointsStatus()
	{
		var neighborsBranches = _currentPointConfig.Branch.GetNeighborBranches();

		foreach (var config in _currentPointsConfig.Where(c => c.Status == PointStatus.NextChoice))
			config.SetStatus(PointStatus.Passive);

		foreach (var config in _currentPointsConfig.Where(c => neighborsBranches.Contains(c.Branch)))
		{
			if (config.Status is
			    PointStatus.Previous or
			    PointStatus.Active ||
			    IsBranchIngoing(_currentPointConfig.Branch, config.Branch)) continue;

			config.SetStatus(PointStatus.NextChoice);
		}
	}

	private bool IsBranchIngoing(Branch currentBranch, Branch neighborBranch)
	{
		var road = currentBranch.Roads
			.FirstOrDefault(r =>
				(r.FirstBranchNode == currentBranch && r.LastBranchNode == neighborBranch) ||
				(r.FirstBranchNode == neighborBranch && r.LastBranchNode == currentBranch));

		return road.LastBranchNode == currentBranch;
	}
}
