using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Station : IMapNode<PathMap>
{
	public string Name { get; }
	public List<PathMap> Paths { get; } = new();

	List<PathMap> IMapNode<PathMap>.Connections => Paths;

	public Station(string name)
	{
		Name = name;
	}

	public HashSet<Station> GetNeighborStations()
	{
		var neighbors = new HashSet<Station>();

		foreach (var path in Paths)
		{
			if (path.FirstStation != this)
				neighbors.Add(path.FirstStation);
			else if (path.LastStation != this)
				neighbors.Add(path.LastStation);
		}

		return neighbors;
	}

	public PathMap GetPathMapToNeighborStation(Station station)
	{
		var pathMap = Paths.FirstOrDefault(p =>
			p.LastStation == station && p.FirstStation == this ||
			p.LastStation == this && p.FirstStation == station);

		if (pathMap is null)
			Debug.LogError($"Couldn`t find path map to station {station.Name}");

		return pathMap;
	}
}