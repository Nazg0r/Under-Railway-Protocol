using System;
using System.Collections.Generic;
using UnityEngine;

public class RailroadStructureFactory
{
	public List<RailroadPiece> Create(Branch from, Branch to, List<string> structure)
	{
		List<RailroadPiece> railroadStructure = new(structure.Count);
		Vector2 start = from.BasePoint;

		RailroadPiece current = CreateNextPiece(start, Quaternion.identity, structure[0]);
		railroadStructure.Add(current);

		for (int i = 1; i < structure.Count; i++)
		{
			string label = structure[i];
			current = CreateNextPiece(current.EndPoint, current.Rotation, label);
			railroadStructure.Add(current);
		}

		to.BasePoint = current.EndPoint;

		return railroadStructure;
	}

	private RailroadPiece CreateNextPiece(Vector2 startPoint, Quaternion rotation, string label)
	{
		var isParsed = Enum.TryParse(label, out DirectionType type);

		if (!isParsed) throw new ArgumentException($"Error while parsing Railroad label: \"{label}\"");

		float sqrtTwo = (float)Math.Sqrt(2);
		float angle = type switch
		{
			DirectionType.Forward => 0f,
			DirectionType.Left => 45f,
			DirectionType.Right => -45f,
			_ => 0f
		};

		Quaternion rotateTo = Quaternion.Euler(0, angle, 0);
		Quaternion newRotation = rotation * rotateTo;


		Vector2 delta = type switch
		{
			DirectionType.Forward => new Vector2(0, 1),
			DirectionType.Left => new Vector2(sqrtTwo, sqrtTwo),
			DirectionType.Right => new Vector2(-sqrtTwo, sqrtTwo),
			_ => Vector2.zero
		};

		return new RailroadPiece(type, startPoint, startPoint + delta, newRotation);

	}
}
