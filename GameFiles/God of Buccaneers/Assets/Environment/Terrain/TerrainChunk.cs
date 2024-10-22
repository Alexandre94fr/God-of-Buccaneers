using System.Collections.Generic;
using UnityEngine;
using static EnvironmentGenerator;

public class TerrainChunk : MonoBehaviour
{
    #region Variables

    // References
    MeshRenderer _meshRenderer; // Un-used for now
    MeshFilter _meshFilter;
    #endregion

    #region Methods

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshFilter = GetComponent<MeshFilter>();
    }

    /// <summary> Will generate a chunk (a mesh) based on the given parameters. </summary>
    /// <param name = "p_environmentOptions"> A struct that containts all the terrain's options </param>
    public void GenerateTerrainChunk(EnvironmentOptionsStruct p_environmentOptions)
    {
        // To optimize
        int verticeSize = p_environmentOptions.VerticeSize;
        float meshSize = p_environmentOptions.ChunkSize;
        float waterLevel = p_environmentOptions.WaterLevel - 0.01f;

        if (_meshFilter == null)
        {
            _meshFilter = GetComponent<MeshFilter>();
        }

        List<Vector3> vertices = new();
        List<int> trianglesPoints = new();
        List<int> waterTrianglePoints = new();

        // Generate the points (vertices)
        Vector3[,] heightMap = new Vector3[verticeSize, verticeSize];

        for (int x = 0; x < verticeSize; x++)
        {
            for (int z = 0; z < verticeSize; z++)
            {
                float spaceBetweenVertex = meshSize / (verticeSize - 1);

                // Creation of the vertex (it's only a position now,
                // it will be converted in real vertex with Unity's function SetVertices())
                Vector3 vertex = new(
                    x * spaceBetweenVertex,
                    GetHeight(x, z, p_environmentOptions),
                    z * spaceBetweenVertex
                );

                heightMap[x, z] = vertex;
                vertices.Add(vertex);
            }
        }

        // Generate the triangles (connection between vertices)
        for (int x = 0; x < heightMap.GetLength(0) - 1; x++)
        {
            for (int z = 0; z < heightMap.GetLength(1) - 1; z++)
            {
                // The vertex position in the terrain that we will use to start drawing our triangle
                int cornerIndex = x + z * verticeSize;

                // Creating the first triangle possible with cornerIndex 
                trianglesPoints.Add(cornerIndex);
                trianglesPoints.Add(cornerIndex + 1);
                trianglesPoints.Add(cornerIndex + 1 + verticeSize);

                // Creating the first triangle possible with cornerIndex
                trianglesPoints.Add(cornerIndex);
                trianglesPoints.Add(cornerIndex + 1 + verticeSize);
                trianglesPoints.Add(cornerIndex + verticeSize);

                /* Drawing triangle diagram (X = cornerIndex, x = other vertices)
                2|                     2|
                1|  x                  1|x x
                0|X x                  0|X  
                  - - -                  - - -
                  0 1 2                  0 1 2
                */
            }
        }

        // Creation of all the components for our water submesh (Ocean)
        //if (p_environmentOptions.HasWater)
        //{
        //    // Creation of the 4 vertices of the water submesh
        //    vertices.Add(new Vector3(0,        waterLevel, 0));
        //    vertices.Add(new Vector3(meshSize, waterLevel, 0));
        //    vertices.Add(new Vector3(0,        waterLevel, meshSize));
        //    vertices.Add(new Vector3(meshSize, waterLevel, meshSize));
        //
        //    // Creating the first triangle
        //    waterTrianglePoints.Add(vertices.Count - 1);
        //    waterTrianglePoints.Add(vertices.Count - 3);
        //    waterTrianglePoints.Add(vertices.Count - 2);
        //
        //    // Creating the second triangle
        //    waterTrianglePoints.Add(vertices.Count - 4);
        //    waterTrianglePoints.Add(vertices.Count - 2);
        //    waterTrianglePoints.Add(vertices.Count - 3);
        //}

        Mesh terrainMesh = new()
        {
            subMeshCount = 2
        };

        terrainMesh.SetVertices(vertices);
        terrainMesh.SetTriangles(trianglesPoints, 0);
        terrainMesh.SetTriangles(waterTrianglePoints, 1);

        terrainMesh.RecalculateBounds();
        terrainMesh.RecalculateNormals();

        _meshFilter.mesh = terrainMesh;
    }

    /// <summary> Will compute and return the height of a vertex (a point in a mesh) based on the given parameters. </summary>
    /// <param name = "p_x"> The X position of the vertex in the verticeSize </param>
    /// <param name = "p_z"> The Z position of the vertex in the verticeSize </param>
    /// <param name = "p_environmentOptions"> A struct that containts all the terrain's options </param>
    /// <returns> Returns the height (float value) of a vertex (a point in a mesh) </returns>
    float GetHeight(float p_x, float p_z, EnvironmentOptionsStruct p_environmentOptions)
    {
        float perlinHeight = 0;

        float amplitude = 1;
        float frequency = p_environmentOptions.PerlinScale;
        float spaceBetweenVertex = p_environmentOptions.ChunkSize / (p_environmentOptions.VerticeSize - 1);

        // To optimize
        Vector3 chunkWorldPosition = transform.position;

        Vector3 worldPosition = new(
            p_x * spaceBetweenVertex + chunkWorldPosition.x,
            0,
            p_z * spaceBetweenVertex + chunkWorldPosition.z
        );

        for (int i = 0; i < p_environmentOptions.OctaveCount; i++)
        {
            perlinHeight += Mathf.PerlinNoise(
                worldPosition.x * frequency + p_environmentOptions.Seed,
                worldPosition.z * frequency + p_environmentOptions.Seed
            ) * amplitude;

            float heightModifier = p_environmentOptions.HeightDistribution.Evaluate(perlinHeight);

            perlinHeight *= heightModifier;

            frequency /= p_environmentOptions.Lacunarity;
            amplitude *= p_environmentOptions.Persistance;
        }

        if (!p_environmentOptions.IsIsland)
            perlinHeight *= p_environmentOptions.HeightMultiplier;
        else
            perlinHeight *= p_environmentOptions.HeightMultiplier * (1 - GetNormalizedDistanceFromTerrainCenter(p_x, p_z, spaceBetweenVertex, p_environmentOptions));

        // USED TO PLACE A BREAKPOINT
        //if (perlinHeight > 1)
        //    return p_environmentOptions.NumberOfLayersPerUnitOfHeight;

        perlinHeight = Mathf.RoundToInt(perlinHeight);

        //perlinHeight /= p_environmentOptions.NumberOfLayersPerUnitOfHeight;

        return perlinHeight;
    }

    float GetNormalizedDistanceFromTerrainCenter(float p_x, float p_z, float p_spaceBetweenVertex, EnvironmentOptionsStruct p_environmentOptions)
    {
        Vector2 terrainCenter = new(p_environmentOptions.ChunkSize / 2, p_environmentOptions.ChunkSize / 2);

        Vector2 vertexPos = new(p_x * p_spaceBetweenVertex, p_z * p_spaceBetweenVertex);

        // Computing the distance
        float distanceFromTerrainCenter = Vector2.Distance(terrainCenter, vertexPos);

        // Returning the normalization
        return Mathf.Clamp01(distanceFromTerrainCenter / (p_environmentOptions.ChunkSize / 2));
    }
    #endregion
}