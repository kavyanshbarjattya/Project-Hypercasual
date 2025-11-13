using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralTunnelMesh : MonoBehaviour
{
    [Header("References")]
    public SplineContainer splineContainer; // Drag your SplineContainer here

    [Header("Tunnel Geometry")]
    [Tooltip("The 'roundness' of the tunnel. 3 = triangle, 8 = octagon, 24 = very circular.")]
    [Range(3, 32)]
    public int tunnelSides = 8; // This is the control you asked for

    [Tooltip("The radius or 'size' of the tunnel.")]
    public float tunnelRadius = 5f;

    [Tooltip("How many 'rings' to build per meter. Higher = smoother curves.")]
    [Range(0.1f, 10f)]
    public float segmentsPerMeter = 1f;

    // Mesh data lists
    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<int> triangles = new List<int>();
    private readonly List<Vector2> uvs = new List<Vector2>();
    private readonly List<Vector3> normals = new List<Vector3>();

    private MeshFilter meshFilter;
    private Mesh mesh; // <-- We'll reuse the mesh object

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();

        // Create a new mesh object to work with
        mesh = new Mesh();
        mesh.name = "ProceduralTunnel";
        meshFilter.mesh = mesh;

        if (splineContainer == null)
        {
            splineContainer = GetComponent<SplineContainer>();
        }
    }

    /// <summary>
    /// We run this in LateUpdate to make sure the mesh is built
    /// AFTER the spline path has been updated in Update().
    /// </summary>
    void LateUpdate()
    {
        if (splineContainer == null || splineContainer.Spline == null)
            return;

        BuildTunnelMesh();
    }

    /// <summary>
    /// Clears the old mesh data and rebuilds it based on the spline's current path.
    /// </summary>
    void BuildTunnelMesh()
    {
        // 1. Clear old data from last frame
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        normals.Clear();

        float splineLength = splineContainer.Spline.GetLength();
        int totalSegments = Mathf.RoundToInt(splineLength * segmentsPerMeter);

        // Make sure we have at least 2 segments to build a mesh
        if (totalSegments < 2)
            return;

        // --- 2. Ring Generation Loop ---
        for (int i = 0; i <= totalSegments; i++)
        {
            // Get the position on the spline (0 to 1)
            float t = i / (float)totalSegments;

            // Get the spline's data (center, forward, up)
            splineContainer.Spline.Evaluate(t,
                out float3 center,
                out float3 tangent,
                out float3 up);

            // Get the "right" vector
            float3 right = math.normalize(math.cross(tangent, up));

            // --- Generate Vertices and Normals for this one Ring ---
            for (int j = 0; j < tunnelSides; j++)
            {
                // Get angle for this vertex (in radians)
                float angle = (j / (float)tunnelSides) * 2 * Mathf.PI;

                // Get the 2D offset
                float xOffset = Mathf.Cos(angle) * tunnelRadius;
                float yOffset = Mathf.Sin(angle) * tunnelRadius;

                // Apply 3D orientation to get vertex position
                float3 vertexPos = center + (right * xOffset) + (up * yOffset);
                vertices.Add(vertexPos);

                // Calculate the smooth normal (points from center to vertex)
                float3 normal = math.normalize(vertexPos - center);
                normals.Add(normal);

                // Add UVs (U wraps around tunnel, V goes along length)
                uvs.Add(new Vector2(j / (float)tunnelSides, t));
            }

            // --- 3. Connect this ring to the previous one ---
            if (i > 0)
            {
                // Start index of the *previous* ring's vertices
                int baseIndex = (i - 1) * tunnelSides;

                for (int j = 0; j < tunnelSides; j++)
                {
                    // Get the 4 vertex indices that form this quad
                    int p1 = baseIndex + j;
                    int p2 = baseIndex + ((j + 1) % tunnelSides); // Modulo wraps around

                    int c1 = baseIndex + tunnelSides + j;
                    int c2 = baseIndex + tunnelSides + ((j + 1) % tunnelSides);

                    // Triangle 1 (p1, c1, c2)
                    triangles.Add(p1);
                    triangles.Add(c1);
                    triangles.Add(c2);

                    // Triangle 2 (p1, c2, p2)
                    triangles.Add(p1);
                    triangles.Add(c2);
                    triangles.Add(p2);
                }
            }
        }

        // --- 4. Assign all data to the Mesh ---
        mesh.Clear(); // Clear previous frame's geometry
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray(); // Use our custom smooth normals
        mesh.RecalculateBounds(); // Good for performance
    }
}