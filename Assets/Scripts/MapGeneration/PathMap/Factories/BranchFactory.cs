using System.Collections.Generic;
using System.Linq;

public class BranchFactory
{
	private readonly BranchStructureTypeFactory _branchTypeFactory = new();

	public List<Branch> Create(PathMapConfig config, BranchStructures structures, string pathMapName)
	{
		List<Branch> branches = new(config.BranchNum);

		for (var i = 0; i < config.BranchNum; i++)
		{
			string name = $"{pathMapName}-Branch{i}";
			var structureName = structures
				.Bindings
				.FirstOrDefault(b => b.BranchIndex == i)
				.StructureName;
			var type = _branchTypeFactory.Create(structureName);
			branches.Add(new Branch(type, name));
		}

		return branches;
	}
}