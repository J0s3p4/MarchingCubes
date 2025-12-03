using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// Attach Script to a game object with Components: Mesh Filter, Mesh Renderer
public class MarchingCubes : MonoBehaviour
{
    // Generated mesh data
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    MeshFilter meshFilter;

    // Values above 'terrainSurface' are considered "solid"
    [SerializeField] float terrainSurface = 0.5f;

    // Dimensions of the voxel field
    [SerializeField] int width = 32;
    [SerializeField] int height = 8;

    // Scalar field storing density values for each point
    float[,,] terrainMap;

    // Debug helper to inspect specific marching config
    [SerializeField] int _configIndex = -1;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();

        // +1 in each dimension to cover every cube corner
        terrainMap = new float[width + 1, height + 1, width + 1];

        PopulateTerrainMap();
        CreateMeshData();
        BuildMesh();
    }

    // Fill the terrainMap[] with values from Perlin noise
    void PopulateTerrainMap()
    {
        for (int x = 0; x < width + 1; x++)
        {
            for (int y = 0; y < height + 1; y++)
            {
                for (int z = 0; z < width + 1; z++)
                {
                    // Generate height using Perlin noise (scaled down) (arbitrary numbers (16f), the "+ 0.001" is to prevent a whole)
                    float thisHeight = (float)height * Mathf.PerlinNoise(
                        (float)x / 16f * 1.5f + 0.001f,
                        (float)z / 16f * 1.5f + 0.001f
                    );

                    // Set the value of this point in the terrainMap
                    terrainMap[x, y, z] = (float)y - thisHeight;
                }
            }
        }
    }


    void CreateMeshData()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < width; z++)
                {

                    // Generate geometry for this cube
                    MarchCube(new Vector3Int(x, y, z));
                }
            }
        }
    }

    // Determine the marching cubes configuration (0–255)
    int GetCubeConfiguration(float[] cube)
    {
        int configurationIndex = 0;

        for (int i = 0; i < 8; i++) // Loop over the 8 cube corners
        {
            if (cube[i] > terrainSurface)
            {
                // Set bit i if the corner is above the surface threshold
                configurationIndex |= 1 << i;
            }
        }

        return configurationIndex;
    }

    // Generate triangles for one cube according to its configuration index
    void MarchCube(Vector3Int position)
    {

        // Sample terrain values at each corner of the cube
        float[] cube = new float[8];
        for (int i = 0; i < 8; i++)
        {
            cube[i] = SampleTerrain(position + MarchingTable.CornerTable[i]);
        }

        int configIndex = GetCubeConfiguration(cube);

        // Skip completely empty (full -1)
        if (configIndex == 0 || configIndex == 255)
        {
            return;
        }

        int edgeIndex = 0;

        // Up to 5 triangles per cube
        for (int i = 0; i < 5; i++)
        {
            // Each triangle has 3 vertices
            for (int j = 0; j < 3; j++)
            {
                int indice = MarchingTable.TriangleTable[configIndex, edgeIndex];

                if (indice == -1)
                {
                    return; // No more triangles for this configuration
                }

                // Look up the two endpoints of this edge
                Vector3 vert1 = position + MarchingTable.CornerTable[MarchingTable.EdgeIndexes[indice, 0]];
                Vector3 vert2 = position + MarchingTable.CornerTable[MarchingTable.EdgeIndexes[indice, 1]];

                // Midpoint = linear interpolation (not yet true iso-surface)
                Vector3 vertPosition = (vert1 + vert2) / 2f;

                // Store vertex + triangle index
                vertices.Add(vertPosition);
                triangles.Add(vertices.Count - 1);

                edgeIndex++;
            }
        }
    }

    float SampleTerrain (Vector3Int point)
    {
        return terrainMap[point.x, point.y, point.z];
    }


    // Reset generated mesh data
    void ClearMeshData()
    {
        vertices.Clear();
        triangles.Clear();
    }

    // Build final mesh from generated vertices + triangles
    void BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        // Recalculate normals for correct lighting
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}