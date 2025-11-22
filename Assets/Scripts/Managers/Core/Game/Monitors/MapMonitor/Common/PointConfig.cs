using System;
using UnityEngine;
using UnityEngine.UI;

public class PointConfig
{
	public readonly Button Button;
	public readonly GameObject Point;
	public readonly Branch Branch;

	public bool IsConfigured { get; set; }
	public PointStatus Status { get; private set; }

	public event Action<PointStatus, GameObject> OnStatusChanged;
	public PointConfig(GameObject point, Branch branch, PointStatus status)
	{
		Point = point;
		Branch = branch;
		Status = status;
		Button = point.GetComponent<Button>();
	}

	public void SetStatus(PointStatus newStatus)
	{
		if (Status == newStatus) return;
		Status = newStatus;
		OnStatusChanged?.Invoke(newStatus, Point);
	}
}