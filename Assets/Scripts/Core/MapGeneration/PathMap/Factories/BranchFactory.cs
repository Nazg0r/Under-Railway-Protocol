using System.Collections.Generic;
using System.Linq;

public class BranchFactory
{
	public List<Branch> Create(PathMapConfig config, string pathMapName)
	{
		List<Branch> branches = new(config.BranchNum);

		for (var i = 0; i < config.BranchNum; i++)
		{
			string name = $"{pathMapName}-Branch{i}";

			branches.Add(new Branch(name));
		}

		return branches;
	}
}