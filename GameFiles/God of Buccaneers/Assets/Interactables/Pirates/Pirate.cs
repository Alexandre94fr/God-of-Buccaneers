using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pirate : InteractableBase
{
    [Header("Inventory :")]
    public Resources Inventory;

    [Header("JUST FOR YOU TO KNOW THAT THIS EXIST :")]
    public FactionStatistics FactionStatistics;

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
        Debug.Log("Je suis spécial");

        // Testing to set new resources into the pirate 

        Inventory.GoldCoinNumber++;
        
        SwordStatistics swordStatistics = new()
        {
            Name = "Je suis un test",
            AttackDamage = 15,
            AttackSpeed = 0.5f,
            ArmorPenetrationFactor = 3,
        };

        Inventory.Equipement.Sword.SetSwordStatistics(swordStatistics);
    }
}