using UnityEngine;

public class Antivirus : Tower
{
    protected override void Awake()
    {
        towerName = "Antivirus";
        maxLevel = 3;
        baseCost = 100;
        upgradeCosts = new int[] { 150, 250 };

        base.Awake();
    }

    protected override void ApplyLevelStats()
    {
        switch (currentLevel)
        {
            case 1: // Antivirus Estándar
                fireRate = 0.5f;
                maxTargets = 1;
                damage = 25;
                break;

            case 2: // Antivirus Pro
                fireRate = 0.25f;
                maxTargets = 1;
                damage = 25;
                break;

            case 3: // Antivirus 360°
                fireRate = 0.35f;
                maxTargets = 2; //Ataca a 2 el locochon
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