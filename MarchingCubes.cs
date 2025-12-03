using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// Attach Script to a game object with Components: Mesh Filter, Mesh Renderer
public class MarchingCubes : MonoBehaviour
{

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    MeshFilter meshFilter;

    [SerializeField] int _configIndex = -1;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _configIndex++;
            ClearMeshData();
            MarchCube(Vector3.zero, _configIndex);
            BuildMesh();
        }
    }

    // position - position of given cube, configIndex - index fron the triangle table
    void MarchCube (Vector3 position, int configIndex)
    {
        if (configIndex == 0 || configIndex == 255)
        {
            return; // 'nothing there' for 0 and 255
        }

        int edgeIndex = 0;
        // No more than 5 triangles in each
        for (int i = 0; i < 5; i++)
        {
            // No more than 3 points in a triangle
            for (int j = 0; j < 3; j++)
            {
                int indice = MarchingTable.TriangleTable[configIndex, edgeIndex];

                if (indice == -1)
                {
                    return;
                }

                // Start and edge of a cube
                Vector3 vert1 = position + MarchingTable.EdgeTable[indice, 0];
                Vector3 vert2 = position + MarchingTable.EdgeTable[indice, 1];

                // Midpoint between the two verts
                Vector3 vertPosition = (vert1 + vert2) / 2f;

                vertices.Add(vertPosition);
                triangles.Add(vertices.Count - 1);
                edgeIndex++;
            }
        }
    }

    void ClearMeshData()
    {
        vertices.Clear();
        triangles.Clear();
    }

    void BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }



}
