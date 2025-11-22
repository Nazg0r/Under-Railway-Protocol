using System.Collections.Generic;

public class BranchStructureType
{
	public string Name { get; }
	public List<RailroadPiece> Directions { get; }

	public BranchStructureType(string name, List<RailroadPiece> directions)
	{
		Name = name;
		Directions = directions;
	}

}
