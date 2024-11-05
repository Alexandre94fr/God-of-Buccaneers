using System;

[Serializable]
public class Armor
{
    public ArmorStatistics Statistics;

    public void SetArmorStatistics(ArmorScriptableObject p_armorScriptableObject)
    {
        // Basic statistics
        Statistics.Name = p_armorScriptableObject.ArmorStatistics.Name;

        // Visual statistics
        Statistics.InventorySprite = p_armorScriptableObject.ArmorStatistics.InventorySprite;
        Statistics.EquippedSprite = p_armorScriptableObject.ArmorStatistics.EquippedSprite;

        // Combat statistics
        Statistics.ProtectionFactor = p_armorScriptableObject.ArmorStatistics.ProtectionFactor;
    }

    public void SetArmorStatistics(ArmorStatistics p_armorStatistics)
    {
        // Basic statistics
        Statistics.Name = p_armorStatistics.Name;

        // Visual statistics
        Statistics.InventorySprite = p_armorStatistics.InventorySprite;
        Statistics.EquippedSprite = p_armorStatistics.EquippedSprite;

        // Combat statistics
        Statistics.ProtectionFactor = p_armorStatistics.ProtectionFactor;
    }
}