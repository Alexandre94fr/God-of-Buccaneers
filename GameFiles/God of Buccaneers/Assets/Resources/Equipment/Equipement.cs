using System;
using UnityEngine;

[Serializable]
public class Equipement
{
    [field: SerializeField] public Armor Armor { get; private set; }
    [field: SerializeField] public Sword Sword { get; private set; }

    public void SetArmor(Armor p_armor)
    {
        // Update UI

        // Update visual in-game (pirate outfit)

        // Update value
        Armor = p_armor;
    }

    public void SetSword(Sword p_sword)
    {
        // Update UI

        // Update visual in-game (pirate outfit)

        // Update value
        Sword = p_sword;
    }
}