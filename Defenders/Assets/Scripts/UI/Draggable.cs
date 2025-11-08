using System;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    private RectTransform _graphic;
    private Action _currentState;
    private TowerSpot _currentTowerSpot;
    private Vector2 _originalPosition;

    private void Awake()
    {
        _graphic = GetComponent<RectTransform>();
        _originalPosition = _graphic.anchoredPosition;
    }

    private void OnEnable()
    {
        EventManager.Subscribe<TowerSpot>(GlobalEvents.MouseEnterTower, MouseEnterTower);
        EventManager.Subscribe<TowerSpot>(GlobalEvents.MouseExitTower, MouseExitTower);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<TowerSpot>(GlobalEvents.MouseEnterTower, MouseEnterTower);
        EventManager.Unsubscribe<TowerSpot>(GlobalEvents.MouseExitTower, MouseExitTower);
    }

    private void Start()
    {
        _currentState = IdleState;
    }

    private void Update()
    {
        _currentState?.Invoke();
    }

    private void DraggingState()
    {
        _graphic.Translate(Input.mousePositionDelta);
        if (Input.GetMouseButtonUp(0))
        {
            _currentState = IdleState;
            if (_currentTowerSpot)
            {
                Debug.Log($"Purchased tower");
            }
        }
    }

    private void IdleState()
    {
        _graphic.anchoredPosition = Vector2.Lerp(_graphic.anchoredPosition, _originalPosition, Time.deltaTime * 5f);
        if (Input.GetMouseButtonDown(0) && GetInsideBounds()) _currentState = DraggingState;
    }

    private bool GetInsideBounds()
    {
        var position = _graphic.anchoredPosition;
        var mousePos = Input.mousePosition;
        var maxX = position.x + _graphic.rect.width / 2;
        var minX = position.x - _graphic.rect.width / 2;
        var maxY = position.y + _graphic.rect.height / 2;
        var minY = position.y - _graphic.rect.height / 2;
        return mousePos.x < maxX && mousePos.x > minX && mousePos.y < maxY && mousePos.y > minY;
    }

    private void MouseEnterTower(TowerSpot spot) => _currentTowerSpot = spot;

    private void MouseExitTower(TowerSpot spot) => _currentTowerSpot = null;
}
