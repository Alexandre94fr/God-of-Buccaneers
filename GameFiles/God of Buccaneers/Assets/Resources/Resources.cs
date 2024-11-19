using System;

[Serializable]
public class Resources
{
    public Equipement Equipement;
    public int GoldCoinNumber;

    public override string ToString()
    {
        return $"Resources : \n" +
            $"- Equipment : {(Equipement != null ? Equipement.ToString() : "No equipment")}\n" +
            $"- Gold coin : {GoldCoinNumber}";
    }

    /// <summary>
    /// Cast ChestLoot into Resources </summary>
    public static explicit operator Resources(ChestLoot chestLoot)
    {
        Resources resources = new()
        {
            GoldCoinNumber = chestLoot.GoldCoin,
            Equipement = new()
        };

        resources.Equipement.SetArmor(chestLoot.ArmorScriptableObject);
        resources.Equipement.SetSword(chestLoot.SwordScriptableObject);

        return resources;
    }

    public void TransfertResources(Resources p_resourcesReciever)
    {
        p_resourcesReciever.GoldCoinNumber += GoldCoinNumber;
        GoldCoinNumber = 0;

        // If the resource reciever does not already have an armor
        if (p_resourcesReciever.Equipement.Armor.Statistics.Name == "")
        {
            p_resourcesReciever.Equipement.Armor.SetArmorStatistics(Equipement.Armor.Statistics);
            Equipement.Armor.ResetArmorValues();
        }

        // If the resource reciever does not already have a sword
        if (p_resourcesReciever.Equipement.Sword.Statistics.Name == "")
        {
            p_resourcesReciever.Equipement.Sword.SetSwordStatistics(Equipement.Sword.Statistics);
            Equipement.Sword.ResetSwordValues();
        }       
    }
}