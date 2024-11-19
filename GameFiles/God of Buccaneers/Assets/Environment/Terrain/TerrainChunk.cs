using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainChunk : MonoBehaviour
{
    #region Variables

    // References
    MeshFilter _meshFilter;
    MeshRenderer _meshRenderer; // Un-used for now
    #endregion

    #region Methods

    private void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    /// <summary> Will generate a terrain chunk (a mesh) based on the given parameters. </summary>
    /// <param name = "p_environmentOptions"> A struct that containts all the terrain's options </param>
    /// <param name = "p_islandCenterWorldPositions"> The list of position (in the vertices) of the center of the islands </param>
    public void GenerateTerrainChunk(
        EnvironmentStatistics.EnvironmentOptionsStruct p_environmentOptions,
        List<Vector2> p_islandCenterWorldPositions,
        Vector2 p_chunkWorldPosition)
    {
        #region Securities

        // Important because GenerateTerrainChunk can be called before the Start method
        if (_meshFilter == null)
            _meshFilter = GetComponent<MeshFilter>();

        if (_meshRenderer == null)
            _meshRenderer = GetComponent<MeshRenderer>();
        #endregion

        // Generating the terrain mesh
        Mesh terrainChunkMesh = ChunkGenerator.GenerateMesh(
            "TerrainChunk",
            p_environmentOptions,
            p_environmentOptions.TerrainChunkVerticeNumberPerLine,
            p_islandCenterWorldPositions,
            p_chunkWorldPosition
        );
        
        // Updating the MeshFilter
        _meshFilter.mesh = terrainChunkMesh;
    }
    #endregion
}