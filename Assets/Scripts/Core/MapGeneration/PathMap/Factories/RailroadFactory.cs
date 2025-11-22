using System.Collections.Generic;

public class RailroadFactory
{
	private readonly RailroadStructureFactory _structureFactory = new();
	public void Create(List<Branch> branches, PathMapConfig config)
	{
		foreach (var connection in config.Structure)
		{
			var startBranch = branches[connection.From];
			var lastBranch = branches[connection.To];

			List<RailroadPiece> railroadStructure = _structureFactory
				.Create(startBranch, lastBranch, connection.structure);
			Railroad road = new(startBranch, lastBranch, railroadStructure);

			startBranch.Roads.Add(road);
			lastBranch.Roads.Add(road);
		}
	}
}