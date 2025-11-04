using System.Collections.Generic;
using JetBrains.Annotations;

public class PathMap : MapBase<Branch, Railroad>, IMapConnection<Station>
{
	public string Id { get; }
	public Station FirstStation { get; }
	public Station LastStation { get; }
	public Branch InitialBranch { get; set; }

	Station IMapConnection<Station>.FirstNode => FirstStation;
	Station IMapConnection<Station>.LastNode => LastStation;

	public PathMap(string id, Branch initialBranch, Station firstStation, Station lastStation)
	{
		Id = id;
		InitialBranch = initialBranch;
		FirstStation = firstStation;
		LastStation = lastStation;
	}

	public List<Branch> GetBranches() =>
		GetNodes(InitialBranch);

	public List<Railroad> GetRailroads() =>
		GetConnections(InitialBranch);


	[CanBeNull]
	public Branch GetBranchByName(string name) =>
		GetNodeByName(InitialBranch, name);

}
