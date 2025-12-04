using System.Collections.Generic;
using System.Linq;

public class PathMapFactory
{
	private string _pathMapName;
	private readonly BranchFactory _branchFactory = new();
	private readonly RailroadFactory _railroadFactory = new();

	public PathMap Create(PathMapConfig config, List<Station> stations)
	{
		string pathName = $"{stations[0].Name}-{stations[1].Name}";
		List<Branch> branches = _branchFactory.Create(config, pathName);
		_railroadFactory.Create(branches, config);
		return new PathMap(config.Id, branches.First(), stations[0], stations[1]);
	}
}