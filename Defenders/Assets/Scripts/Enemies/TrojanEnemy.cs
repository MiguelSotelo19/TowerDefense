using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrojanEnemy : Enemy
{
    [Header("Trojan Specific Settings")]
    [SerializeField] private GameObject fragmentPrefab; 
    [SerializeField] private int fragmentCount = 3;
    [SerializeField] private float spawnRadius = 2f; 

    [Header("Visual Settings")]
    [SerializeField] private GameObject normalModel;

    private bool hasSplit = false;
    private float splineProgressAtDeath = 0f;

    private void Awake()
    {
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.onDeath.RemoveAllListeners();
            health.onDeath.AddListener(OnTrojanDeath);
        }
    }

    private void Start()
    {
        // Re-verificar listeners
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.onDeath.RemoveAllListeners();
            health.onDeath.AddListener(OnTrojanDeath);
        }
    }

    private void OnTrojanDeath()
    {
        if (hasSplit) //Este es para que no muera infinito, si ya se petateo una vez ya truena ahora si
        {
            ReturnToPoolOrDisable();
            return;
        }
        SplitIntoFragments();
    }

    private void SplitIntoFragments()
    {
        hasSplit = true;
        Vector3 splitPosition = transform.position;

        var followPath = GetComponent<FollowPathAgent>();

        if (followPath != null)
        {
            splineProgressAtDeath = GetCurrentSplineProgress();
        }

        if (fragmentPrefab == null)
        {
            ReturnToPoolOrDisable();
            return;
        }

        for (int i = 0; i < fragmentCount; i++)
        {
            SpawnFragment(i, splitPosition);
        }

        if (EconomyManager.Instance != null)
            EconomyManager.Instance.AddBytes(bytes);

        ReturnToPoolOrDisable();
    }

    private float GetCurrentSplineProgress()
    {
        var followPath = GetComponent<FollowPathAgent>();
        if (followPath == null) return 0f;

        // Este es reflexion, la verdad si no le sé
        var progressField = typeof(FollowPathAgent).GetField("_t",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public);

        if (progressField != null)
        {
            object value = progressField.GetValue(followPath);
            if (value is float progress)
            {
                return progress;
            }
        }

        return 0f;
    }

    public float GetSavedProgress()
    {
        return splineProgressAtDeath;
    }

    private void SpawnFragment(int index, Vector3 centerPosition)
    {
        float angle = (360f / fragmentCount) * index;
        Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * spawnRadius;
        Vector3 spawnPos = centerPosition + offset;
        spawnPos.y = centerPosition.y;

        //Crea el fragmento
        GameObject fragmentObj = Instantiate(fragmentPrefab, spawnPos, Quaternion.identity);
        TrojanFragment fragment = fragmentObj.GetComponent<TrojanFragment>();

        if (fragment != null)
        {
            fragment.Initialize(ownerSpawner, spawnPos, splineProgressAtDeath);
        }
        else
        {
            Destroy(fragmentObj);
        }
    }

    private void ReturnToPoolOrDisable()
    {
        hasSplit = false;

        if (normalModel != null)
            normalModel.SetActive(true);

        var collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = true;

        var followPath = GetComponent<FollowPathAgent>();
        if (followPath != null)
        {
            followPath.enabled = true;
            followPath.ResetProgress(false);
        }

        if (ownerSpawner != null)
            ownerSpawner.ReturnToPool(gameObject);
        else
            gameObject.SetActive(false);
    }

    private new void OnDisable()
    {
        hasSplit = false;

        if (normalModel != null)
            normalModel.SetActive(true);
    }
}