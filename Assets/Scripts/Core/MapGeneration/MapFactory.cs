using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class MapFactory
{
	private readonly PathMapFactory _factory = new();
	private readonly PathMapConfig[] _pathMapConfigs = Resources.LoadAll<PathMapConfig>("PathMaps/");
	private readonly MapConfig _mapConfig = Resources.Load<MapConfig>("Configs/MapConfig");
	private readonly BranchStructures[] _BranchStructuresBindings = Resources.LoadAll<BranchStructures>("Branches/Bindings/");

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
		if (_pathMapConfigs.Length != _BranchStructuresBindings.Length)
			throw new AssertionException($"Number of {nameof(PathMapConfig)} and {nameof(BranchStructures)} must be equal", "Check your configs");

		int length = _pathMapConfigs.Length;

		for (int i = 0; i < length; i++)
		{
			var config = _pathMapConfigs[i];
			var branchBinding = _BranchStructuresBindings[i];

			List<Station> stationsList = new()
			{
				stations[config.From],
				stations[config.To]
			};
			PathMap pathMap = _factory.Create(config, stationsList, branchBinding);

			stationsList[0].Paths.Add(pathMap);
			stationsList[1].Paths.Add(pathMap);
		}
	}
}