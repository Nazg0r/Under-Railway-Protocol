using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalEventManager : MonoBehaviour
{
    // Dictionary of all events by name
    private Dictionary<string, Action> eventTable = new Dictionary<string, Action>();

    /// <summary>
    /// Subscribe to event
    /// </summary>
    public void Subscribe(string eventName, Action listener)
    {
        if (!eventTable.ContainsKey(eventName))
            eventTable[eventName] = listener;
        else
            eventTable[eventName] += listener;
    }

    /// <summary>
    /// Unsubscribe from event
    /// </summary>
    public void Unsubscribe(string eventName, Action listener)
    {
        if (eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] -= listener;
            if (eventTable[eventName] == null)
                eventTable.Remove(eventName);
        }
    }

    /// <summary>
    /// Event call
    /// </summary>
    public void Invoke(string eventName)
    {
        if (eventTable.ContainsKey(eventName))
            eventTable[eventName]?.Invoke();
    }

    /// <summary>
    /// Clear all events
    /// </summary>
    public void Clear()
    {
        eventTable.Clear();
    }
}
