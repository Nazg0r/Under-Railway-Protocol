using System.Collections.Generic;
using UnityEngine;

public class Branch : IMapNode<Railroad>
{
	private Vector2 _basePoint = Vector2.zero;
	public string Name { get; }
	public BranchStructureType Type { get; }
	public List<Railroad> Roads { get; } = new();

	public Vector2 BasePoint { get; set; }
	// {
	// 	get => _basePoint;
	// 	set
	// 	{
	// 		if (_basePoint == Vector2.zero) _basePoint = value;
	// 		else if (_basePoint != value)
	// 			throw new ArgumentException($"Base points are not equal. {_basePoint} vs {value}");
	// 	}
	// }

	List<Railroad> IMapNode<Railroad>.Connections => Roads;

	public Branch(BranchStructureType type, string name)
	{
		Type = type;
		Name = name;
	}
}
