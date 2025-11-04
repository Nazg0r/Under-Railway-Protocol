using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathMapFactory
{
	private string _pathMapName = "StationName-StationName";
	private readonly BranchTypes _branchTypes = Resources.Load<BranchTypes>("Configs/BranchTypes");

	public PathMap Create(PathMapConfig config, List<Station> stations)
	{
		_pathMapName = $"{stations[0].Name}-{stations[1].Name}";
		List<Branch> branches = CreateBranches(config);
		CreateRailroads(branches, config);
		return new PathMap(config.Id, branches.First(), stations[0], stations[1]);
	}

	private List<Branch> CreateBranches(PathMapConfig config)
	{
		List<Branch> branches = new(config.BranchNum);

		for (var i = 0; i < config.BranchNum; i++)
		{
			int railroadsCount = config.Structure.Count(c => c.From == i || c.To == i);

			var type = _branchTypes
				.Bindings
				.FirstOrDefault(b => b.RailroadsCount == railroadsCount)
				.TypeName;

			string name = $"{_pathMapName}-Branch{i}";
			branches.Add(new Branch(type, name));
		}

		return branches;
	}

	private void CreateRailroads(List<Branch> branches, PathMapConfig config)
	{
		foreach (var connection in config.Structure)
		{
			var startBranch = branches[connection.From];
			var lastBranch = branches[connection.To];

			Railroad road = new(startBranch, lastBranch, connection.structure);

			startBranch.Roads.Add(road);
			lastBranch.Roads.Add(road);
		}
	}
}