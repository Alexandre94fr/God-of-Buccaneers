using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Sword", menuName = "ScriptableObject/Equipment/Sword")]
public class SwordScriptableObject : ScriptableObject
{
    [Header("Statistics :")]
    public SwordStatistics SwordStatistics;
}