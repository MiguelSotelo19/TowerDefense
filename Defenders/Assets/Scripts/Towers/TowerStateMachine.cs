using System.Collections;
using UnityEngine;

public enum TowerState
{
    Idle,
    Attacking
}

public class TowerStateMachine
{
    private Tower tower;
    private TowerState currentState;
    private Coroutine attackCoroutine;

    public TowerStateMachine(Tower tower)
    {
        this.tower = tower;
        currentState = TowerState.Idle;
    }

    public void Update()
    {
        switch (currentState)
        {
            case TowerState.Idle:
                if (tower.Target != null)
                    ChangeState(TowerState.Attacking);
                break;

            case TowerState.Attacking:
                if (tower.Target == null)
                    ChangeState(TowerState.Idle);
                break;
        }
    }

    private void ChangeState(TowerState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        switch (newState)
        {
            case TowerState.Idle:
                if (attackCoroutine != null)
                    tower.StopCoroutine(attackCoroutine);
                break;

            case TowerState.Attacking:
                attackCoroutine = tower.StartCoroutine(tower.FireRoutine());
                break;
        }
    }
}
