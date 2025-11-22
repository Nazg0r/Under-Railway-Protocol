using System.Collections.Generic;

public interface IMapNode<T>
{
	string Name { get; }
	List<T> Connections { get; }
}
