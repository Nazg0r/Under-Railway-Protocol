using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSelectListener : MonoBehaviour, ISelectHandler, IDeselectHandler
{
	public event Action OnSelected;
	public event Action OnDeselected;
	public void OnSelect(BaseEventData eventData)
	{
		OnSelected.Invoke();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		OnDeselected.Invoke();
	}
}
