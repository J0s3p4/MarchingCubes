using UnityEngine;

[RequireComponent(typeof(MarchingCubes))]
public class FoliageSpawner : MonoBehaviour
{
    [Header("Foliage Settings")]
    public GameObject treePrefab;    // Single tree prefab
    public int treeCount = 20;       // Number of random sample attempts per chunk

    [Header("Noise Settings")]
    public float noiseScale = 20f;   // Larger = smoother distribution
    public float noiseThreshold = 0.5f; // Only spawn trees where Perlin > threshold

    // Cached references
    private MarchingCubes marchingCubes;
    private MeshCollider meshCollider;

    private Vector3 chunkOrigin;
    private int chunkSize;

    private void Start()
    {
        marchingCubes = GetComponent<MarchingCubes>();
        meshCollider = GetComponent<MeshCollider>();

        // Extract from MarchingCubes
        chunkOrigin = marchingCubes.chunkOffset;
        chunkSize = marchingCubes.width;  // width = chunk size in X and Z

        // Ensure the mesh exists before spawning trees
        SpawnFoliage();
    }

    void SpawnFoliage()
    {
        if (treePrefab == null)
        {
            Debug.LogWarning("FoliageSpawner: No prefab assigned.");
            return;
        }

        for (int i = 0; i < treeCount; i++)
        {
            TrySpawnFoliage();
        }
    }

    void TrySpawnFoliage()
    {
        // Pick a random point inside the chunk bounds
        float randX = Random.Range(0f, chunkSize);
        float randZ = Random.Range(0f, chunkSize);

        Vector3 samplePos = new Vector3(chunkOrigin.x + randX, 200f, chunkOrigin.z + randZ);

        // Perlin noise for natural distribution
        float noiseValue = Mathf.PerlinNoise(
            (samplePos.x + 0.1f) / noiseScale,
            (samplePos.z + 0.1f) / noiseScale
        );

        if (noiseValue < noiseThreshold)
            return; // Skip this sample

        // Raycast down to find the terrain surface
        if (Physics.Raycast(samplePos, Vector3.down, out RaycastHit hit, 500f))
        {
            // Make sure we hit THIS chunk's collider
            if (hit.collider == meshCollider)
            {
                Instantiate(
                    treePrefab,
                    hit.point,
                    Quaternion.identity,
                    transform // parent under chunk
                );
            }
        }
    }
}