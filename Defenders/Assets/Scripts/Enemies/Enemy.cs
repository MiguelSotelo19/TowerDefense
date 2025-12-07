using UnityEngine;
using System.Collections;
using UnityEngine.Splines;

public class Enemy : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [Tooltip("Tiempo de espera para que se reproduzca la animación de muerte antes de devolverlo al Pool.")]
    //[SerializeField] private float deathAnimationDuration = 2.0f; //Aca me imagino que cuando haya animacion va a ser este xd
    [SerializeField] private float deathAnimationDuration = 0.1f;
    [Header("Enemy Stats")]
    [SerializeField] private int damageToCore = 1;
    public int bytes = 0;

    private EnemyHealthBarController healthBar;

    [HideInInspector] public SpawnPoint ownerSpawner;

    private Health enemyHealth;
    private FollowPathAgent followPathAgent;
    public int DamageToCore => damageToCore;


    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        enemyHealth = GetComponent<Health>();
        followPathAgent = GetComponent<FollowPathAgent>();
    }

    private void OnEnable()
    {
        if (animator != null)
            animator.SetBool("IsDead", false);
        if (enemyHealth != null)
        {
            enemyHealth.onDeath.AddListener(OnDeath);
            enemyHealth.onHealthChange.AddListener(HandleHealthChange);
        }
        if (followPathAgent != null)
            followPathAgent.enabled = true;
    }

    protected void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.onDeath.RemoveListener(OnDeath);
            enemyHealth.onHealthChange.RemoveListener(HandleHealthChange);
        }
    }

    private void OnDeath()
    {
        Die(giveReward: true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Core"))
        {
            ReachCore();
        }
    }

    private void HandleHealthChange(float current, float max)
    {
    if (current > 0 && animator != null)
        animator.SetTrigger("Hit");

    if (healthBar != null)
        healthBar.SetHealth(current, max);
    }


    public void ReachCore()
    {
        CoreHealth core = FindFirstObjectByType<CoreHealth>();
        if (core != null)
        {
            core.TakeDamage(DamageToCore);
        }
        Die(giveReward: false);
    }

    public void Die(bool giveReward = true)
    {
        if (animator != null && animator.GetBool("IsDead"))
            return;

        if (EventManager.Instance != null)
            //TEMPORAL - Migue
            //EventManager.Invoke(GlobalEvents.EnemyDied, transform);
            EventManager.Invoke<Enemy>(GlobalEvents.EnemyDied, this);

        if (followPathAgent != null)
            followPathAgent.enabled = false;

        if (giveReward && EconomyManager.Instance != null)
            EconomyManager.Instance.AddBytes(bytes);

        if (animator != null)
            animator.SetBool("IsDead", true);

        //TEMPORAL - Migue
        //StartCoroutine(WaitAndReturnToPool(deathAnimationDuration));
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(WaitAndReturnToPool(deathAnimationDuration));
        }
        else
        {
            ReturnToPoolOrDisable();
        }
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
            ownerSpawner.ReturnToPool(gameObject);
        else
            gameObject.SetActive(false);
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

    public void AssignHealthBar(EnemyHealthBarController bar)
    {
        healthBar = bar;
    }
    
}
