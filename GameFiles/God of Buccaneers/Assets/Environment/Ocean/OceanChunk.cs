using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class OceanChunk : MonoBehaviour
{
    #region Variables

    // References
    MeshFilter _meshFilter;
    MeshRenderer _meshRenderer; // Un-used for now
    MeshCollider _meshCollider;
    #endregion

    #region Methods

    private void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshCollider = GetComponent<MeshCollider>();
    }

    /// <summary> Will generate a ocean chunk (a mesh) based on the given parameters. </summary>
    /// <param name = "p_environmentOptions"> A struct that containts all the terrain's options </param>
    public void GenerateOceanChunkMesh(EnvironmentStatistics.EnvironmentOptionsStruct p_environmentOptions)
    {
        #region Securities

        // Important because GenerateTerrainChunk can be called before the Start methods
        if (_meshFilter == null)
            _meshFilter = GetComponent<MeshFilter>();

        if (_meshRenderer == null)
            _meshRenderer = GetComponent<MeshRenderer>();

        if (_meshCollider == null)
            _meshCollider = GetComponent<MeshCollider>();
        #endregion

        #region BACKUP
        // To optimize
        //int verticeSize = p_environmentOptions.OceanChunkVerticeNumberPerLine;
        //float meshSize = p_environmentOptions.ChunkSize;
        //float waterLevel = p_environmentOptions.WaterLevel;
        //float spaceBetweenVertex = meshSize / (verticeSize - 1);
        //
        //int verticesNumber = verticeSize * verticeSize;
        //
        //// NOTE : The " * 6 " is there because in one square there are 3 x 2 (vertices x triangles)  
        //int trianglesNumber = (verticeSize - 1) * (verticeSize - 1) * 6; 
        //
        //// Pre-allocation of tables to avoid using dynamic list
        //Vector3[] vertices = new Vector3[verticesNumber];
        //int[] trianglesPoints = new int[trianglesNumber];
        //
        //int triangleIndex = 0;
        //
        //// Computing of vertices and triangles
        //for (int i = 0, z = 0; z < verticeSize; z++)
        //{
        //    for (int x = 0; x < verticeSize; x++, i++)
        //    {
        //        // Vertice generation
        //        vertices[i] = new Vector3(
        //            x * spaceBetweenVertex,
        //            waterLevel,
        //            z * spaceBetweenVertex
        //        );
        //
        //        // Triangles generation (not the borders)
        //        if (x < verticeSize - 1 && z < verticeSize - 1)
        //        {
        //            int cornerIndex = i;
        //            int nextRow = verticeSize;
        //
        //            // NOTE : triangleIndex++ mean we will look for triangleIndex value,
        //            // and after the line is finished, we do triangleIndex + 1 
        //
        //            // First triangle
        //            trianglesPoints[triangleIndex++] = cornerIndex;
        //            trianglesPoints[triangleIndex++] = cornerIndex + nextRow + 1;
        //            trianglesPoints[triangleIndex++] = cornerIndex + 1;
        //
        //            // Second triangle
        //            trianglesPoints[triangleIndex++] = cornerIndex;
        //            trianglesPoints[triangleIndex++] = cornerIndex + nextRow;
        //            trianglesPoints[triangleIndex++] = cornerIndex + nextRow + 1;
        //
        //            /* Drawing triangle diagram (X = cornerIndex, x = other vertices)
        //
        //            2|                     2|
        //            1|  x                  1|x x
        //            0|X x                  0|X  
        //              - - -                  - - -
        //              0 1 2                  0 1 2
        //
        //            */
        //        }
        //    }
        //}
        //
        //// Mesh creation
        //Mesh oceanChunkMesh = new()
        //{
        //    name = "OceanMesh",
        //    vertices = vertices,
        //    triangles = trianglesPoints
        //};
        //
        //oceanChunkMesh.RecalculateBounds();
        //oceanChunkMesh.RecalculateNormals();
        #endregion

        // Generating the ocean mesh
        Mesh oceanChunkMesh = ChunkGenerator.GenerateMesh(
            "OceanChunk",
            p_environmentOptions,
            p_environmentOptions.WaterLevel,
            p_environmentOptions.OceanChunkVerticeNumberPerLine
        );

        // Updating the MeshFilter and the MeshCollider
        _meshFilter.mesh = oceanChunkMesh;
        _meshCollider.sharedMesh = oceanChunkMesh;
    }

    public void ChangeOceanShaderMaterial(Material p_newMaterial)
    {
        _meshRenderer.material = p_newMaterial;
    }
    #endregion
}