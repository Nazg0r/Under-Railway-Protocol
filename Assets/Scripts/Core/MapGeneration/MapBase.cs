using System.Collections.Generic;
using System.Linq;

public abstract class MapBase<T1, T2>
	where T1 : IMapNode<T2>
	where T2 : IMapConnection<T1>
{
	protected List<T1> GetNodes(T1 initial)
	{
		HashSet<T1> visited = new();
		Queue<T1> queue = new();

		queue.Enqueue(initial);
		visited.Add(initial);

		while (queue.Count > 0)
		{
			T1 current = queue.Dequeue();

			foreach (var connection in current.Connections)
			{
				T1 next = connection.LastNode;

				if (visited.Add(next))
					queue.Enqueue(next);
			}
		}

		return visited.ToList();
	}

	protected List<T2> GetConnections(T1 initial)
	{
		List<T1> nodes = GetNodes(initial);

		return nodes.Select(b => b.Connections)
			.SelectMany(r => r)
			.Distinct()
			.ToList();
	}

	protected T1 GetNodeByName(T1 initial, string name)
	{
		List<T1> nodes = GetNodes(initial);

		return nodes.FirstOrDefault(b => b.Name == name);
	}
}