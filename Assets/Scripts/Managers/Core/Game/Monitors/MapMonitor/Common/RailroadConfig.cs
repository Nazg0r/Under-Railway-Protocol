using System;
using System.Collections.Generic;
using UnityEngine;

public class RailroadConfig
{
	public readonly List<GameObject> RailroadSprites;
	public readonly List<Branch> RailroadBranches;
	public readonly Railroad Railroad;

	public RailroadStatus Status { get; private set; }

	public event Action<RailroadStatus> OnStatusChanged;

	public RailroadConfig(List<GameObject> sprites, List<Branch> railroadBranches, RailroadStatus status, Railroad railroad)
	{
		RailroadSprites = sprites;
		RailroadBranches = railroadBranches;
		Status = status;
		Railroad = railroad;
	}

	public void SetStatus(RailroadStatus newStatus)
	{
		if (Status == newStatus) return;
		Status = newStatus;
		OnStatusChanged?.Invoke(newStatus);
	}
}