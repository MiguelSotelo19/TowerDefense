using UnityEngine;
using System.Collections;

public class FreezePowerUp : MonoBehaviour
{
    [Header("Freeze Settings")]
    [SerializeField] private float freezeDuration = 5f;
    [SerializeField] private Color freezeColor = new Color(0.5f, 0.8f, 1f, 1f); // Color azul claro para indicar congelamiento
    
    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownTime = 10f; // Tiempo de recarga en segundos
    
    [Header("UI/Audio (Opcional)")]
    [SerializeField] private AudioClip freezeSound;
    [SerializeField] private ParticleSystem freezeEffect; // Efecto visual opcional
    
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    
    public void ActivateFreezeAll()
    {
        if (isOnCooldown)
        {
            Debug.Log($"La cuarentena está en cooldown! Espera {cooldownTimer:F1} segundos");
            return;
        }
        
        StartCoroutine(FreezeAllEnemies());
    }
    
    private IEnumerator FreezeAllEnemies()
    {
        isOnCooldown = true;
        cooldownTimer = cooldownTime;
        
        // Reproducir sonido si existe
        if (freezeSound != null)
        {
            AudioSource.PlayClipAtPoint(freezeSound, Camera.main.transform.position);
        }
        
        // Encontrar todos los enemigos activos en la escena
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        
        Debug.Log($"Congelando {allEnemies.Length} enemigos por {freezeDuration} segundos");
        
        // Guardar los estados originales y congelar
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy.gameObject.activeInHierarchy)
            {
                enemy.SetFrozen(true, freezeColor);
                
                // Activar efecto visual en cada enemigo si existe
                if (freezeEffect != null)
                {
                    ParticleSystem effect = Instantiate(freezeEffect, enemy.transform.position, Quaternion.identity);
                    effect.transform.SetParent(enemy.transform);
                    Destroy(effect.gameObject, freezeDuration);
                }
            }
        }
        
        // Esperar la duración del congelamiento
        yield return new WaitForSeconds(freezeDuration);
        
        // Descongelar todos los enemigos
        allEnemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy.gameObject.activeInHierarchy)
            {
                enemy.SetFrozen(false, Color.white);
            }
        }
        
        Debug.Log("Cuarentena finalizada. Enemigos liberados.");
        
        // Iniciar el cooldown
        yield return StartCoroutine(CooldownRoutine());
    }
    
    private IEnumerator CooldownRoutine()
    {
        while (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            yield return null;
        }
        
        cooldownTimer = 0f;
        isOnCooldown = false;
        Debug.Log("Cuarentena lista para usar de nuevo!");
    }
    
    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }
    
    public float GetCooldownTimer()
    {
        return cooldownTimer;
    }
    
    public float GetCooldownProgress()
    {
        return 1f - (cooldownTimer / cooldownTime);
    }
}