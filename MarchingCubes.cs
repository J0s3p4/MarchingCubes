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

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
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
