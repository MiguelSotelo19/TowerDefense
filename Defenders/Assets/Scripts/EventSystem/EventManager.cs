using System;
using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(-1000)]
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    private static readonly Dictionary<GlobalEvents, List<Delegate>> Events = new();

    private void Awake()
    {
        
        Instance = this;

        foreach (var eventName in Enum.GetValues(typeof(GlobalEvents)))
        {
            Events.Add((GlobalEvents)eventName, new List<Delegate>());
        }
    }
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    public static void Subscribe<T>(GlobalEvents globalEvent, Action<T> method)
    {
        if (Instance == null) return;
        Events[globalEvent].Add(method);
    }

    public static void Subscribe(GlobalEvents globalEvent, Action method)
    {
        if (Instance == null) return;
        Events[globalEvent].Add(method);
    }

    public static void Unsubscribe<T>(GlobalEvents globalEvent, Action<T> method)
    {
        if (Instance == null) return;
        Events[globalEvent].Remove(method);
    }

    public static void Unsubscribe(GlobalEvents globalEvent, Action method)
    {
        if (Instance == null) return;
        Events[globalEvent].Remove(method);
    }

    public static void Invoke<T>(GlobalEvents globalEvent, T value = default)
    {
        if (Instance == null) return;
        if (!Events.ContainsKey(globalEvent)) return;

        foreach (var @delegate in Events[globalEvent])
        {
            if (@delegate is Action<T> action)
                action.Invoke(value);
        }
    }

    public static void Invoke(GlobalEvents globalEvent)
    {
        if (Instance == null) return;
        if (!Events.ContainsKey(globalEvent)) return;

        foreach (var @delegate in Events[globalEvent])
        {
            if (@delegate is Action action)
                action.Invoke();
        }
    }
    public static void ClearAll()
    {
        if (Events == null) return;

        foreach (var key in Events.Keys)
        {
            Events[key].Clear();
        }

        Debug.Log("[EventManager] All events cleared.");
    }

}