using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BranchStructureTypeFactory
{
	private readonly BranchType[] _branchTypes = Resources.LoadAll<BranchType>("Branches/Types/");

	public BranchStructureType Create(string name)
	{
		BranchType type = _branchTypes.FirstOrDefault(t => t.Name == name);

		if (type == null) 
			throw new NullReferenceException($"Branch type with name \"{name}\" does not exist");

		List<RailroadPiece> directions = type.Directions
			.Select(direction => 
				new RailroadPiece(
					direction.Type,
					direction.StartPoint,
					direction.EndPoint,
					Quaternion.identity))
			.ToList();

		return new BranchStructureType(type.Name, directions);
	}
}
