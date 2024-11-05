using System;

[Serializable]
public class Sword
{
    public SwordStatistics Statistics;

    public void SetSwordStatistics(SwordScriptableObject p_swordScriptableObject)
    {
        // Basic statistics
        Statistics.Name = p_swordScriptableObject.SwordStatistics.Name;

        // Visual statistics
        Statistics.InventorySprite = p_swordScriptableObject.SwordStatistics.InventorySprite;
        Statistics.EquippedSprite = p_swordScriptableObject.SwordStatistics.EquippedSprite;

        // Combat statistics
        Statistics.AttackDamage = p_swordScriptableObject.SwordStatistics.AttackDamage;
        Statistics.AttackSpeed = p_swordScriptableObject.SwordStatistics.AttackSpeed;
        Statistics.ArmorPenetrationFactor = p_swordScriptableObject.SwordStatistics.ArmorPenetrationFactor;
    }

    public void SetSwordStatistics(SwordStatistics p_swordStatistics)
    {
        // Basic statistics
        Statistics.Name = p_swordStatistics.Name;

        // Visual statistics
        Statistics.InventorySprite = p_swordStatistics.InventorySprite;
        Statistics.EquippedSprite = p_swordStatistics.EquippedSprite;

        // Combat statistics
        Statistics.AttackDamage = p_swordStatistics.AttackDamage;
        Statistics.AttackSpeed = p_swordStatistics.AttackSpeed;
        Statistics.ArmorPenetrationFactor = p_swordStatistics.ArmorPenetrationFactor;
    }
}