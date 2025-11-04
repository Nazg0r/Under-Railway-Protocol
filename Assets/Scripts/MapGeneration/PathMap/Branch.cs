using System.Collections.Generic;

public class Branch : IMapNode<Railroad>
{
	public string Name { get; }
	public string Type { get; }
	public List<Railroad> Roads { get; } = new();

	List<Railroad> IMapNode<Railroad>.Connections => Roads;

	public Branch(string type, string name)
	{
		Type = type;
		Name = name;
	}
}
