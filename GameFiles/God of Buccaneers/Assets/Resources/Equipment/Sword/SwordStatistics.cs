using System;
using UnityEngine;

// For debugging purpose this class is visible in the inspector
[Serializable]
public struct SwordStatistics
{
    // Basic statistics
    [Header("Basic statistics :")]
    public string Name;

    // Visual statistics
    [Header("Visual statistics :")]
    public Sprite InventorySprite;
    public Sprite EquippedSprite;

    // Combat statistics
    [Header("Combat statistics :")]
    [Tooltip("Brut damage deal to a pirate if he has no armor, and if this sword has an armor penetration of 1. " +
        "\n\nClamped between 0 and 666.")]
    [Range(0, 666)] public int AttackDamage;

    [Tooltip("The time in second between each attack. " +
        "\n\nClamped between 0 and 2.")]
    [Range(0, 2)] public float AttackSpeed;

    [Tooltip("The rate of penetration of the attack. A value of 1.5f mean +50% of penetration, " +
        "that mean the AttackDamage value will be multiply by 1.5f when calculating the real damage dealt " +
        "\n\nClamped between 0 and 2.")]
    [Range(0, 2)] public float ArmorPenetrationFactor; // ArmorPenetrationRate ?

    public override string ToString()
    {
        return $"SwordStatistics :\n" +
            $"--- Name : {(Name != "" ? Name : "No name")}\n" +
            $"--- InventorySprite : {(InventorySprite != null ? Name : "null")}\n" +
            $"--- EquippedSprite : {(EquippedSprite != null ? Name : "null")}\n" +
            $"--- AttackDamage : {AttackDamage}\n" +
            $"--- AttackSpeed : {AttackSpeed}\n" +
            $"--- ArmorPenetrationFactor : {ArmorPenetrationFactor}";
    }
}