using System;
using UnityEngine;

[Serializable]
public class FactionStatistics
{
    // Basic statistics
    [Header("Basic statistics :")]
    public string Name;

    // Basic statistics
    [Header("Visual statistics :")]
    public Sprite Logo;
    public Color Color; 

    // Basic statistics
    [Header("Faction statistics :")]
    public Pirate Leader;
    public int MemberNumber;
    public int CampNumber;
    public int BoatNumber;
    public int HarbourNumber;
    public Resources Resources;
}