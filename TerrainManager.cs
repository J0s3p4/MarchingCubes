using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    // The MarchingCubes script attached to the chunk prefab
    public MarchingCubes chunkPrefab;

    // The player's transform to follow
    public Transform player;

    // Radius of chunks (in chunk units) to generate around the player
    public int viewDistance = 8;

    // ToDo, Get chunk size from marching cubes?
    // Size of a single chunk (should match the dimensions in MarchingCubes)
    private readonly int chunkSize = 32;

    // Dictionary to store and track currently active chunks
    private Dictionary<Vector3Int, MarchingCubes> activeChunks = new Dictionary<Vector3Int, MarchingCubes>();

    private Vector3Int lastPlayerChunkCoord;

    private void Start()
    {
        // Initial setup for the last recorded chunk coordinate
        lastPlayerChunkCoord = GetPlayerChunkCoord();
        GenerateChunks();
    }

    private void Update()
    {
        // Check if the player has moved into a new chunk
        Vector3Int currentPlayerChunkCoord = GetPlayerChunkCoord();

        if (currentPlayerChunkCoord != lastPlayerChunkCoord)
        {
            lastPlayerChunkCoord = currentPlayerChunkCoord;
            GenerateChunks();
        }
    }

    // Converts player's world position to chunk coordinates (e.g., (32, 0, 32) -> (1, 0, 1))
    Vector3Int GetPlayerChunkCoord()
    {
        return new Vector3Int(
            Mathf.FloorToInt(player.position.x / chunkSize),
            0, // We assume infinite terrain is only on the XZ plane for simplicity
            Mathf.FloorToInt(player.position.z / chunkSize)
        );
    }

    // Primary function to generate new chunks and destroy old ones
    void GenerateChunks()
    {
        // Generate/Update Chunks 
        Vector3Int playerCoord = GetPlayerChunkCoord();
        HashSet<Vector3Int> chunksToKeep = new HashSet<Vector3Int>();

        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int z = -viewDistance; z <= viewDistance; z++)
            {
                // Calculate world coordinates for the chunk
                Vector3Int chunkCoord = new Vector3Int(playerCoord.x + x, 0, playerCoord.z + z);
                chunksToKeep.Add(chunkCoord);

                if (!activeChunks.ContainsKey(chunkCoord))
                {
                    // Calculate the world position where the chunk should be placed
                    Vector3 chunkWorldPos = new Vector3(
                        chunkCoord.x * chunkSize,
                        0, // Chunks are on the y=0 plane
                        chunkCoord.z * chunkSize
                    );

                    // Instantiate and initialize the new chunk
                    MarchingCubes newChunk = Instantiate(chunkPrefab, chunkWorldPos, Quaternion.identity, transform);

                    // Crucial: Set the chunk's offset before it generates its mesh
                    newChunk.chunkOffset = chunkWorldPos;

                    activeChunks.Add(chunkCoord, newChunk);
                }
            }
        }

        // Destroy Far-Away Chunks
        List<Vector3Int> chunksToDestroy = new List<Vector3Int>();

        foreach (var pair in activeChunks)
        {
            if (!chunksToKeep.Contains(pair.Key))
            {
                chunksToDestroy.Add(pair.Key);
            }
        }

        foreach (Vector3Int chunkCoord in chunksToDestroy)
        {
            Destroy(activeChunks[chunkCoord].gameObject);
            activeChunks.Remove(chunkCoord);
        }
    }
}