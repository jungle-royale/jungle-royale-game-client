using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class InGameEventBus
{
    private static readonly IDictionary<InGameGUIEventType, UnityEvent> Events = new Dictionary<InGameGUIEventType, UnityEvent>();

    public static void Subscribe(InGameGUIEventType eventType, UnityAction listener)
    {
        UnityEvent thisEvent;
        if (Events.TryGetValue(eventType, out thisEvent)) 
        {
            thisEvent.AddListener(listener);
        }
        else 
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            Events.Add(eventType, thisEvent);
        }
    }

    public static void Unsubscribe(InGameGUIEventType eventType, UnityAction listener)
    {
        UnityEvent thisEvent;
        if (Events.TryGetValue(eventType, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public static void Publish(InGameGUIEventType eventType)
    {
        UnityEvent thisEvent;
        if (Events.TryGetValue(eventType, out thisEvent))
        {
            thisEvent.Invoke();
        }
    }


}