using System;
using UnityEngine;

// For debugging purpose this class is visible in the inspector
[Serializable]
public struct ArmorStatistics
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
    [Tooltip("The direct number of attack damage nullify when multiplied by the ProtectionFactor value." +
        "\n\nClamped between 0 and 666.")]
    [Range(0, 666)] public float Defense;

    [Tooltip("The rate of damage reduction againts an attack. A value of 1.5f means +50% of damage reduction, " +
        "that means the Defense value will be multiply by 1.5f when calculating the real damage recieved." +
        "\n\nClamped between 1 and 10.")]
    [Range(1, 10)] public float ProtectionFactor;

    public override string ToString()
    {
        return $"ArmorStatistics :\n" +
            $"--- Name : {(Name != "" ? Name : "No name")}\n" +
            $"--- InventorySprite : {(InventorySprite != null ? Name : "null")}\n" +
            $"--- EquippedSprite : {(EquippedSprite != null ? Name : "null")}\n" +
            $"--- Defense : {Defense}\n" +
            $"--- ProtectionFactor : {ProtectionFactor}";
    }
}