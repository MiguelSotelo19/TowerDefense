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
    
    // Variables para el congelamiento
    private bool isFrozen = false;
    private Renderer enemyRenderer;
    private Color originalColor;


    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        enemyHealth = GetComponent<Health>();
        followPathAgent = GetComponent<FollowPathAgent>();
        enemyRenderer = GetComponentInChildren<Renderer>();
        
        if (enemyRenderer != null)
            originalColor = enemyRenderer.material.color;
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
        
        isFrozen = false;
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
    
    // ===== NUEVOS MÉTODOS PARA CONGELAMIENTO =====
    
    public void SetFrozen(bool frozen, Color freezeColor)
    {
        isFrozen = frozen;
        
        if (followPathAgent != null)
        {
            // Desactivar/activar el componente para detener completamente el movimiento
            followPathAgent.enabled = !frozen;
            
            // Si se descongela, asegurarse de que el Rigidbody esté activo
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null && frozen)
            {
                rb.linearVelocity = Vector3.zero;
            }
        }
        
        // Pausar o reanudar el animator
        if (animator != null)
        {
            animator.speed = frozen ? 0f : 1f;
        }
        
        // Cambiar color visual
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = frozen ? freezeColor : originalColor;
        }
        
        Debug.Log($"Enemigo {gameObject.name} congelado: {frozen}");
    }
    
    public bool IsFrozen()
    {
        return isFrozen;
    }
}
