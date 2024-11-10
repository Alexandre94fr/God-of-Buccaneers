using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChestUIManager : MonoBehaviour
{
    [Serializable]
    struct ChestUILocalReferences
    {
        [Header("Main UI :")]
        public GameObject MainChestInventoryUI;

        [Header("Gold coin :")]
        public Image GoldCoinImage;
        public TextMeshProUGUI GoldCoinNumberText;

        [Header("Armor :")]
        public Image ArmorImage;
        public TextMeshProUGUI ArmorNameText;

        [Header("Sword :")]
        public Image SwordImage;
        public TextMeshProUGUI SwordNameText;
    }

    [Header("References :")]
    [SerializeField] Sprite _goldCoinSprite;
    [SerializeField] Sprite _wrongSprite;

    [SerializeField] ChestUILocalReferences _localReferences;

    Resources _chestInventoryUIResources;

    // Start is called before the first frame update
    void Start()
    {
        Chest.OnPlayerOpeningChest += ShowCheckInventoryUI;

        _localReferences.GoldCoinImage.sprite = _goldCoinSprite;
    }

    void ShowCheckInventoryUI(Chest p_chest)
    {
        UpdateShownChestData(p_chest);

        if (!_localReferences.MainChestInventoryUI.activeSelf)
            _localReferences.MainChestInventoryUI.SetActive(true);
    }

    void UpdateShownChestData(Chest p_chest)
    {
        Resources chestResources = p_chest.Loot;

        ArmorStatistics armorStatistics = chestResources.Equipement.Armor.Statistics;
        SwordStatistics swordStatistics = chestResources.Equipement.Sword.Statistics;

        if (chestResources == null)
            Debug.LogError($"ERROR ! The chest's Resources of the '{p_chest.gameObject.name}' GameObject is null.");
        
        bool hasArmorSprite = !armorStatistics.InventorySprite.IsUnityNull();
        bool hasSwordSprite = !swordStatistics.InventorySprite.IsUnityNull();

        // Gold coin
        _localReferences.GoldCoinNumberText.text = ": " + chestResources.GoldCoinNumber;

        // Armor
        _localReferences.ArmorImage.sprite = hasArmorSprite ? armorStatistics.InventorySprite : _wrongSprite;
        _localReferences.ArmorNameText.text = hasArmorSprite ? ": " + armorStatistics.Name : ": No armor";

        // Sword
        _localReferences.SwordImage.sprite = hasSwordSprite ? swordStatistics.InventorySprite : _wrongSprite;
        _localReferences.SwordNameText.text = hasSwordSprite ? ": " + swordStatistics.Name : ": No sword";
    }
}