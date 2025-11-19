using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBars : MonoBehaviour
{
    // Diccionario: Transform del enemigo -> RectTransform de la barra (active)
    private Dictionary<Transform, RectTransform> activeBars = new();

    // Stack como pool simple (LIFO)
    private Stack<RectTransform> pool = new();

    [SerializeField] private Image barPrefab;   // prefab de Image (UI) asignado en inspector
    [SerializeField] private Camera mainCamera; // asignar en inspector o se busca Camera.main

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        EventManager.Subscribe<Transform>(GlobalEvents.SpawnEnemy, AddEnemy);
        EventManager.Subscribe<Transform>(GlobalEvents.EnemyDied, RemoveEnemy);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<Transform>(GlobalEvents.SpawnEnemy, AddEnemy);
        EventManager.Unsubscribe<Transform>(GlobalEvents.EnemyDied, RemoveEnemy);
    }

    private void Update()
    {
        // Actualiza la posición de las barras activas según la posición del enemigo
        var toRemove = new List<Transform>(); // por si necesitamos limpiar refs nulas
        foreach (var kv in activeBars)
        {
            var enemy = kv.Key;
            var bar = kv.Value;

            if (enemy == null)
            {
                toRemove.Add(enemy);
                continue;
            }

            Vector2 screenPos = mainCamera.WorldToScreenPoint(enemy.position);
            bar.anchoredPosition = screenPos;
        }

        // Limpieza de referencias nulas (seguro)
        foreach (var t in toRemove)
        {
            if (activeBars.ContainsKey(t))
            {
                var b = activeBars[t];
                b.gameObject.SetActive(false);
                pool.Push(b);
                activeBars.Remove(t);
            }
        }
    }

    private RectTransform GetBarFromPool()
    {
        if (pool.Count > 0)
        {
            var bar = pool.Pop();
            bar.gameObject.SetActive(true);
            return bar;
        }

        // crear nueva si pool vacío
        var instance = Instantiate(barPrefab, transform);
        return instance.GetComponent<RectTransform>();
    }

    public void AddEnemy(Transform enemy)
    {
        if (enemy == null) return;
        if (activeBars.ContainsKey(enemy)) return; // ya tiene una asignada

        var bar = GetBarFromPool();
        // opcional: reset visual del fill / texto
        var image = bar.GetComponent<Image>();
        if (image != null) image.fillAmount = 1f;

        activeBars.Add(enemy, bar);
    }

    public void RemoveEnemy(Transform enemy)
    {
        if (enemy == null) return;

        if (!activeBars.TryGetValue(enemy, out var bar)) return;

        bar.gameObject.SetActive(false);
        pool.Push(bar);
        activeBars.Remove(enemy);
    }
}
