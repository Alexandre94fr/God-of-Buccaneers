using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentStatistics", menuName = "ScriptableObject/EnvironmentStatistics")]
public class EnvironmentStatistics : ScriptableObject
{
    #region -= Struct =-

    [Serializable]
    public struct EnvironmentOptionsStruct
    {
        // NOTE : We can't set values insind of the struct because the Unity's C# is 9.0 not 10.0

        [Header("Environment options :")]
        [Range(0, 9999)] public int Seed; // 666
        public Vector2Int NumberOfChunks; // 10x10
        public bool IsIsland; // true
        public bool HasWater; // true

        [Range(0.05f, 1)] public float EnvironmentEdgeFactor; // 0.1

        [Header("Island options :")]
        [Range(1, 10)] public int IslandNumber; // 4
        [Range(0.05f, 1)] public float IslandSizeFactor; // 0.15f
        [Range(0.05f, 1)] public float MinimalDistanceBetweenIslandsFactor; // 0.15f

        [Header("Mesh options :")]
        [Range(1, 32)] public int ChunkSize; // 30
        [Range(3, 255)] public int TerrainChunkVerticeNumberPerLine; // 128
        [Range(16, 255)] public int OceanChunkVerticeNumberPerLine; // 128

        [Header("Perlin noise options :")]
        [Range(0, 1)] public float Frequency; // 0.05f
        [Range(1, 10)] public int OctaveCount; // 5
        [Range(0.05f, 1)] public float Lacunarity; // 1f
        [Range(0, 1)] public float Persistance; // 0.1f
        [Range(1, 32)] public float HeightMultiplier; // 5
        public AnimationCurve HeightDistribution; // 1

        [Header("Water options :")]

        [Range(0, 2)] public float WaterLevel; // 1f
    }
    #endregion

    public EnvironmentOptionsStruct EnvironmentOptions;
}