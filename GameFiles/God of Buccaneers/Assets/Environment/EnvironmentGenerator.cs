using System;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentGenerator : MonoBehaviour
{
    #region Struct
    [Serializable]
    public struct EnvironmentOptionsStruct
    {
        // NOTE : We can't set values insind of the struct because the Unity's C# is 9.0 not 10.0

        [Header("Environment options :")]
        [Range(0, 9999)] public int Seed;
        public bool IsIsland;
        public bool HasWater;

        [Header("Mesh options :")]
        [Range(0, 32)] public float ChunkSize; // 30
        [Range(0, 255)] public int VerticeSize; // 128
        [Range(0, 32)] public float HeightMultiplier; // 5
        [Range(1, 10)] public int NumberOfLayersPerUnitOfHeight; // 2

        [Header("Perlin noise options :")]
        [Range(0, 1)] public float PerlinScale; // 0.03f
        [Range(0, 10)] public int OctaveCount; // 8
        [Range(0.05f, 1)] public float Lacunarity; // 0.3f
        [Range(0, 1)] public float Persistance; // 0.4f
       
        public AnimationCurve HeightDistribution; 

        [Header("Water options :")]
        [Range(0, 2)] [SerializeField] public float WaterLevel; // 0.2f
    }
    #endregion

    #region Variables
    public static EnvironmentGenerator Instance;

    // References
    [Header("References :")]
    [SerializeField] GameObject _terrainChunkPrefab;
    [SerializeField] GameObject _oceanChunkPrefab;

    [Header("Debuging :")]
    [Tooltip("BEWARE CAN BE LAGGY ! If this option is ON it will regenerate all the chunks every time the TerrainGenerator script values are modified.")]
    [SerializeField] bool _liveUpdate;

    [field:Header("Map size :")]
    [field: SerializeField]
    public Vector2 NumberOfChunks { get; private set; }

    // Struct
    [field:Header("Environment options :")]
    [field:SerializeField]
    public EnvironmentOptionsStruct EnvironmentOptions { get; private set; }

    // Terrain and Ocean chunk management
    readonly List<TerrainChunk> _terrainChunkComponents = new();
    readonly List<GameObject> _oceanChunkGameObjects = new();

    // Local references
    Transform _transform;
    #endregion

    #region Methods

    void Awake()
    {
        Instance = Instantiator.ReturnInstance(this, Instantiator.InstanceConflictResolutions.WarningAndPause);
    }

    void Start()
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

        DestroyAllEnvironment();
        GenerateTerrain();
    }

    void GenerateTerrain()
    {
        for (int x = 0; x < NumberOfChunks.x; x++)
        {
            for (int z = 0; z < NumberOfChunks.y; z++)
            {
                #region Terrain chunk

                // Computing the terrain chunk position
                Vector3 chunkPosition = new(
                    x * EnvironmentOptions.ChunkSize + _transform.position.x,
                    0,
                    z * EnvironmentOptions.ChunkSize + _transform.position.z
                );

                // Creation of the terrain chunk GameObject
                GameObject terrainChunkGameObject = Instantiate(_terrainChunkPrefab, chunkPosition, Quaternion.identity, _transform);

                TerrainChunk terrainChunkComponent = terrainChunkGameObject.GetComponent<TerrainChunk>();

                // Naming the terrain chunk GameObject
                terrainChunkGameObject.name = $"TerrainChunk ({x}x, {z}z)";

                _terrainChunkComponents.Add(terrainChunkComponent);

                // Generating the terrain chunk
                terrainChunkComponent.GenerateTerrainChunk(EnvironmentOptions);
                #endregion

                #region Ocean chunk

                // Computing the ocean chunk position
                Vector3 oceanChunkPosition = new(
                    x * EnvironmentOptions.ChunkSize + (EnvironmentOptions.ChunkSize / 2) + _transform.position.x,
                    EnvironmentOptions.WaterLevel,
                    z * EnvironmentOptions.ChunkSize + (EnvironmentOptions.ChunkSize / 2) + _transform.position.z
                );

                // Creation of the ocean chunk GameObject
                GameObject oceanChunkGameObject = Instantiate(_oceanChunkPrefab, oceanChunkPosition, Quaternion.identity, _transform);
                
                // Changing the scale of the ocean chunk accordingly (to the same size as the terrain chunk)
                oceanChunkGameObject.transform.localScale = new(EnvironmentOptions.ChunkSize / 10, 1, EnvironmentOptions.ChunkSize / 10);

                // Naming the ocean chunk GameObject
                oceanChunkGameObject.name = $"OceanChunk ({x}x, {z}z)";

                _oceanChunkGameObjects.Add(oceanChunkGameObject);
                #endregion
            }
        }
    }

    void DestroyAllEnvironment()
    {
        // Security
        if (_terrainChunkComponents.Count == 0)
            return;

        foreach (TerrainChunk terrainChunkComponent in _terrainChunkComponents)
        {
            Destroy(terrainChunkComponent.gameObject);
        }

        foreach (GameObject oceanChunkGameObject in _oceanChunkGameObjects)
        {
            Destroy(oceanChunkGameObject);
        }

        // Reset the lists
        _terrainChunkComponents.Clear();
        _oceanChunkGameObjects.Clear();
    }

    private void OnApplicationQuit()
    {
        DestroyAllEnvironment();
    }
    #endregion
}