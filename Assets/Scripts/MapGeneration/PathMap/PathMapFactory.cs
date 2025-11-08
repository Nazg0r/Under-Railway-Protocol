using System.Collections.Generic;
using System.Linq;

public class PathMapFactory
{
	private string _pathMapName;
	private readonly BranchStructureTypeFactory _branchTypeFactory = new();

	public PathMap Create(PathMapConfig config, List<Station> stations, BranchStructures structures)
	{
		_pathMapName = $"{stations[0].Name}-{stations[1].Name}";
		List<Branch> branches = CreateBranches(config, structures);
		CreateRailroads(branches, config);
		return new PathMap(config.Id, branches.First(), stations[0], stations[1]);
	}

	private List<Branch> CreateBranches(PathMapConfig config, BranchStructures structures)
	{
		List<Branch> branches = new(config.BranchNum);

		for (var i = 0; i < config.BranchNum; i++)
		{
			string name = $"{_pathMapName}-Branch{i}";
			var structureName = structures
				.Bindings
				.FirstOrDefault(b => b.BranchIndex == i)
				.StructureName;
			var type = _branchTypeFactory.Create(structureName);

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