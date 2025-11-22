using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Railroad : IMapConnection<Branch>
{
	public Branch FirstBranchNode { get; }
	public Branch LastBranchNode { get; }
	public List<RailroadPiece> RailroadStructure { get; }
	public List<IAnomaly> Anomalies { get; set; }

	Branch IMapConnection<Branch>.FirstNode => FirstBranchNode;
	Branch IMapConnection<Branch>.LastNode => LastBranchNode;


	public Railroad(Branch firstBranchNode, Branch lastBranchNode, List<RailroadPiece> railroadStructure)
	{
		FirstBranchNode = firstBranchNode;
		LastBranchNode = lastBranchNode;
		RailroadStructure = railroadStructure;
	}

	public float CalculateRailroadLength() =>
		RailroadStructure.Sum(p => Vector2.Distance(p.StartPoint, p.EndPoint));
}
