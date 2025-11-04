using System.Collections.Generic;

public class Railroad : IMapConnection<Branch>
{
	public Branch FirstBranchNode { get; }
	public Branch LastBranchNode { get; }
	public List<string> PieceStructure { get; }
	public List<IAnomaly> Anomalies { get; set; }

	Branch IMapConnection<Branch>.FirstNode => FirstBranchNode;
	Branch IMapConnection<Branch>.LastNode => LastBranchNode;


	public Railroad(Branch firstBranchNode, Branch lastBranchNode, List<string> pieceStructure)
	{
		FirstBranchNode = firstBranchNode;
		LastBranchNode = lastBranchNode;
		PieceStructure = pieceStructure;
	}
}
