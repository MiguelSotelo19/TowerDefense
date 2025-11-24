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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var eventName in Enum.GetValues(typeof(GlobalEvents)))
        {
            Events.Add((GlobalEvents)eventName, new List<Delegate>());
        }
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
}