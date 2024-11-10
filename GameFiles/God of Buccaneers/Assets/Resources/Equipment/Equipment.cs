using System;
using UnityEngine;

[Serializable]
public class Equipement
{
    [field: SerializeField] public Armor Armor { get; private set; }
    [field: SerializeField] public Sword Sword { get; private set; }

    #region Methods

    public override string ToString()
    {
        return $"Equipment : \n" +
            $"-- Armor : {(Armor != null ? Armor.ToString() : "No armor")}\n" +
            $"-- Sword : {(Sword != null ? Sword.ToString() : "No sword")}";
    }

    #region Set armor

    public void SetArmor(ArmorStatistics p_armorStatistics)
    {
        // Create armor, with stats
        // NOTE : The 'armor' variable may be used in the futur 
        Armor armor = new(p_armorStatistics);
        Armor = armor;

        // Update UI
        UpdateUI();

        // Update visual in-game (pirate outfit)
        UpdateInGameVisual();
    }

    public void SetArmor(ArmorScriptableObject p_armorScriptableObject)
    {
        // Create armor, with stats
        // NOTE : The 'armor' variable may be used in the futur 
        Armor armor = new(p_armorScriptableObject);
        Armor = armor;

        // Update UI
        UpdateUI();

        // Update visual in-game (pirate outfit)
        UpdateInGameVisual();
    }
    #endregion

    #region Set sword

    public void SetSword(SwordStatistics p_swordStatistics)
    {
        // Create sword, with stats
        // NOTE : The 'sword' variable may be used in the futur 
        Sword sword = new(p_swordStatistics);
        Sword = sword;

        // Update UI
        UpdateUI();

        // Update visual in-game (pirate outfit)
        UpdateInGameVisual();
    }

    public void SetSword(SwordScriptableObject p_swordScriptableObject)
    {
        // Create sword, with stats
        // NOTE : The 'sword' variable may be used in the futur 
        Sword sword = new(p_swordScriptableObject);
        Sword = sword;

        // Update UI
        UpdateUI();

        // Update visual in-game (pirate outfit)
        UpdateInGameVisual();
    }
    #endregion

    void UpdateUI()
    {

    }

    void UpdateInGameVisual()
    {

    }
    #endregion
}