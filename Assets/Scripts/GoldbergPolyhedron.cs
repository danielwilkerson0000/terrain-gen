using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// THIS WAS LARGELY GENERATED WITH THE HELP OF GOOGLE GEMINI - MANY THANKS.
/// </summary>
public class GoldbergPolyhedron : MonoBehaviour
{
    [Header("Polyhedron Settings")]
    [Range(1, 5)] public int frequency = 1;
    public float radius = 1f;
    // [Range(0.5f, 1.01f)] 
    private float cellScale = 1f; // Creates gaps between tiles

    [Header("Materials")]
    public Material hexagonMaterial;
    public Material pentagonMaterial;

    private List<Vector3> primalVertices;
    private List<int> primalTriangles;

    public List<GameObject> tiles;

    public float faceScale = 0.25f;

    public void Generate(Transform transform)
    {
        tiles = new();

        faceScale = 0.5f * Mathf.Pow(1 / 2f, frequency);

        // correct gaps in tiles.
        // gaps still show in low frequency
        // TODO: fix ??
        if (frequency == 1f)
        {
            cellScale = 1.14f;
        }
        else if (frequency == 2f)
        {
            cellScale = 1.04f;
        }
        else if (frequency == 3f)
        {
            cellScale = 1.01f;
        }
        else
        {
            cellScale = 1f;
        }


        GenerateSubdividedIcosahedron();
        BuildIndividualTiles(transform);
    }

    void GenerateSubdividedIcosahedron()
    {
        primalVertices = new List<Vector3>();
        primalTriangles = new List<int>();

        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        // Base 12 vertices of an icosahedron
        AddPrimalVertex(new Vector3(-1, t, 0));
        AddPrimalVertex(new Vector3(1, t, 0));
        AddPrimalVertex(new Vector3(-1, -t, 0));
        AddPrimalVertex(new Vector3(1, -t, 0));
        AddPrimalVertex(new Vector3(0, -1, t));
        AddPrimalVertex(new Vector3(0, 1, t));
        AddPrimalVertex(new Vector3(0, -1, -t));
        AddPrimalVertex(new Vector3(0, 1, -t));
        AddPrimalVertex(new Vector3(t, 0, -1));
        AddPrimalVertex(new Vector3(t, 0, 1));
        AddPrimalVertex(new Vector3(-t, 0, -1));
        AddPrimalVertex(new Vector3(-t, 0, 1));

        primalTriangles.AddRange(new int[] {
            0, 11, 5,  0, 5, 1,   0, 1, 7,   0, 7, 10,  0, 10, 11,
            1, 5, 9,   5, 11, 4,  11, 10, 2, 10, 7, 6,  7, 1, 8,
            3, 9, 4,   3, 4, 2,   3, 2, 6,   3, 6, 8,   3, 8, 9,
            4, 9, 5,   2, 4, 11,  6, 2, 10,  8, 6, 7,   9, 8, 1
        });

        for (int i = 0; i < frequency; i++)
        {
            SubdividePrimal();
        }

        // Project everything uniformly to the sphere surface
        for (int i = 0; i < primalVertices.Count; i++)
        {
            primalVertices[i] = primalVertices[i].normalized * radius;
        }
    }

    void SubdividePrimal()
    {
        List<int> newTriangles = new List<int>();
        Dictionary<long, int> midPointCache = new Dictionary<long, int>();

        for (int i = 0; i < primalTriangles.Count; i += 3)
        {
            int v1 = primalTriangles[i];
            int v2 = primalTriangles[i + 1];
            int v3 = primalTriangles[i + 2];

            int a = GetMidPoint(v1, v2, ref midPointCache);
            int b = GetMidPoint(v2, v3, ref midPointCache);
            int c = GetMidPoint(v3, v1, ref midPointCache);

            newTriangles.AddRange(new int[] { v1, a, c });
            newTriangles.AddRange(new int[] { v2, b, a });
            newTriangles.AddRange(new int[] { v3, c, b });
            newTriangles.AddRange(new int[] { a, b, c });
        }
        primalTriangles = newTriangles;
    }

    int GetMidPoint(int p1, int p2, ref Dictionary<long, int> cache)
    {
        long smaller = Mathf.Min(p1, p2);
        long greater = Mathf.Max(p1, p2);
        long key = (smaller << 32) + greater;

        if (cache.TryGetValue(key, out int value)) return value;

        Vector3 middle = (primalVertices[p1] + primalVertices[p2]) / 2f;
        primalVertices.Add(middle);
        int i = primalVertices.Count - 1;
        cache.Add(key, i);
        return i;
    }

    void AddPrimalVertex(Vector3 v) { primalVertices.Add(v.normalized); }

    void BuildIndividualTiles(Transform transform)
    {
        // Map each vertex index to a list of triangles that share it
        Dictionary<int, List<int>> vertexToTriangles = new();
        for (int i = 0; i < primalTriangles.Count; i += 3)
        {
            for (int j = 0; j < 3; j++)
            {
                int v = primalTriangles[i + j];
                if (!vertexToTriangles.ContainsKey(v)) vertexToTriangles[v] = new List<int>();
                vertexToTriangles[v].Add(i);
            }
        }

        // Generate a flat cell for every vertex position
        foreach (var kvp in vertexToTriangles)
        {
            int centerVertexIndex = kvp.Key;
            List<int> sharedTriangleIndices = kvp.Value;

            Vector3 cellCenterPos = primalVertices[centerVertexIndex];
            Vector3 cellNormal = cellCenterPos.normalized;

            // Compute the un-ordered corners of the dual tile (centroids of shared triangles)
            List<Vector3> corners = new List<Vector3>();
            foreach (int triIndex in sharedTriangleIndices)
            {
                Vector3 c0 = primalVertices[primalTriangles[triIndex]];
                Vector3 c1 = primalVertices[primalTriangles[triIndex + 1]];
                Vector3 c2 = primalVertices[primalTriangles[triIndex + 2]];
                Vector3 centroid = (c0 + c1 + c2) / 3f;
                corners.Add(centroid);
            }

            // Order corners wrapping around the tile center to ensure proper triangle winding
            Vector3 forwardBasis = (Mathf.Abs(Vector3.Dot(cellNormal, Vector3.up)) > 0.9f) ? Vector3.forward : Vector3.up;
            Vector3 right = Vector3.Cross(cellNormal, forwardBasis).normalized;
            Vector3 up = Vector3.Cross(right, cellNormal).normalized;

            corners.Sort((a, b) =>
            {
                Vector3 dirA = a - cellCenterPos;
                Vector3 dirB = b - cellCenterPos;
                float angleA = Mathf.Atan2(Vector3.Dot(dirA, up), Vector3.Dot(dirA, right));
                float angleB = Mathf.Atan2(Vector3.Dot(dirB, up), Vector3.Dot(dirB, right));
                return angleB.CompareTo(angleA);
            });

            // Create individual tile mesh
            bool isPentagon = (corners.Count == 5);

            // Transform positions, normals, corners to match the given Transform
            Matrix4x4 transformation = transform.localToWorldMatrix;
            cellCenterPos = transformation.MultiplyPoint3x4(cellCenterPos);
            cellCenterPos = transformation.MultiplyPoint3x4(cellNormal);
            corners = corners.Select(c => transformation.MultiplyPoint3x4(c)).ToList();


            GameObject tile = CreateTileGameObject(cellCenterPos, cellNormal, corners, isPentagon);
            tiles.Add(tile);
        }
    }

    GameObject CreateTileGameObject(Vector3 center, Vector3 normal, List<Vector3> globalCorners, bool isPentagon)
    {
        GameObject tile = new GameObject(isPentagon ? "PentagonTile" : "HexagonTile");
        // tile.transform.SetParent(parent);

        // Position the tile center in the world
        tile.transform.position = center;
        tile.transform.rotation = Quaternion.LookRotation(normal);

        MeshFilter mf = tile.AddComponent<MeshFilter>();
        MeshRenderer mr = tile.AddComponent<MeshRenderer>();
        mr.material = isPentagon ? pentagonMaterial : hexagonMaterial;

        // Generate flat local vertices
        List<Vector3> localVertices = new List<Vector3> { Vector3.zero }; // Local Center
        List<int> localTriangles = new List<int>();

        for (int i = 0; i < globalCorners.Count; i++)
        {
            // Convert global corner positions to local tile coordinates
            Vector3 localCorner = tile.transform.InverseTransformPoint(globalCorners[i]);

            // Force flatness by flattening the local Z axis relative to its facing direction
            localCorner.z = 0f;

            // Apply tile scale down transformation for distinct spacing gaps
            localCorner *= cellScale;

            localVertices.Add(localCorner);
        }

        // Fan triangulation from the center point
        for (int i = 1; i <= globalCorners.Count; i++)
        {
            int next = (i == globalCorners.Count) ? 1 : i + 1;
            localTriangles.Add(0);
            localTriangles.Add(i);
            localTriangles.Add(next);
        }

        Mesh mesh = new Mesh
        {
            vertices = localVertices.ToArray(),
            triangles = localTriangles.ToArray()
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Add a MeshCollider to make the individual flat tiles raycastable/clickable
        tile.AddComponent<MeshCollider>().sharedMesh = mesh;
        mf.mesh = mesh;

        return tile;
    }
}