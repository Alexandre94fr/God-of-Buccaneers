using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    #region Struct
    [Serializable]
    public struct TerrainOptions
    {
        // NOTE : We can't set values insind of the struct because the Unity's C# is 9.0 not 10.0

        [Header("Environment options :")]
        [Range(0, 9999)]
        public int Seed;
        public bool IsIsland;
        public bool HasWater;

        [Header("Mesh options :")]
        [Range(0, 32)]
        public float MeshSize; // 30
        [Range(0, 255)]
        public int VerticeSize; // 128
        [Range(0, 32)]
        public float HeightMultiplier; // 5

        [Header("Perlin noise options :")]
        [Range(0, 1)]
        public float PerlinScale; // 0.03f
        [Range(0, 10)]
        public int OctaveCount; // 8
        [Range(0.05f, 1)]
        public float Lacunarity; // 0.3f
        [Range(0, 1)]
        public float Persistance; // 0.4f

        [Header("Water options :")]
        [Range(0, 2)]
        public float WaterLevel; // 0.2f
    }
    #endregion

    #region Variables
    // References
    [Header("References :")]
    [SerializeField] GameObject _terrainChunkPrefab;

    [Header("Debuging :")]
    [Tooltip("BEWARE CAN BE LAGGY ! If this option is ON it will regenerate all the chunks every time the TerrainGenerator script values are modified.")]
    [SerializeField] bool _liveUpdate;

    [Header("Map size :")]
    [SerializeField] Vector2 _numberOfChunks;

    // Struct
    [Header("Terrain options :")]
    [SerializeField] TerrainOptions _terrainOptions;

    // Chunk management
    readonly List<TerrainChunk> _terrainChunkComponents = new();

    // Local references
    Transform _transform;
    #endregion

    #region Methods

    private void Start()
    {
        // Security
        if (_terrainChunkPrefab == null)
            Debug.LogError("ERROR ! The variable '_terrainChunkPrefab' is null, do you forgot to give the reference ?");

        // Setting local references
        _transform = transform;

        GenerateTerrain();
    }

    void OnValidate()
    {
        // Securities
        if (!_liveUpdate)
        {
            return;
        }

        if (_terrainChunkComponents.Count == 0)
        {
            return;
        }

        foreach (TerrainChunk terrainChunk in _terrainChunkComponents)
        {
            terrainChunk.GenerateTerrainChunk(_terrainOptions);
        }
    }

    void GenerateTerrain()
    {
        for (int x = 0; x < _numberOfChunks.x; x++)
        {
            for (int z = 0; z < _numberOfChunks.y; z++)
            {
                // Computing the chunk position
                Vector3 chunkPosition = new(
                    x * _terrainOptions.MeshSize + _transform.position.x,
                    0,
                    z * _terrainOptions.MeshSize + _transform.position.z
                );

                // Creation of the chunk GameObject
                GameObject terrainChunkGameObject = Instantiate(_terrainChunkPrefab, chunkPosition, Quaternion.identity, _transform);

                TerrainChunk terrainChunkComponent = terrainChunkGameObject.GetComponent<TerrainChunk>();

                // Naming the chunk GameObject
                terrainChunkGameObject.name = $"TerrainChunk ({x}x, {z}z)";

                _terrainChunkComponents.Add(terrainChunkComponent);

                // Generating the chunk
                terrainChunkComponent.GenerateTerrainChunk(_terrainOptions);
            }
        }
    }

    private void OnApplicationQuit()
    {
        // Security
        if (_terrainChunkComponents.Count == 0)
            return;

        // Destroy all the chunks
        for (int i = 0; i < _terrainChunkComponents.Count; i++)
        {
            DestroyImmediate(_terrainChunkComponents[i].gameObject);
        }

        // Reset the list
        _terrainChunkComponents.Clear();
    }
    #endregion
}