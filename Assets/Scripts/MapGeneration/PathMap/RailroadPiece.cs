using UnityEngine;

public class RailroadPiece
{
	public DirectionType Type { get;}
	public Vector2 StartPoint { get; set; }
	public Vector2 EndPoint { get; set; }
	public Quaternion Rotation { get;}

	public RailroadPiece(DirectionType type, Vector2 startPoint, Vector2 endPoint)
	{
		Type = type;
		StartPoint = startPoint;
		EndPoint = endPoint;
		Rotation = Quaternion.identity;
		Rotation.eulerAngles.Set(0f, (float)type, 0f);
	}
}
