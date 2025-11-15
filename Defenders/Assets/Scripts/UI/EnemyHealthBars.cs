using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBars : MonoBehaviour
{
    private List<(Transform body, RectTransform bar)> enemies = new();
    [SerializeField] private Image barPrefab;
    [SerializeField] private Camera mainCamera;

    void OnEnable()
    {
        EventManager.Subscribe<Transform>(GlobalEvents.SpawnEnemy, AddEnemy);
    }

    void Oisable()
    {
        EventManager.Unsubscribe<Transform>(GlobalEvents.SpawnEnemy, AddEnemy);        
    }

    void Update()
    {
        foreach (var enemy in enemies)
        {
            enemy.bar.anchoredPosition = mainCamera.WorldToScreenPoint(enemy.body.position);
        }
    }

    public void AddEnemy(Transform enemy)
    {
        var instance = Instantiate(barPrefab, transform);
        var rectTransform = instance.GetComponent<RectTransform>();
        enemies.Add((enemy, rectTransform));
    }
}
