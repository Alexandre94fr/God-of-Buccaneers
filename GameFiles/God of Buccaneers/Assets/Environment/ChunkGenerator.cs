using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkGenerator
{
    #region GenerateMesh methods

    /// <summary> 
    /// Will generate and return a chunk (a mesh) based on the given parameters. Made to work for generating procedural terrain chunk. </summary>
    /// <param name = "p_meshName"> The name of the created mesh. </param>
    /// <param name = "p_environmentOptions"> A struct that containts all the environment's options. </param>
    /// <param name = "p_verticeNumberPerLine"> The total number of vertices in x axis. </param>
    /// <param name = "p_islandCenterWorldPosition"> The list of all the island center world position. </param>
    /// <param name = "p_chunkWorldPosition"> The position of the chunk were this mesh will be generated </param>
    public static Mesh GenerateMesh(
        string p_meshName,
        EnvironmentStatistics.EnvironmentOptionsStruct p_environmentOptions,
        int p_verticeNumberPerLine,
        List<Vector2> p_islandCenterWorldPosition,
        Vector2 p_chunkWorldPosition)
    {
        // To optimize
        float meshSize = p_environmentOptions.ChunkSize;
        float spaceBetweenVertex = meshSize / (p_verticeNumberPerLine - 1);

        int verticesNumber = p_verticeNumberPerLine * p_verticeNumberPerLine;

        // NOTE : The " * 6 " is there because in one square there are 3 x 2 (vertices x triangles)  
        int trianglesNumber = (p_verticeNumberPerLine - 1) * (p_verticeNumberPerLine - 1) * 6;

        // Pre-allocation of tables to avoid using dynamic list
        Vector3[] vertices = new Vector3[verticesNumber];
        int[] trianglesPoints = new int[trianglesNumber];

        int triangleIndex = 0;

        // Computing of vertices and triangles
        for (int i = 0, z = 0; z < p_verticeNumberPerLine; z++)
        {
            for (int x = 0; x < p_verticeNumberPerLine; x++, i++)
            {
                // Vertice generation
                vertices[i] = new Vector3(
                    x * spaceBetweenVertex,
                    GetHeight(x, z, p_environmentOptions, p_islandCenterWorldPosition, p_chunkWorldPosition),
                    z * spaceBetweenVertex
                );

                // Triangles generation (not the borders)
                if (x < p_verticeNumberPerLine - 1 && z < p_verticeNumberPerLine - 1)
                {
                    GenerateTriangles(ref trianglesPoints, ref triangleIndex, i, p_verticeNumberPerLine);
                }
            }
        }

        // Mesh creation, and returning it
        return CreateMesh(p_meshName, ref vertices, ref trianglesPoints);
    }

    /// <summary> 
    /// Will generate and return a chunk (a mesh) based on the given parameters. Made to work for generating procedural ocean chunk. </summary>
    /// <param name = "p_environmentOptions"> A struct that containts all the environment's options. </param>
    /// <param name = "p_meshName"> The name of the created mesh. </param>
    /// <param name = "p_verticesHeight"> A reference to the height (y coordonate) you want to give to all your mesh. </param>
    /// <param name = "p_verticeNumberPerLine"> The total number of vertices in x axis. </param>
    public static Mesh GenerateMesh(string p_meshName, EnvironmentStatistics.EnvironmentOptionsStruct p_environmentOptions, float p_verticesHeight, int p_verticeNumberPerLine)
    {
        // To optimize
        float meshSize = p_environmentOptions.ChunkSize;
        float spaceBetweenVertex = meshSize / (p_verticeNumberPerLine - 1);

        int verticesNumber = p_verticeNumberPerLine * p_verticeNumberPerLine;

        // NOTE : The " * 6 " is there because in one square there are 3 x 2 (vertices x triangles)  
        int trianglesNumber = (p_verticeNumberPerLine - 1) * (p_verticeNumberPerLine - 1) * 6;

        // Pre-allocation of tables to avoid using dynamic list
        Vector3[] vertices = new Vector3[verticesNumber];
        int[] trianglesPoints = new int[trianglesNumber];

        int triangleIndex = 0;

        // Computing of vertices and triangles
        for (int i = 0, z = 0; z < p_verticeNumberPerLine; z++)
        {
            for (int x = 0; x < p_verticeNumberPerLine; x++, i++)
            {
                // Vertice generation
                vertices[i] = new Vector3(
                    x * spaceBetweenVertex,
                    p_verticesHeight,
                    z * spaceBetweenVertex
                );

                // Triangles generation (not the borders)
                if (x < p_verticeNumberPerLine - 1 && z < p_verticeNumberPerLine - 1)
                {
                    GenerateTriangles(ref trianglesPoints, ref triangleIndex, i, p_verticeNumberPerLine);
                }
            }
        }

        // Mesh creation, and returning it
        return CreateMesh(p_meshName, ref vertices, ref trianglesPoints);
    }
    #endregion

    #region GenerateMesh sub-methods

    #region For terrain generation only

    /// <summary> Will compute and return the height of a vertex (a point in a mesh) based on the given parameters. </summary>
    /// <param name = "p_x"> The X position of the vertex in the verticeSize </param>
    /// <param name = "p_z"> The Z position of the vertex in the verticeSize </param>
    /// <param name = "p_environmentOptions"> A struct that containts all the terrain's options </param>
    /// <param name = "p_islandCenterWorldPositions"> The list of position (in the vertices) of the center of the islands </param>
    /// <returns> Returns the height (float value) of a vertex (a point in a mesh) </returns>
    static float GetHeight(float p_x, float p_z,
        EnvironmentStatistics.EnvironmentOptionsStruct p_environmentOptions,
        List<Vector2> p_islandCenterWorldPositions,
        Vector2 p_chunkWorldPosition)
    {
        // Those variables will not change
        float worldSpaceBetweenVertex = (float)p_environmentOptions.ChunkSize / (p_environmentOptions.TerrainChunkVerticeNumberPerLine - 1);

        Vector2 vertexWorldPosition = new(
            p_x * worldSpaceBetweenVertex + p_chunkWorldPosition.x,
            p_z * worldSpaceBetweenVertex + p_chunkWorldPosition.y
        );

        // Those variables will change
        float perlinHeight = 0;
        float amplitude = 1;
        float frequency = p_environmentOptions.Frequency;

        for (int i = 0; i < p_environmentOptions.OctaveCount; i++)
        {
            perlinHeight += Mathf.PerlinNoise(
                vertexWorldPosition.x * frequency + p_environmentOptions.Seed,
                vertexWorldPosition.y * frequency + p_environmentOptions.Seed
            ) * amplitude;

            frequency /= p_environmentOptions.Lacunarity;
            amplitude *= p_environmentOptions.Persistance;
        }

        perlinHeight *= p_environmentOptions.HeightDistribution.Evaluate(perlinHeight);

        float averageChunkNumber = (p_environmentOptions.NumberOfChunks.x + p_environmentOptions.NumberOfChunks.y) / 2;
        float heightMultiplier = p_environmentOptions.HeightMultiplier * averageChunkNumber / 3;

        if (!p_environmentOptions.IsIsland)
            perlinHeight *= heightMultiplier;
        else
            perlinHeight *= heightMultiplier * (1 - GetNormalizedDistanceFromIslandCenters(vertexWorldPosition, worldSpaceBetweenVertex, p_environmentOptions, p_islandCenterWorldPositions));

        perlinHeight = Mathf.RoundToInt(perlinHeight);

        return perlinHeight;
    }

    static float GetNormalizedDistanceFromIslandCenters(
        Vector2 p_vertexWorldPosition,
        float p_spaceBetweenVertex,
        EnvironmentStatistics.EnvironmentOptionsStruct p_environmentOptions,
        List<Vector2> p_islandCenterWorldPositions)
    {
        #region Security

        if (p_islandCenterWorldPositions == null) // If you set in run time IslandSizeFactor to 0.2f it loop infinitly
        {
            Debug.LogError("ERROR ! p_islandCenterWorldPositions is null.");
            Debug.Break();
            return 0;
        }
        #endregion

        const float scaleFactor = 0.5f;

        float minimumDistanceInfluence = 1f;

        // Computing the distance
        foreach (Vector2 islandCenterWorldPosition in p_islandCenterWorldPositions)
        {
            float worldDistanceFromIslandCenter = Vector2.Distance(islandCenterWorldPosition, p_vertexWorldPosition) * (scaleFactor / p_environmentOptions.IslandSizeFactor);

            // Normalisation en fonction de la taille totale de l'environnement
            float totalIslandHeightInfluence = Mathf.Clamp01(worldDistanceFromIslandCenter / (p_environmentOptions.ChunkSize * EnvironmentGenerator.Instance.EnvironmentOptions.NumberOfChunks.x * 0.5f));

            minimumDistanceInfluence = Mathf.Min(minimumDistanceInfluence, totalIslandHeightInfluence);
        }

        return minimumDistanceInfluence;
    }
    #endregion

    static void GenerateTriangles(ref int[] p_trianglePoints, ref int p_trianglePointsIndex, int p_cornerIndex, int p_verticeSize)
    {
        // NOTE : p_trianglePointsIndex++ mean we will look for p_trianglePointsIndex value,
        // and after the line is finished, we do p_trianglePointsIndex + 1 

        // First triangle
        p_trianglePoints[p_trianglePointsIndex++] = p_cornerIndex;
        p_trianglePoints[p_trianglePointsIndex++] = p_cornerIndex + p_verticeSize + 1;
        p_trianglePoints[p_trianglePointsIndex++] = p_cornerIndex + 1;
                                           
        // Second triangle                 
        p_trianglePoints[p_trianglePointsIndex++] = p_cornerIndex;
        p_trianglePoints[p_trianglePointsIndex++] = p_cornerIndex + p_verticeSize;
        p_trianglePoints[p_trianglePointsIndex++] = p_cornerIndex + p_verticeSize + 1;

        /* Drawing triangle diagram (X = p_cornerIndex, x = other vertices)

        2|                     2|
        1|  x                  1|x x
        0|X x                  0|X  
          - - -                  - - -
          0 1 2                  0 1 2

        */
    }

    /// <summary>
    /// Creates a mesh according to the given values. </summary>
    /// <param name = "p_meshName"> The name of the created mesh. </param>
    /// <param name = "p_vertices"> Vertice table of the mesh you want to create. It's a 'ref' to avoid copying all the table and causing lag. </param>
    /// <param name = "p_trianglesPoints"> Triangle table of the mesh you want to create. It's a 'ref' to avoid copying all the table and causing lag. </param>
    static Mesh CreateMesh(string p_meshName, ref Vector3[] p_vertices, ref int[] p_trianglesPoints)
    {
        // Mesh creation
        Mesh mesh = new()
        {
            name = p_meshName,
            vertices = p_vertices,
            triangles = p_trianglesPoints
        };

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }
    #endregion
}