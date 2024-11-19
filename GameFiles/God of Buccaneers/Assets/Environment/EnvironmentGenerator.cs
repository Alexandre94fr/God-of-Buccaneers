using System.Collections.Generic;
using UnityEngine;
using static EnvironmentStatistics;

public class EnvironmentGenerator : MonoBehaviour
{
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

    [Tooltip("BEWARE CAN BE LAGGY ! If this option is ON it will regenerate all the chunks every time the TerrainGenerator script's values are modified.")]
    [SerializeField] bool _liveUpdate;
    [field: SerializeField] public EnvironmentOptionsStruct EnvironmentOptions { get; private set; }

    // Environment parameters
    [field: Header("Environment options :")]
    [SerializeField] EnvironmentStatistics _environmentStatisticsScriptableObject;

    // Terrain and Ocean chunk management
    readonly List<TerrainChunk> _terrainChunkComponents = new();
    readonly List<OceanChunk> _oceanChunkComponents = new();

    Vector2Int _environmentWorldSize;
    float[,] _terrainNoiseMap;

    Vector2Int _allTerrainVertices;
    float _worldSpaceBetweenVertex;

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
            Debug.LogError("<color=red>ERROR !</color> The variable '_terrainChunkPrefab' is null, do you forgot to give the reference ?");

        if (_environmentStatisticsScriptableObject == null)
            Debug.LogError("<color=red>ERROR !</color> The variable '_environmentStatisticsScriptableObject' is null, do you forgot to give the reference ?");

        EnvironmentOptions = _environmentStatisticsScriptableObject.EnvironmentOptions;

        if (!DoesEnvironmentOptionsHaveCorrectValues(EnvironmentOptions))
            return;

        // Setting local references
        _transform = transform;

        // Compute the environment size
        _environmentWorldSize = EnvironmentOptions.ChunkSize * EnvironmentOptions.NumberOfChunks;

        // Compute all the vertices in all the terrains
        _allTerrainVertices = EnvironmentOptions.TerrainChunkVerticeNumberPerLine * EnvironmentOptions.NumberOfChunks;

        _worldSpaceBetweenVertex = (float)EnvironmentOptions.ChunkSize / (EnvironmentOptions.TerrainChunkVerticeNumberPerLine - 1);

        GenerateAllEnvironment();
    }

    private void OnApplicationQuit()
    {
        DestroyAllEnvironment();
    }

    void OnValidate()
    {
        // Securities
        if (!_liveUpdate)
        {
            return;
        }

        if (_terrainChunkComponents.Count == 0 ||
            _allTerrainVertices == Vector2.zero ||
            !DoesEnvironmentOptionsHaveCorrectValues(EnvironmentOptions))
        {
            return;
        }

        GenerateAllEnvironment();
    }

    #region Securizer

    public bool DoesEnvironmentOptionsHaveCorrectValues(EnvironmentOptionsStruct p_environmentOptions)
    {
        #region Sum of factors multiplied by island

        float sumOfFactorsMultipliedByIsland = 
            p_environmentOptions.EnvironmentEdgeFactor +
            p_environmentOptions.IslandSizeFactor * p_environmentOptions.IslandNumber +
            p_environmentOptions.MinimalDistanceBetweenIslandsFactor;

        if (sumOfFactorsMultipliedByIsland >= 0.95f)
        {
            Debug.LogError(
                $"ERROR ! The sum of all the EnvironmentOptions' factors multiplied by the IslandNumber {p_environmentOptions.IslandNumber} " +
                $"EnvironmentEdgeFactor {p_environmentOptions.EnvironmentEdgeFactor}, " +
                $"IslandSizeFactor {p_environmentOptions.IslandSizeFactor}, " +
                $"and MinimalDistanceBetweenIslandsFactor {p_environmentOptions.MinimalDistanceBetweenIslandsFactor}, " +
                $"is superiour or equal to 0.95f.\n" +
                $"That can cause infinite loop when generating environment if not handled correctly.\n\n" +
                $"Maths :\n" +
                $"p_environmentOptions.EnvironmentEdgeFactor +\n" +
                $"p_environmentOptions.IslandSizeFactor * p_environmentOptions.IslandNumber +\n" +
                $"p_environmentOptions.MinimalDistanceBetweenIslandsFactor\n" +
                $"Result : {sumOfFactorsMultipliedByIsland} >= 0.95f\n"
            );
            return false;
        }
        #endregion

        if (p_environmentOptions.EnvironmentEdgeFactor == 0 || 
            p_environmentOptions.IslandNumber == 0 ||
            p_environmentOptions.IslandSizeFactor == 0 ||
            p_environmentOptions.MinimalDistanceBetweenIslandsFactor == 0)
        {
            Debug.LogError(
                "ERROR ! Some values insind of EnvironmentOptions are equal to 0, " +
                "which cause game breaker procedural generation bugs.\n" +
                "Values checked :\n" +
                $"- EnvironmentEdgeFactor : {p_environmentOptions.EnvironmentEdgeFactor}\n" +
                $"- IslandNumber : {p_environmentOptions.IslandNumber}\n" +
                $"- IslandSizeFactor : {p_environmentOptions.IslandSizeFactor}\n" +
                $"- MinimalDistanceBetweenIslandsFactor : {p_environmentOptions.MinimalDistanceBetweenIslandsFactor}\n"
            );
            return false;
        }

        return true;
    }
    #endregion

    void GenerateAllEnvironment()
    {
        DestroyAllEnvironment();

        if (_isDebugOn) 
            TimeCounter.StartTimer(0);

        List<Vector2> islandCenterWorldPositions = GetIslandPositionInTerrain(
            ref _allTerrainVertices,
            EnvironmentOptions.IslandNumber,
            EnvironmentOptions.MinimalDistanceBetweenIslandsFactor,
            EnvironmentOptions.EnvironmentEdgeFactor,
            EnvironmentOptions.IslandSizeFactor
        );

        if (_isDebugOn)
        {
            TimeCounter.StopTimer(0, true, "GetIslandPositionInTerrain");

            TimeCounter.StartTimer(1);
        }

        GenerateTerrain(islandCenterWorldPositions);

        if (_isDebugOn) 
            TimeCounter.StopTimer(1, true, "GenerateTerrain");

    }

    List<Vector2> GetIslandPositionInTerrain(
        ref Vector2Int p_terrainSizeInVertices,
        int p_wantedIslandNumber,
        float p_minimalDistanceBetweenIslandsFactor,
        float p_environmentEdgeFactor,
        float p_islandSizeFactor,
        int p_maxWhileIterationNumber = 100)
    {
        int p_currentFoundedIslandNumber = 0;
        int p_currentWhileIterationCount = 0;

        List<Vector2> validedIslandPositions = new(p_currentWhileIterationCount);

        #region Computing a new terrain size in vertices without the borders

        float terrainSizeInVerticesAverage = (float)(p_terrainSizeInVertices.x + p_terrainSizeInVertices.y) / 2;
        float islandSizeInVerticesSize = p_islandSizeFactor * terrainSizeInVerticesAverage;
        float minimalDistanceBetweenIslandsInVerticesSize = p_minimalDistanceBetweenIslandsFactor * terrainSizeInVerticesAverage;

        Vector2Int minimalUsableTerrainPosition = new(
            Mathf.RoundToInt((p_terrainSizeInVertices.x * p_environmentEdgeFactor) + islandSizeInVerticesSize),
            Mathf.RoundToInt((p_terrainSizeInVertices.y * p_environmentEdgeFactor) + islandSizeInVerticesSize)
        );

        Vector2Int maximalUsableTerrainPosition = new(
            Mathf.RoundToInt((p_terrainSizeInVertices.x * (1 - p_environmentEdgeFactor)) - islandSizeInVerticesSize),
            Mathf.RoundToInt((p_terrainSizeInVertices.y * (1 - p_environmentEdgeFactor)) - islandSizeInVerticesSize)
        );
        #endregion

        #region Getting island positions in vertices

        // Get a random value based on the given seed (the value will be the same if the seed doesn't change)
        System.Random randomValue = new(EnvironmentOptions.Seed);

        while (!(p_currentFoundedIslandNumber == p_wantedIslandNumber))
        {
            // Security
            if (p_currentWhileIterationCount >= p_maxWhileIterationNumber)
            {
                Debug.LogError($"Reached maximum recursion limit ({p_currentWhileIterationCount} / {p_maxWhileIterationNumber}).\n");
                return null;
            }
            
            // NOTE : The second parameter of UnityEngine.Random.Range for int is exclusive, that's why we do "+ 1"
            Vector2Int potentialIslandPosition = new(
                randomValue.Next(minimalUsableTerrainPosition.x, maximalUsableTerrainPosition.x + 1),
                randomValue.Next(minimalUsableTerrainPosition.y, maximalUsableTerrainPosition.y + 1)
            );

            bool isPotentialIslandAtGoodPosition = true;

            foreach (Vector2 validedIslandPosition in validedIslandPositions)
            {
                if (Vector2.Distance(potentialIslandPosition, validedIslandPosition) <=
                    minimalDistanceBetweenIslandsInVerticesSize + islandSizeInVerticesSize)
                {
                    isPotentialIslandAtGoodPosition = false;
                    break;
                }
            }

            if (isPotentialIslandAtGoodPosition)
            {
                validedIslandPositions.Add(potentialIslandPosition);

                p_currentFoundedIslandNumber++;
            }

            p_currentWhileIterationCount++;
        }
        #endregion

        #region Converting island positions into world position

        List<Vector2> validedIslandWorldPositions = new(p_currentFoundedIslandNumber);

        for (int i = 0; i < validedIslandPositions.Count; i++)
        {
            validedIslandWorldPositions.Add(validedIslandPositions[i] * _worldSpaceBetweenVertex);
        }
        #endregion

        #region Debuging

        if (_isDebugOn)
        {
            Debug.Log(
                $"{p_currentFoundedIslandNumber} / {p_wantedIslandNumber} islands have been founded.\n" +
                $"While iteration count : {p_currentWhileIterationCount}"
            );

            for (int i = 0; i < validedIslandPositions.Count; i++)
            {
                Debug.Log(
                    $"Position in vertices : {validedIslandPositions[i]} \n" +
                    $"Position in world : {validedIslandWorldPositions[i]}"
                );

                Vector3 debugIslandCenterPosition = new(
                    validedIslandWorldPositions[i].x,
                    20,
                    validedIslandWorldPositions[i].y
                );

                // This will cause a lot of warnings when called by OnValidate (because Instantiate send message)
                GameObject debugIslandCenter = Instantiate(
                    _debugIslandCenterPrefab,
                    debugIslandCenterPosition,
                    Quaternion.identity,
                    _transform
                );

                debugIslandCenter.name = $"{i}. DebugIslandCenter (x.{debugIslandCenterPosition.x} z.{debugIslandCenterPosition.z})";

                _debugIslandCenterPrefabs.Add(debugIslandCenter);
            }
        }
        #endregion

        return validedIslandWorldPositions;
    }

    void GenerateTerrain(List<Vector2> p_islandCenterWorldPositions)
    {
        double oceanTimeElapsed = 0;
        double terrainTimeElapsed = 0;

        #region Creation of Terrain and Ocean chunks

        for (int x = 0; x < EnvironmentOptions.NumberOfChunks.x; x++)
        {
            for (int z = 0; z < EnvironmentOptions.NumberOfChunks.y; z++)
            {
                if (_isDebugOn) 
                    TimeCounter.StartTimer(2);

                #region Terrain chunk

                // Computing the terrain chunk position
                Vector3 chunkWorldPosition = new(
                    x * EnvironmentOptions.ChunkSize + _transform.position.x,
                    0,
                    z * EnvironmentOptions.ChunkSize + _transform.position.z
                );

                // Creation of the terrain chunk GameObject
                GameObject terrainChunkGameObject = Instantiate(_terrainChunkPrefab, chunkWorldPosition, Quaternion.identity, _terrainParent.transform);

                // Naming the terrain chunk GameObject
                terrainChunkGameObject.name = $"TerrainChunk ({x}x, {z}z)";

                if (!terrainChunkGameObject.TryGetComponent(out TerrainChunk terrainChunkComponent))
                    Debug.LogError($"<color=red>ERROR !</color> The Terrain chunk GameObject '{terrainChunkGameObject.name}' don't have an OceanChunk Script");

                _terrainChunkComponents.Add(terrainChunkComponent);

                // Generating the terrain chunk
                terrainChunkComponent.GenerateTerrainChunk(
                    EnvironmentOptions,
                    p_islandCenterWorldPositions,
                    new Vector2(chunkWorldPosition.x, chunkWorldPosition.z)
                );
                #endregion

                if (_isDebugOn)
                {
                    terrainTimeElapsed += TimeCounter.StopTimer(2, false);

                    TimeCounter.StartTimer(3);
                }

                #region Ocean chunk

                // Computing the ocean chunk position
                Vector3 oceanChunkPosition = new(
                    x * EnvironmentOptions.ChunkSize + _transform.position.x,
                    EnvironmentOptions.WaterLevel,
                    z * EnvironmentOptions.ChunkSize + _transform.position.z
                );

                // Creation of the ocean chunk GameObject
                GameObject oceanChunkGameObject = Instantiate(_oceanChunkPrefab, oceanChunkPosition, Quaternion.identity, _oceanParent.transform);

                // Naming the ocean chunk GameObject
                oceanChunkGameObject.name = $"OceanChunk ({x}x, {z}z)";

                if (!oceanChunkGameObject.TryGetComponent(out OceanChunk oceanChunkComponent))
                    Debug.LogError($"ERROR ! The Ocean chunk GameObject '{oceanChunkGameObject.name}' don't have an OceanChunk Script");

                _oceanChunkComponents.Add(oceanChunkComponent);

                oceanChunkComponent.GenerateOceanChunkMesh(EnvironmentOptions);


                #endregion

                if (_isDebugOn) 
                    oceanTimeElapsed += TimeCounter.StopTimer(3, false);
            }
        }
        #endregion

        if (_isDebugOn)
        {
            Debug.Log("TerrainChunk. " + terrainTimeElapsed + " s.");
            Debug.Log("OceanChunk. " + oceanTimeElapsed + " s.");
        }
    }

    void DestroyAllEnvironment()
    {
        // Security
        if (_terrainChunkComponents.Count == 0)
            return;

        // Destruction of all GameObjects links to the environment
        foreach (TerrainChunk terrainChunkComponent in _terrainChunkComponents)
        {
            Destroy(terrainChunkComponent.gameObject);
        }

        foreach (OceanChunk oceanChunkComponent in _oceanChunkComponents)
        {
            Destroy(oceanChunkComponent.gameObject);
        }

        foreach (GameObject debugIslandCenter in _debugIslandCenterPrefabs)
        {
            Destroy(debugIslandCenter);
        }

        // Reset the lists
        _terrainChunkComponents.Clear();
        _oceanChunkComponents.Clear();
        _debugIslandCenterPrefabs.Clear();
    }

    #region NOT USED FOR NOW (has been used in the past)
    /*
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

                print("(p_x < p_terrainSizeInVertices.x * Mathf.Abs(environmentEdgeFactor - 1) && p_x > p_terrainSizeInVertices.x * environmentEdgeFactor) && "
                    + "(p_z < p_terrainSizeInVertices.y * Mathf.Abs(environmentEdgeFactor - 1) && p_z > p_terrainSizeInVertices.y * environmentEdgeFactor)");
                
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
    }*/
    #endregion

    #endregion
}