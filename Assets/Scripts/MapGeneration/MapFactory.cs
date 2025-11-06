using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapFactory
{
	private readonly PathMapFactory _factory = new();
	private readonly PathMapConfig[] _pathMapConfigs = Resources.LoadAll<PathMapConfig>("PathMaps/");
	private readonly MapConfig _mapConfig = Resources.Load<MapConfig>("Configs/MapConfig");

	public Map Create()
	{
		List<Station> stations = CreateStations();
		CreatePaths(stations);
		return new Map(stations.First());
	}

	private List<Station> CreateStations()
	{
		var names = _mapConfig.StationNames;
		List<Station> stations = new(names.Count);
		stations.AddRange(names.Select(t => new Station(t)));
		return stations;
	}

	private void CreatePaths(List<Station> stations)
	{
		foreach (var config in _pathMapConfigs)
		{
			List<Station> stationsList = new()
			{
				stations[config.From],
				stations[config.To]
			};

			PathMap pathMap = _factory.Create(config, stationsList);

			stationsList[0].Paths.Add(pathMap);
			stationsList[1].Paths.Add(pathMap);
		}
	}
}