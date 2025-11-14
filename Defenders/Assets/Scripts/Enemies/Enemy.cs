using UnityEngine;
using System.Collections;
using UnityEngine.Splines;

public class Enemy : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [Tooltip("Tiempo de espera para que se reproduzca la animación de muerte antes de devolverlo al Pool.")]
    [SerializeField] private float deathAnimationDuration = 2.0f;

    [Header("Enemy Stats")]
    [SerializeField] private int damageToCore = 1;
    public int bytes = 0;

    [HideInInspector] public SpawnPoint ownerSpawner;

    private Health enemyHealth;
    private FollowPathAgent followPathAgent;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        enemyHealth = GetComponent<Health>();
        followPathAgent = GetComponent<FollowPathAgent>();
    }

    private void OnEnable()
    {
        if (animator != null)
        {
            animator.SetBool("IsDead", false);
        }

        if (enemyHealth != null)
        {
            enemyHealth.onDeath.AddListener(Die);
            enemyHealth.onHealthChange.AddListener(HandleHealthChange);
        }

        if (followPathAgent != null)
        {
            followPathAgent.enabled = true;
        }
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.onDeath.RemoveListener(Die);
            enemyHealth.onHealthChange.RemoveListener(HandleHealthChange);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Core"))
        {
            Debug.Log("Llego a core");
            ReachCore();
        }
    }

    private void HandleHealthChange(float current, float max)
    {
        if (current > 0 && animator != null)
        {
            animator.SetTrigger("Hit");
        }
    }

    public void ReachCore()
    {
        CoreHealth core = FindFirstObjectByType<CoreHealth>();
        if (core != null)
        {
            core.TakeDamage(damageToCore);
        }

        Die();
    }

    public void Die()
    {
        if (animator != null && animator.GetBool("IsDead")) return;

        if (followPathAgent != null)
        {
            followPathAgent.enabled = false;
        }

        EconomyManager.Instance.AddBytes(bytes);

        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }

        StartCoroutine(WaitAndReturnToPool(deathAnimationDuration));
    }

    private IEnumerator WaitAndReturnToPool(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPoolOrDisable();
    }

    private void ReturnToPoolOrDisable()
    {
        if (followPathAgent != null)
        {
            followPathAgent.enabled = true;
            followPathAgent.ResetProgress(false);
        }

        if (ownerSpawner != null)
        {
            ownerSpawner.ReturnToPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void Initialize(SpawnPoint spawner)
    {
        ownerSpawner = spawner;

        if (followPathAgent != null)
        {
            followPathAgent.enabled = true;
            followPathAgent.ResetProgress(false);
        }
    }
}