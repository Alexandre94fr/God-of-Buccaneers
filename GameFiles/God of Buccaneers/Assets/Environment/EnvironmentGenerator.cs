using System;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentGenerator : MonoBehaviour
{
    #region -= Struct =-
    [Serializable]
    public struct EnvironmentOptionsStruct
    {
        // NOTE : We can't set values insind of the struct because the Unity's C# is 9.0 not 10.0

        [Header("Environment options :")]
        [Range(0, 9999)] public int Seed; // 666
        public bool IsIsland; // true
        public bool HasWater; // true

        [Range(0, 1)] public float EnvironmentEdgeFactor;

        [Header("Island options :")]
        [Range(1, 10)] public int IslandNumber; // 5
        [Range(10, 50)] public int IslandSize; // 15
        [Range(0, 1)] public float MinimalDistanceBetweenIslandsFactor; // 0.1f
        [Range(0, 1)] public float IslandGenerationHeightThreshold; // 0.01f
        [Range(0, 0.001f)] public float MinimalThreshold; // 0.0f

        [Header("Mesh options :")]
        [Range(0, 32)] public float ChunkSize; // 30
        [Range(3, 255)] public int VerticeNumberPerLine; // 255
        [Range(0, 32)] public float HeightMultiplier; // 5
        [Range(1, 10)] public int NumberOfLayersPerUnitOfHeight; // 2

        [Header("Perlin noise options :")]
        [Range(0, 1)] public float PerlinScale; // 0.03f
        [Range(0, 10)] public int OctaveCount; // 5
        [Range(0.05f, 1)] public float Lacunarity; // 0.3f
        [Range(0, 1)] public float Persistance; // 0.4f

        public AnimationCurve HeightDistribution;

        [Header("Water options :")]
        [Range(0, 2)][SerializeField] public float WaterLevel; // 0.2f
    }
    #endregion

    #region -= Variables =-
    public static EnvironmentGenerator Instance;

    // References
    [Header("Prefab references :")]
    [SerializeField] GameObject _terrainChunkPrefab;
    [SerializeField] GameObject _oceanChunkPrefab;
    [SerializeField] GameObject _debugIslandCenterPrefab;

    [Header("Parent references :")]
    [SerializeField] GameObject _oceanParent;
    [SerializeField] GameObject _terrainParent;

    // Debuging
    [Header("Debuging :")]
    [SerializeField] bool _isDebugOn;

    [SerializeField] Vector2 _verticeToCheckIfInTerrainBorder;

    [Tooltip("BEWARE CAN BE LAGGY ! If this option is ON it will regenerate all the chunks every time the TerrainGenerator script's values are modified.")]
    [SerializeField] bool _liveUpdate;

    // Environment parameters
    [field:Header("Map size :")]
    [field:SerializeField]
    public Vector2 NumberOfChunks { get; private set; }

    // Struct
    [field:Header("Environment options :")]
    [field:SerializeField]
    public EnvironmentOptionsStruct EnvironmentOptions { get; private set; }

    // Terrain and Ocean chunk management
    readonly List<TerrainChunk> _terrainChunkComponents = new();
    readonly List<GameObject> _oceanChunkGameObjects = new();

    Vector2 _environmentWorldSize;
    float[,] _terrainNoiseMap;

    Vector2 _allTerrainVertices;
    float _spaceBetweenVertex;

    List<Vector2> _islandCenterWorldPositions = new();

    // NOT USED NOW
    Vector2 _environmentCenterWorldPosition; 

    // Debuging variables
    List<GameObject> _debugIslandCenterPrefabs = new();

    // Local references
    Transform _transform;
    #endregion

    #region -= Methods =-

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

        // Compute the environment size
        _environmentWorldSize = EnvironmentOptions.ChunkSize * NumberOfChunks;

        // Compute all the vertices in all the terrains
        _allTerrainVertices = EnvironmentOptions.VerticeNumberPerLine * NumberOfChunks;

        _spaceBetweenVertex = EnvironmentOptions.ChunkSize / (EnvironmentOptions.VerticeNumberPerLine - 1);

        GenerateAllEnvironment();
    }

    void OnValidate()
    {
        // Securities
        if (!_liveUpdate)
        {
            return;
        }

        if (_terrainChunkComponents.Count == 0 || _allTerrainVertices == Vector2.zero)
        {
            return;
        }

        GenerateAllEnvironment();
    }

    void GenerateAllEnvironment()
    {
        DestroyAllEnvironment();

        GenerateEnvironmentNoiseMap(_allTerrainVertices);

        float islandGenerationHeightThreshold = EnvironmentOptions.IslandGenerationHeightThreshold;
        float adjustementHeightStep = 0.01f;

        DetectIslandPositionInTerrain(
            ref _terrainNoiseMap,
            ref islandGenerationHeightThreshold,
            ref adjustementHeightStep,
            _allTerrainVertices,
            EnvironmentOptions.IslandNumber,
            EnvironmentOptions.MinimalDistanceBetweenIslandsFactor
        );

        GenerateTerrain();
    }

    void GenerateEnvironmentNoiseMap(Vector2 p_allTerrainVertices)
    {
        float[,] terrainNoiseMap = new float[(int)p_allTerrainVertices.x, (int)p_allTerrainVertices.y];
        
        Vector3 chunkWorldPosition = transform.position;

        for (int x = 0; x < p_allTerrainVertices.x; x++)
        {
            for (int z = 0; z < p_allTerrainVertices.y; z++)
            {
                Vector3 worldPosition = new(
                    x * _spaceBetweenVertex + chunkWorldPosition.x,
                    0,
                    z * _spaceBetweenVertex + chunkWorldPosition.z
                );

                terrainNoiseMap[x, z] = Mathf.PerlinNoise(
                    worldPosition.x * (EnvironmentOptions.PerlinScale) + EnvironmentOptions.Seed,
                    worldPosition.z * (EnvironmentOptions.PerlinScale) + EnvironmentOptions.Seed
                ) * EnvironmentOptions.Persistance;
            }
        }

        _terrainNoiseMap = terrainNoiseMap;
    }

    // TODO : Remplir ceci

    /// <summary>
    /// Recursive method </summary>
    /// <param name = "p_terrainNoiseMap">  </param>
    /// <param name = "p_islandGenerationHeightThreshold">  </param>
    /// <param name = "p_terrainSize"> The total number of vertices for the terrain in horizontal and vertical axis </param>
    /// <param name = "p_currentFoundedIslandNumber">  </param>
    /// <param name = "p_wantedFoundedIslandNumber">  </param>
    void DetectIslandPositionInTerrain(
        ref float[,] p_terrainNoiseMap,
        ref float p_islandGenerationHeightThreshold,
        ref float p_adjustmentHeightStep,
        Vector2 p_terrainSize,
        int p_wantedFoundedIslandNumber,
        float p_minimalDistanceBetweenIslands,
        int p_currentFoundedIslandNumber = 0,
        int p_currentRecursionCount = 0, int p_maxRecursionsNumber = 100)
    {
        // Stop recursion if p_maxRecursionsNumber is reached
        if (p_currentRecursionCount >= p_maxRecursionsNumber)
        {
            if (_isDebugOn) 
                Debug.LogWarning(
                    $"Reached maximum recursion limit ({p_currentRecursionCount} / {p_maxRecursionsNumber}).\n" +
                    $"A new island will be generated, we will use the 666 seed."
                );

            // Generate a new environment with the seed 666
            // NOTE : We are forced to create a new environment options because we can't modify the Seed directly
            EnvironmentOptionsStruct environmentOptions = new();
            environmentOptions = EnvironmentOptions;
            environmentOptions.Seed = 666;
            EnvironmentOptions = environmentOptions;

            GenerateAllEnvironment();

            return;
        }

        // Search in all the terrain noise map (p_terrainNoiseMap) if there are value that are above the threshold (p_islandGenerationHeightThreshold)
        // NOTE : Looking in z -> x order is easier for the computer to read
        for (int z = 0; z < p_terrainSize.y; z++)
        {
            for (int x = 0; x < p_terrainSize.x; x++)
            {
                if (IsPositionAtTerrainEdge(x, z, p_terrainSize, p_currentRecursionCount))
                    continue;

                if (p_terrainNoiseMap[x, z] > p_islandGenerationHeightThreshold)
                {
                    Vector2 potentialIslandPosition = new(x, z);

                    // Check if this point is far enought from other valided islands
                    bool isFarEnough = true;

                    foreach (Vector2 existingIsland in _islandCenterWorldPositions)
                    {
                        if (Vector2.Distance(potentialIslandPosition, existingIsland) < (p_minimalDistanceBetweenIslands * p_terrainSize.x) + EnvironmentOptions.IslandSize)
                        {
                            isFarEnough = false;
                            break;
                        }
                    }

                    // If the position is far enought then we add him
                    if (isFarEnough)
                    {
                        p_currentFoundedIslandNumber++;
                        _islandCenterWorldPositions.Add(potentialIslandPosition);
                    }
                }
            }
        }

        // Stop conditions
        if ((p_islandGenerationHeightThreshold == 0 || p_islandGenerationHeightThreshold == 1) && p_currentFoundedIslandNumber != p_wantedFoundedIslandNumber)
        {
            #region Debuging

            if (_isDebugOn)
                Debug.LogWarning(
                    $"At no height threshold, the wanted number of island was been founded"
                );
            #endregion

            return;
        }

        // If we have the wanted number of island then we return
        if (p_currentFoundedIslandNumber == p_wantedFoundedIslandNumber)
        {
            #region Debuging

            if (_isDebugOn)
            {
                Debug.Log(
                    $"{p_currentFoundedIslandNumber} / {p_wantedFoundedIslandNumber} islands have been founded " +
                    $"at the height threshold of {p_islandGenerationHeightThreshold}\n" +
                    $"Recursive iteration count : {p_currentRecursionCount}"
                );

                for (int i = 0; i < _islandCenterWorldPositions.Count; i++)
                {

                    print($"Position in vertices : {_islandCenterWorldPositions[i]} \nPosition in world : {_islandCenterWorldPositions[i] * _spaceBetweenVertex}");

                    GameObject debugIslandCenter = Instantiate(
                        _debugIslandCenterPrefab,
                        new Vector3(_islandCenterWorldPositions[i].x * _spaceBetweenVertex, 20, _islandCenterWorldPositions[i].y * _spaceBetweenVertex),
                        Quaternion.identity,
                        _transform
                    );

                    _debugIslandCenterPrefabs.Add(debugIslandCenter);

                    // TODO : Faire que ça puisse pas spawn sur les bords, mettre un mode debug, opti le code (3 boucle for, halp), 
                }
            }
            #endregion

            return;
        }

        if (p_currentFoundedIslandNumber < p_wantedFoundedIslandNumber)
        {
            p_islandGenerationHeightThreshold -= p_adjustmentHeightStep;
        }
        else if (p_currentFoundedIslandNumber > p_wantedFoundedIslandNumber)
        {
            p_islandGenerationHeightThreshold += p_adjustmentHeightStep;
        }

        // Reducing prograssivly to step value for more precision
        if (Math.Abs(p_currentFoundedIslandNumber - p_wantedFoundedIslandNumber) < 10 && p_adjustmentHeightStep > EnvironmentOptions.MinimalThreshold)
        {
            p_adjustmentHeightStep /= 2;
        }

        // Resting the value for the next loop
        _islandCenterWorldPositions.Clear();

        DetectIslandPositionInTerrain(
            ref p_terrainNoiseMap,
            ref p_islandGenerationHeightThreshold,
            ref p_adjustmentHeightStep,
            p_terrainSize,
            p_wantedFoundedIslandNumber,
            p_minimalDistanceBetweenIslands,
            0,
            p_currentRecursionCount + 1,
            p_maxRecursionsNumber
        );
    }

    /// <summary>
    /// Returns true if the given coordonates are at the edge of the terrain.  </summary>
    /// <param name = "p_x"> The X coordonate of the vertice insind the terrain </param>
    /// <param name = "p_z"> The Z coordonate of the vertice insind the terrain </param>
    /// <param name = "p_terrainSize"> The size of the terrain </param>
    /// <param name = "p_currentRecursionCount"> The number of time the DetectIslandPositionInTerrain has been called </param>
    bool IsPositionAtTerrainEdge(int p_x, int p_z, Vector2 p_terrainSize, int p_currentRecursionCount)
    {
        // To optimize
        float environmentEdgeFactor = EnvironmentOptions.EnvironmentEdgeFactor;

        #region Debuging

        // This section is to check if we correctly compute if the given coordonates are at the edge of the terrain
        // NOTE : Those print statement will be called every time the method DetectIslandPositionInTerrain is used.
        if (_isDebugOn) 
        {
            if (p_x == _verticeToCheckIfInTerrainBorder.x && p_z == _verticeToCheckIfInTerrainBorder.y)
            {
                print($"------------\n" +
                    $"Debuging, if the vertice {p_x}x {p_z}z is insind the terrain border. (Recursion count : {p_currentRecursionCount})"
                );

                print("(p_x < p_terrainSize.x * Mathf.Abs(environmentEdgeFactor - 1) && p_x > p_terrainSize.x * environmentEdgeFactor) && "
                    + "(p_z < p_terrainSize.y * Mathf.Abs(environmentEdgeFactor - 1) && p_z > p_terrainSize.y * environmentEdgeFactor)");
                
                print($"({p_x} < {p_terrainSize.x} * {Mathf.Abs(environmentEdgeFactor - 1)} && {p_x} > {p_terrainSize.x} * {environmentEdgeFactor}) && " +
                    $"({p_z} < {p_terrainSize.y} * {Mathf.Abs(environmentEdgeFactor - 1)} && {p_x} > {p_terrainSize.x} * {environmentEdgeFactor})"
                );
                
                print($"({p_x} < {p_terrainSize.x * Mathf.Abs(environmentEdgeFactor - 1)} && {p_x} > {p_terrainSize.x * environmentEdgeFactor}) && " +
                    $"({p_z} < {p_terrainSize.y * Mathf.Abs(environmentEdgeFactor - 1)} && {p_x} > {p_terrainSize.x * environmentEdgeFactor})"
                );
                
                print($"({p_x < p_terrainSize.x * Mathf.Abs(environmentEdgeFactor - 1)} && {p_x > p_terrainSize.x * environmentEdgeFactor}) && " +
                    $"({p_z < p_terrainSize.y * Mathf.Abs(environmentEdgeFactor - 1)} && {p_x > p_terrainSize.x * environmentEdgeFactor})"
                );
            }
        }
        #endregion

        // Check if he is not at the terrain borders
        if (p_x < p_terrainSize.x * Mathf.Abs(environmentEdgeFactor - 1) && p_x > p_terrainSize.x * environmentEdgeFactor &&
            p_z < p_terrainSize.y * Mathf.Abs(environmentEdgeFactor - 1) && p_z > p_terrainSize.y * environmentEdgeFactor)
        {
            return false;
        }

        return true;
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
                GameObject terrainChunkGameObject = Instantiate(_terrainChunkPrefab, chunkPosition, Quaternion.identity, _terrainParent.transform);

                TerrainChunk terrainChunkComponent = terrainChunkGameObject.GetComponent<TerrainChunk>();

                // Naming the terrain chunk GameObject
                terrainChunkGameObject.name = $"TerrainChunk ({x}x, {z}z)";

                _terrainChunkComponents.Add(terrainChunkComponent);
                #endregion

                #region Ocean chunk

                // Computing the ocean chunk position
                Vector3 oceanChunkPosition = new(
                    x * EnvironmentOptions.ChunkSize + (EnvironmentOptions.ChunkSize / 2) + _transform.position.x,
                    EnvironmentOptions.WaterLevel,
                    z * EnvironmentOptions.ChunkSize + (EnvironmentOptions.ChunkSize / 2) + _transform.position.z
                );

                // Creation of the ocean chunk GameObject
                GameObject oceanChunkGameObject = Instantiate(_oceanChunkPrefab, oceanChunkPosition, Quaternion.identity, _oceanParent.transform);
                
                // Changing the scale of the ocean chunk accordingly (to the same size as the terrain chunk)
                oceanChunkGameObject.transform.localScale = new(EnvironmentOptions.ChunkSize / 10, 1, EnvironmentOptions.ChunkSize / 10);

                // Naming the ocean chunk GameObject
                oceanChunkGameObject.name = $"OceanChunk ({x}x, {z}z)";

                _oceanChunkGameObjects.Add(oceanChunkGameObject);
                #endregion
            }
        }

        // Computing the environment center world position
        _environmentCenterWorldPosition = _environmentWorldSize / 2;

        foreach (TerrainChunk terrainChunkComponent in _terrainChunkComponents)
        {
            // Generating the terrain chunk
            terrainChunkComponent.GenerateTerrainChunk(EnvironmentOptions, _islandCenterWorldPositions);
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

        foreach (GameObject debugIslandCenter in _debugIslandCenterPrefabs)
        {
            Destroy(debugIslandCenter);
        }

        // Reset the lists
        _terrainChunkComponents.Clear();
        _oceanChunkGameObjects.Clear();
        _debugIslandCenterPrefabs.Clear();
    }

    private void OnApplicationQuit()
    {
        DestroyAllEnvironment();
    }
#endregion
}