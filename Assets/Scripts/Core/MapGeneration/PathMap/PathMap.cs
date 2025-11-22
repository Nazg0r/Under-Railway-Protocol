using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class PathMap : MapBase<Branch, Railroad>, IMapConnection<Station>
{
	public string Id { get; }
	public Station FirstStation { get; }
	public Station LastStation { get; }
	public Branch InitialBranch { get; set; }

	Station IMapConnection<Station>.FirstNode => FirstStation;
	Station IMapConnection<Station>.LastNode => LastStation;

	public PathMap(string id, Branch initialBranch, Station firstStation, Station lastStation)
	{
		Id = id;
		InitialBranch = initialBranch;
		FirstStation = firstStation;
		LastStation = lastStation;
	}

	public List<Branch> GetBranches() =>
		GetNodes(InitialBranch);

	public List<Railroad> GetRailroads() =>
		GetConnections(InitialBranch);


	[CanBeNull]
	public Branch GetBranchByName(string name) =>
		GetNodeByName(InitialBranch, name);

	public PathMapDimensions DefineDimensions()
	{
		var allPoints = GetRailroads()
			.SelectMany(r => r.RailroadStructure)
			.SelectMany(s => new[] { s.StartPoint, s.EndPoint })
			.Distinct()
			.ToList();

		float minX = allPoints.Min(p => p.x);
		float maxX = allPoints.Max(p => p.x);
		float minY = allPoints.Min(p => p.y);
		float maxY = allPoints.Max(p => p.y);

		return new PathMapDimensions(new Vector2(minX, maxX), new Vector2(minY, maxY));
	}

}

public struct PathMapDimensions
{
	private readonly Vector2 _x;
	private readonly Vector2 _y;
	public float K;

	public float MinX => _x.x * K;
	public float MaxX => _x.y * K;
	public float MinY => _y.x * K;
	public float MaxY => _y.y * K;

	public PathMapDimensions(Vector2 x, Vector2 y, float k = 1)
	{
		K = k;
		_x = x;
		_y = y;
	}
}
