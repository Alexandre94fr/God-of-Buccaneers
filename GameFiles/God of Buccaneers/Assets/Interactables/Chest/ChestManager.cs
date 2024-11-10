using System;
using System.Collections.Generic;
using UnityEngine;

public class ChestManager : MonoBehaviour
{
    #region Struct

    [Serializable]
    struct ChestLootDropRate
    {
        public ChestLoot ChestLoot;
        [Range(0f, 100f)] public float DropRate;
    }
    #endregion

    [Header("References :")]
    [SerializeField] GameObject _chestPrefab;

    [Header("Default chest loots :")]
    // Quality of life variable
    [SerializeField] float _totalDropRate;

    [SerializeField] List<ChestLootDropRate> _defaultChestLoots = new();

    List<GameObject> _chests = new();

    // Start is called before the first frame update
    void Start()
    {
        if (!DoesDropRateSumEquals100(out float totalDropRate))
        {
            Debug.LogError($"ERROR ! The sum of all the default chest loot drop rate is not equal to 100, but equal to {totalDropRate}.");
        }

        for (int i = 0; i < 10; i++)
        {
            CreateChest(new Vector3(40 + (i * 5), 7, 20), gameObject.transform);
        }

        
    }

    void OnValidate()
    {
        DoesDropRateSumEquals100(out float totalDropRate);
        _totalDropRate = totalDropRate;
    }

    bool DoesDropRateSumEquals100(out float p_totalDropRate)
    {
        p_totalDropRate = 0;

        foreach (ChestLootDropRate chestLootDropRate in _defaultChestLoots)
        {
            p_totalDropRate += chestLootDropRate.DropRate;
        }

        if (p_totalDropRate != 100)
            return false;

        return true;
    }

    #region CreateChest methods

    public void CreateChest(Vector3 p_position, Transform p_chestParent)
    {
        InstantiateChest(p_position, p_chestParent).GetComponent<Chest>().SetChestLoot(GetRandomDefaultLoot());
    }

    public void CreateChest(Vector3 p_position, Transform p_chestParent, ChestLoot p_customLoot)
    {
        InstantiateChest(p_position, p_chestParent).GetComponent<Chest>().SetChestLoot(p_customLoot);
    }

    public void CreateChest(Vector3 p_position, Transform p_chestParent, Resources p_customLoot)
    {
        InstantiateChest(p_position, p_chestParent).GetComponent<Chest>().SetChestLoot(p_customLoot);
    }

    GameObject InstantiateChest(Vector3 p_position, Transform p_chestParent)
    {
        GameObject chest = Instantiate(_chestPrefab, p_position, Quaternion.identity, p_chestParent);

        _chests.Add(chest);

        return chest;
    }

    Resources GetRandomDefaultLoot()
    {
        float randomPourcentage = UnityEngine.Random.Range(0f, 100f);
        float dropRateSum = 0f;
        
        foreach (ChestLootDropRate chestLootDropRate in _defaultChestLoots)
        {
            dropRateSum += chestLootDropRate.DropRate;

            if (dropRateSum >= randomPourcentage)
                return (Resources)chestLootDropRate.ChestLoot;
        }

        Debug.LogError(
            "ERROR ! No chest loot was selected. " +
            "Please check that the sum of all default drop rates are equal to 100, or that randomPourcentage is not above 100."
        );
        return null;
    }
    #endregion
}