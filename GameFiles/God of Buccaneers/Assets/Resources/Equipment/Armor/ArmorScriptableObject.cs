using UnityEngine;

[CreateAssetMenu(fileName = "Armor", menuName = "ScriptableObject/Equipment/Armor")]
public class ArmorScriptableObject : ScriptableObject
{
    [Header("Statistics :")]
    public ArmorStatistics ArmorStatistics;
}