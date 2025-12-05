using UnityEngine;

/// <summary>
/// Torre Antivirus - Simple y fácil de entender.
/// Define sus propios stats y cómo cambian en cada nivel.
/// </summary>
public class Antivirus : Tower
{
    protected override void Awake()
    {
        // Configurar solo valores que NO quieres cambiar en el Inspector
        towerName = "Antivirus";
        maxLevel = 3;

        // Costos
        baseCost = 100;
        upgradeCosts = new int[] { 150, 250 };

        // Los stats (range, fireRate, etc.) ahora se configuran en el Inspector
        // o los deja con los valores por defecto de Tower.cs

        // Llamar al Awake de Tower
        base.Awake();
    }

    protected override void ApplyLevelStats()
    {
        // Aquí defines qué cambia en cada nivel
        switch (currentLevel)
        {
            case 1: // Antivirus Estándar
                fireRate = 0.5f;
                maxTargets = 1;
                damage = 25;
                break;

            case 2: // Antivirus Pro (más rápido)
                fireRate = 0.25f; // 2x más rápido
                maxTargets = 1;
                damage = 25;
                break;

            case 3: // Antivirus 360° (ataca a 2 enemigos)
                fireRate = 0.35f;
                maxTargets = 2; // Puede atacar a 2 virus a la vez
                damage = 25;
                break;
        }
    }

    public override string GetTowerInfo()
    {
        string levelName = currentLevel switch
        {
            1 => "Antivirus Estándar",
            2 => "Antivirus Pro",
            3 => "Antivirus 360°",
            _ => towerName
        };

        string info = $"{levelName}\n";
        info += $"Nivel {currentLevel}/{maxLevel}\n";
        info += $"Velocidad: {(1f / fireRate):F1} disparos/s\n";
        info += $"Daño: {damage}\n";
        info += $"Objetivos: {maxTargets}\n";
        info += $"Rango: {range:F1}\n";

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
}