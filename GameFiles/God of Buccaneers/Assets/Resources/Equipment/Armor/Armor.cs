using System;

[Serializable]
public class Armor
{
    public ArmorStatistics Statistics = new();

    #region Constructors

    public Armor(ArmorScriptableObject p_armorScriptableObject)
    {
        SetArmorStatistics(p_armorScriptableObject);
    }

    public Armor(ArmorStatistics p_armorStatistics)
    {
        SetArmorStatistics(p_armorStatistics);
    }
    #endregion

    #region Methods

    public override string ToString()
    {
        return $"Statistics : {Statistics}";
    }

    #region Set armor statistics

    public void SetArmorStatistics(ArmorScriptableObject p_armorScriptableObject)
    {
        if (p_armorScriptableObject == null)
        {
            ResetArmorValues();
            return;
        }
            
        // Basic statistics
        Statistics.Name = p_armorScriptableObject.ArmorStatistics.Name;

        // Visual statistics
        Statistics.InventorySprite = p_armorScriptableObject.ArmorStatistics.InventorySprite;
        Statistics.EquippedSprite = p_armorScriptableObject.ArmorStatistics.EquippedSprite;

        // Combat statistics
        Statistics.Defense = p_armorScriptableObject.ArmorStatistics.Defense;
        Statistics.ProtectionFactor = p_armorScriptableObject.ArmorStatistics.ProtectionFactor;
    }

    public void SetArmorStatistics(ArmorStatistics p_armorStatistics)
    {
        Statistics = p_armorStatistics;
    }
    #endregion

    public void ResetArmorValues()
    {
        // Basic statistics
        Statistics.Name = "";

        // Visual statistics
        Statistics.InventorySprite = null;
        Statistics.EquippedSprite = null;

        // Combat statistics
        Statistics.Defense = 0;
        Statistics.ProtectionFactor = 0;
    }
    #endregion
}