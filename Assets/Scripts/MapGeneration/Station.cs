using System.Collections.Generic;

public class Station : IMapNode<PathMap>
{
	public string Name { get; }
	public List<PathMap> Paths { get; } = new();

	List<PathMap> IMapNode<PathMap>.Connections => Paths;

	public Station(string name)
	{
		Name = name;
	}
}