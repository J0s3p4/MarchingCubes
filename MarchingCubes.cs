using System.Collections.Generic;
using UnityEngine;

// Attach Script to a game object with Components: Mesh Filter, Mesh Renderer, Mesh Collider.
// This script is intended to be used as a Prefab and managed by a TerrainManager.
public class MarchingCubes : MonoBehaviour
{
    // Public property set by the TerrainManager to offset Perlin noise sampling
    [HideInInspector] public Vector3 chunkOffset;

    // Generated mesh data
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    // Values above 'terrainSurface' are considered "solid"
    [SerializeField] float terrainSurface = 0.5f;

    // Dimensions of the voxel field (Chunk size - MUST match the value used in TerrainManager)
    [SerializeField] int width = 32;
    [SerializeField] int height = 8;

    // Scalar field storing density values for each point
    private float[,,] terrainMap;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        // +1 in each dimension to cover every cube corner
        terrainMap = new float[width + 1, height + 1, width + 1];

        PopulateTerrainMap();
        CreateMeshData();
        BuildMesh();
    }

    // Fill the terrainMap[] with values from Perlin noise
    void PopulateTerrainMap()
    {
        // Caching the combined scaling factor for noise lookup
        float noiseScale = 16f * 1.5f;

        for (int x = 0; x < width + 1; x++)
        {
            for (int y = 0; y < height + 1; y++)
            {
                for (int z = 0; z < width + 1; z++)
                {
                    // --- CRITICAL CHANGE ---
                    // Calculate the absolute world position of this voxel corner
                    // This ensures the noise is continuous across chunk boundaries
                    float worldX = chunkOffset.x + x;
                    float worldZ = chunkOffset.z + z;

                    // Generate height using Perlin noise, based on world position
                    float thisHeight = (float)height * Mathf.PerlinNoise(
                        worldX / noiseScale + 0.001f, // Use worldX
                        worldZ / noiseScale + 0.001f  // Use worldZ
                    );

                    // Set the value of this point in the terrainMap
                    // (y coordinate minus the calculated height)
                    terrainMap[x, y, z] = (float)y - thisHeight;
                }
            }
        }
    }


    void CreateMeshData()
    {
        // Clear previous data before generating new mesh data
        ClearMeshData();

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
                // Set bit i if the corner is above the surface threshold (solid)
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

        // Skip completely empty or completely full cubes
        if (configIndex == 0 || configIndex == 255)
        {
            return;
        }

        int edgeIndex = 0;

        // Iterate through the triangles defined for this configuration
        for (int i = 0; i < 5; i++)
        {
            // Each triangle has 3 vertices
            for (int j = 0; j < 3; j++)
            {
                int indice = MarchingTable.TriangleTable[configIndex, edgeIndex];

                if (indice == -1)
                {
                    return; // No more vertices/triangles for this configuration
                }

                // Look up the two endpoints of this edge
                Vector3 vert1 = position + MarchingTable.CornerTable[MarchingTable.EdgeIndexes[indice, 0]];
                Vector3 vert2 = position + MarchingTable.CornerTable[MarchingTable.EdgeIndexes[indice, 1]];

                // Midpoint = linear interpolation (simple midpoint for now)
                Vector3 vertPosition = (vert1 + vert2) / 2f;

                // Store vertex and triangle index
                vertices.Add(vertPosition);
                triangles.Add(vertices.Count - 1);

                edgeIndex++;
            }
        }
    }

    float SampleTerrain(Vector3Int point)
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
        meshCollider.sharedMesh = mesh;
    }
}