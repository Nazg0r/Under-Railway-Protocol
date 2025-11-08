using System.Collections.Generic;

public class Branch : IMapNode<Railroad>
{
	public string Name { get; }
	public BranchStructureType Type { get; }
	public List<Railroad> Roads { get; } = new();

	List<Railroad> IMapNode<Railroad>.Connections => Roads;

	public Branch(BranchStructureType type, string name)
	{
		Type = type;
		Name = name;
	}
}
