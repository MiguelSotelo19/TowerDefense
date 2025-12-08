using System.Linq;
using UnityEngine;

public class Cannon : Tower
{
    [Header("Cannon Specific")]
    public float explosionRadius = 0f;

    protected override void Awake()
    {
        towerName = "Cañón de datos";
        maxLevel = 3;
        baseCost = 150;
        upgradeCosts = new int[] { 200, 350 };

        base.Awake();
    }

    protected override void ApplyLevelStats()
    {
        switch (currentLevel)
        {
            case 1: // Cañón de datos
                fireRate = 1.5f;
                maxTargets = 1;
                damage = 50;
                explosionRadius = 0f;
                break;

            case 2: // Cañón Binario
                fireRate = 1.2f;
                maxTargets = 2; //aTaca a 2
                damage = 40; 
                explosionRadius = 0f;
                break;

            case 3: // Cañón Comprimido
                fireRate = 1.0f;
                maxTargets = 1;
                damage = 60;
                explosionRadius = 2f;
                break;
        }
    }

    protected override void FireBullet(Transform target)
    {
        Bullet available = bullets.FirstOrDefault(x => !x.gameObject.activeInHierarchy);
        if (available == null) return;

        Vector3 direction = (target.position - firePoint.transform.position).normalized;

        available.direction = direction;
        available.transform.position = firePoint.transform.position;
        available.SetDamage(damage);
        available.SetExplosionRadius(explosionRadius);
        available.SetFollowTarget(false, null);
        available.gameObject.SetActive(true);


        if (shootSound != null)
            shootSound.Play();
    }


    public override string GetTowerInfo()
    {
        string levelName = currentLevel switch
        {
            1 => "Cañón de datos",
            2 => "Cañón Binario",
            3 => "Cañón Comprimido",
            _ => towerName
        };

        string info = $"{levelName}\n";
        info += $"Nivel {currentLevel}/{maxLevel}\n";
        info += $"Velocidad: {(1f / fireRate):F1} disparos/s\n";
        info += $"Daño: {damage}\n";
        info += $"Objetivos: {maxTargets}\n";
        info += $"Rango: {range:F1}\n";

        if (explosionRadius > 0)
        {
            info += $"Radio explosión: {explosionRadius:F1}m\n";
        }

        if (CanUpgrade())
        {
            info += $"\nMejora: {GetUpgradeCost()} bytes";
        }
        else
        {
            info += "\n¡NIVEL MÁXIMO!";
        }

        return info;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (explosionRadius > 0 && PrimaryTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(PrimaryTarget.position, explosionRadius);
        }
    }
}