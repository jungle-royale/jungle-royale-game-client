using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus<T> where T : Enum
{
    private static readonly Dictionary<string, Delegate> events = new Dictionary<string, Delegate>();

    public static void Subscribe(T eventType, Action listener)
    {
        string key = eventType.ToString(); // enum의 이름을 문자열로 가져옴
        if (!events.ContainsKey(key))
        {
            events[key] = listener;
        }
        else
        {
            events[key] = Delegate.Combine(events[key], listener);
        }
    }

    public static void Subscribe<U>(T eventType, Action<U> listener)
    {
        string key = eventType.ToString(); // enum의 이름을 문자열로 가져옴
        if (!events.ContainsKey(key))
        {
            events[key] = listener;
        }
        else
        {
            events[key] = Delegate.Combine(events[key], listener);
        }
    }

    public static void Unsubscribe<U>(T eventType, Action<U> listener)
    {
        string key = eventType.ToString(); // enum의 이름을 문자열로 가져옴
        if (events.ContainsKey(key))
        {
            var currentDel = Delegate.Remove(events[key], listener);
            if (currentDel == null)
                events.Remove(key);
            else
                events[key] = currentDel;
        }
    }

    public static void Publish<U>(T eventType, U eventData)
    {
        string key = eventType.ToString(); // enum의 이름을 문자열로 가져옴
        // Debug.Log($"{key}: PUblish");
        if (events.ContainsKey(key) && events[key] is Action<U> callback)
        {
            callback.Invoke(eventData);
        }
    }
}