using System.Collections.Generic;

public class Map : MapBase<Station, PathMap>
{
	public Station InitialStation { get; set; }

	public Map(Station initialStation)
	{
		InitialStation = initialStation;
	}

	public List<Station> GetStations() =>
		GetNodes(InitialStation);

	public List<PathMap> GetPathMaps() =>
		GetConnections(InitialStation);

	public Station GetStationByName(string name) =>
		GetNodeByName(InitialStation, name);
}
