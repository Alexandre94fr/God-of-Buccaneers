using System;
using UnityEngine;

public class Chest : InteractableBase
{
    public static Action<Chest> OnPlayerOpeningChest;

    public Resources Loot;

    [SerializeField] Pirate _looter;

    // Start is called before the first frame update
    protected override void Start()
    {
        // Check if the Sprite of our GameObject has the good layer,
        LayerSecurity(GetComponentInChildren<BoxCollider>().gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Interact()
    {
        Debug.Log("Je suis le coffre");

        OnPlayerOpeningChest?.Invoke(this);

        //LootChest(_looter);
    }

    public Resources GetChestLoot()
    {
        return Loot;
    }

    public void SetChestLoot(Resources p_loot)
    {
        Loot = p_loot;
    }

    public void SetChestLoot(ChestLoot p_loot)
    {
        Loot = (Resources)p_loot;
    }

    public void LootChest(Pirate p_pirate)
    {
        print($"CHEST INVENTORY\n\n{Loot}");
        print($"PIRATE INVENTORY\n\n{p_pirate.Inventory}");

        Loot.TransfertResources(p_pirate.Inventory);

        print($"CHEST INVENTORY\n\n{Loot}");
        print($"PIRATE INVENTORY\n\n{p_pirate.Inventory}");
    }

    void ResetLoot()
    {
        Loot = null;
    }
}