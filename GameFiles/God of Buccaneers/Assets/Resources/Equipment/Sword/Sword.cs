using System;

[Serializable]
public class Sword
{
    public SwordStatistics Statistics = new();

    #region Constructors

    public Sword(SwordScriptableObject p_swordScriptableObject)
    {
        SetSwordStatistics(p_swordScriptableObject);
    }

    public Sword(SwordStatistics p_swordStatistics)
    {
        SetSwordStatistics(p_swordStatistics);
    }
    #endregion

    #region Methods

    public override string ToString()
    {
        return $"Statistics : {Statistics}";
    }

    #region Set sword statistics

    public void SetSwordStatistics(SwordScriptableObject p_swordScriptableObject)
    {
        if (p_swordScriptableObject == null)
        {
            ResetSwordValues();
            return;
        }

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
        Statistics = p_swordStatistics;
    }
    #endregion

    public void ResetSwordValues()
    {
        // Basic statistics
        Statistics.Name = "";

        // Visual statistics
        Statistics.InventorySprite = null;
        Statistics.EquippedSprite = null;

        // Combat statistics
        Statistics.AttackDamage = 0;
        Statistics.AttackSpeed = 0;
        Statistics.ArmorPenetrationFactor = 1;
    }
    #endregion
}