using System;
using System.Collections.Generic;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

public class RailroadStructureFactory
{
	private static readonly float Sqrt2 = Mathf.Sqrt(2) / 2f;

	private static readonly Dictionary<float, Vector2> Deltas = new()
	{
		{ 0f, Vector2.down },
		{ 45f, new Vector2(Sqrt2, -Sqrt2) },
		{ 90f, Vector2.right },
		{ 135f, new Vector2(Sqrt2, Sqrt2) },
		{ 180f, Vector2.up },
		{ 225f, new Vector2(-Sqrt2, Sqrt2) },
		{ 270f, Vector2.left },
		{ 315f, new Vector2(-Sqrt2, -Sqrt2) }
	};

	private static readonly Dictionary<DirectionType, float> AngleOffsets = new()
	{
		{ DirectionType.Forward, 0f },
		{ DirectionType.SmallForward, 0f },
		{ DirectionType.Left, 45f },
		{ DirectionType.Right, -45f }
	};

	public List<RailroadPiece> Create(Branch from, Branch to, List<string> structure)
	{
		ValidateStructure(structure);

		var railroadStructure = new List<RailroadPiece>(structure.Count);
		Vector2 currentPoint = from.BasePoint;
		Quaternion currentRotation = Quaternion.identity;

		foreach (string label in structure)
		{
			RailroadPiece piece = CreateNextPiece(currentPoint, currentRotation, label);
			railroadStructure.Add(piece);

			currentPoint = piece.EndPoint;
			currentRotation = piece.Rotation;
		}

		to.BasePoint = currentPoint;
		return railroadStructure;
	}

	private RailroadPiece CreateNextPiece(Vector2 startPoint, Quaternion rotation, string label)
	{
		DirectionType type = ParseDirectionType(label);
		float angleOffset = AngleOffsets[type];
		Quaternion resultRotation = rotation * Quaternion.Euler(0, angleOffset, 0);

		// Vector2 endPoint = type == DirectionType.Forward
		// 	? CalculateStraightEndPoint(startPoint, rotation.eulerAngles.y)
		// 	: CalculateTurnEndPoint(startPoint, type, rotation);

		Vector2 endPoint;

		if (type == DirectionType.Forward)
			endPoint = CalculateStraightEndPoint(startPoint, rotation.eulerAngles.y, Vector2.right);
		else if (type == DirectionType.SmallForward)
			endPoint = CalculateStraightEndPoint(startPoint, rotation.eulerAngles.y, new Vector2(Mathf.Sqrt(2) - 1, 0));
		else
			endPoint = CalculateTurnEndPoint(startPoint, type, rotation);

		return new RailroadPiece(type, startPoint, endPoint, resultRotation);
	}

	private Vector2 CalculateStraightEndPoint(Vector2 startPoint, float currentAngle, Vector2 vector)
	{
		Vector2 direction = RotateVector(vector, currentAngle);
		return startPoint + direction;
	}

	private Vector2 CalculateTurnEndPoint(Vector2 startPoint, DirectionType type, Quaternion rotation)
	{
		float rotationAngle = type == DirectionType.Right ? -45f : 45f;
		float lookupAngle = type == DirectionType.Right
			? Mathf.Round(rotation.eulerAngles.y)
			: Mathf.Round(rotation.eulerAngles.y + 180) % 360;

		Vector2 pivotPoint = startPoint + Deltas[lookupAngle];
		Vector2 relativeVector = startPoint - pivotPoint;
		Vector2 rotatedVector = RotateVector(relativeVector, rotationAngle);

		return pivotPoint + rotatedVector;
	}

	private Vector2 RotateVector(Vector2 vector, float degrees)
	{
		float radians = degrees * Mathf.Deg2Rad;
		float cos = Mathf.Cos(radians);
		float sin = Mathf.Sin(radians);

		return new Vector2(
			vector.x * cos - vector.y * sin,
			vector.x * sin + vector.y * cos
		);
	}

	private void ValidateStructure(List<string> structure)
	{
		if (structure == null)
			throw new ArgumentNullException(nameof(structure));

		if (structure.Count == 0)
			throw new ArgumentException("Structure cannot be empty", nameof(structure));
	}

	private DirectionType ParseDirectionType(string label)
	{
		if (!Enum.TryParse(label, out DirectionType type))
			throw new ArgumentException(
				$"Invalid railroad label: \"{label}\". Expected: {string.Join(", ", Enum.GetNames(typeof(DirectionType)))}");

		return type;
	}
}
