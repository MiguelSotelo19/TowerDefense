using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public float range = 10f;
    public string enemyTag = "Enemy";
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject firePoint;
    [SerializeField] private GameObject bulletPool;
    private List<Bullet> bullets = new();
    private Transform target;
    [SerializeField] private GameObject rangeDisplay;
    private TowerStateMachine stateMachine;
    [SerializeField] private AudioSource shootSound;

    public Transform Target
    {
        get { return target; }
    }

    public enum TowerState
    {
        Idle,
        Attacking
    }

    private void Awake()
    {
        for (int i = 0; i < 50; i++)
        {
            var instance = Instantiate(bulletPrefab, bulletPool.transform);
            var bullet = instance.GetComponent<Bullet>();
            bullets.Add(bullet);
            instance.SetActive(false);
        }
        stateMachine = new TowerStateMachine(this);
    }

    private void Start()
    {
        if (rangeDisplay != null)
        {
            rangeDisplay.transform.localScale = new Vector3(range / 5f, 1f, range / 5f);
            rangeDisplay.SetActive(false);
        }
    }

    private void Update()
    {
        ClosestEnemy();
        AimAtTarget();
        stateMachine.Update();
    }

    public IEnumerator FireRoutine()
    {
        while (Target != null)
        {
            var available = bullets.FirstOrDefault(x => !x.gameObject.activeInHierarchy);
            if (available)
            {
                available.direction = firePoint.transform.up;
                available.transform.position = firePoint.transform.position;
                available.gameObject.SetActive(true);

                if (shootSound != null)
                shootSound.Play();
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ClosestEnemy()
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (Enemy enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);

            if (distanceToEnemy < shortestDistance && distanceToEnemy <= range)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy.transform;
            }
        }

        target = nearestEnemy;
    }

    private void AimAtTarget()
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = lookRotation.eulerAngles;

        transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    private void OnMouseEnter()
    {
        if (rangeDisplay != null)
            rangeDisplay.SetActive(true);
    }

    private void OnMouseExit()
    {
        if (rangeDisplay != null)
            rangeDisplay.SetActive(false);
    }
}
