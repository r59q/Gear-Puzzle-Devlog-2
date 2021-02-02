using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Put this on an empty game object.
/// </summary>
public class GearMesh : MonoBehaviour
{
    // Variables
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

     /// <summary>
    /// Used for generating/regenerating gear
    /// </summary>
    public void GenerateGear()
    {
        // Add components
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // Generate mesh
        Mesh mesh = GenerateMesh(); // Vertices
        meshFilter.mesh = mesh;
        meshRenderer.material = Material;
    }
    
    /// <summary>
    /// Creates the actual mesh of the gear.
    /// </summary>
    /// <returns>A mesh of a gear.</returns>
    private Mesh GenerateMesh()
    {
        // Generate vertices
        Vector3[] topFanVerts = TriangleFanVerts(Thickness / 2f);
        Vector3[] bottomFanVerts = TriangleFanVerts(-(Thickness / 2f));

        // Calculate tris
        int[] topFanTris = TriangleFanTris();
        int[] bottomFanTris = TriangleFanTris(); // We use the same algorithm to calculate tri indices. We simply offset by the length of topFanTris
        int[] cylindricalTris = CylindricalTris();
        for (int i = 0; i < bottomFanTris.Length; i++)
        {
            bottomFanTris[i] += topFanVerts.Length;
        }
        // Now we need to flip the direction of the tris on the bottom triangle fan.
        for (int i = 0; i < bottomFanTris.Length; i += 3)
        {
            int tmp = bottomFanTris[i + 2];
            bottomFanTris[i + 2] = bottomFanTris[i]; // swap the first and last index to flip the face.
            bottomFanTris[i] = tmp;
        }

        // Normals
        Vector3 topFanNormal = Vector3.forward;
        Vector3 bottomFanNormal = -Vector3.forward;

        // Initialize final arrays
        Vector3[] verts = new Vector3[topFanVerts.Length * 2];
        // Vector3[] norms = new Vector3[verts.Length];
        Vector2[] uvs = new Vector2[verts.Length];
        int[] tris = new int[topFanTris.Length * 2 + cylindricalTris.Length];
        // Copy vertices
        topFanVerts.CopyTo(verts, 0);
        bottomFanVerts.CopyTo(verts, topFanVerts.Length);

        // Copy tris
        topFanTris.CopyTo(tris, 0);
        bottomFanTris.CopyTo(tris, topFanTris.Length);
        cylindricalTris.CopyTo(tris, topFanTris.Length * 2);

        // Get vertex pairs
        VertexPair[] pairs = SideVertPairs();

        for (int i = 0; i < TeethCount; i+=1)
        {
            for (int k = 0; k < (Resolution/2); k++)
            {
                VertexPair pair = pairs[i * Resolution + k];
                Vector3 direction = RemoveZ(verts[pair.X]).normalized;
                verts[pair.X] += direction * ToothDepth;
                verts[pair.Y] += direction * ToothDepth;
            }
        }

        // Setup mesh
        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        UnityEditor.Unwrapping.GenerateSecondaryUVSet(mesh);
        mesh.RecalculateNormals();

        // Display some stats.
        // Debug.Log("--- Mesh stats ---");
        // Debug.Log("VertCount : " + mesh.vertices.Length);
        // Debug.Log("TriCount : " + mesh.triangles.Length);

        return mesh;
    }

    /// <summary>
    /// Calculates the indeces of vertices, that makes up a triangle fan
    /// </summary>
    /// <returns>Indeces of triangle fan.</returns>
    private int[] TriangleFanTris()
    {
        // Calculate how many vertices makes up a triangle fan.
        int vertCount = (TeethCount * Resolution) + 1; 
        int[] tris = new int[(vertCount - 1) * 3]; // Assign how many indeces we need to create the edges between them.


        for (int i = 0; i < vertCount - 1; i++)
        {
            int v1, v2, v3;

            v1 = 0;
            v2 = (i + 1) % vertCount;
            v3 = (i + 2) % vertCount;

            // We want to skip index 0, since that's the center of the fan.
            if (v3 == 0)
            {
                v3 = 1;
            }

            tris[(i * 3)] = v1;
            tris[(i * 3) + 1] = v2;
            tris[(i * 3) + 2] = v3;
        }

        return tris;
    }

    /// <summary>
    /// Pairs each triangle fan vertex with the equivalent on the opposite side.
    /// </summary>
    /// <returns>The pairs of vertices that makes up the two triangle fans.</returns>
    private VertexPair[] SideVertPairs()
    {
        int vertCount = Resolution * TeethCount;
        List<VertexPair> pairs = new List<VertexPair>();

        for (int i = 0; i < vertCount; i++)
        {
            int x = 1 + i;
            int y = 2 + Resolution * TeethCount + i;
            pairs.Add(new VertexPair(x, y));
        }

        return pairs.ToArray();
    }

    /// <summary>
    /// Calculate the triangles, that makes up the side of the cylinder.
    /// </summary>
    /// <returns>The indeces of vertices that makes up the triangles of the side.</returns>
    private int[] CylindricalTris()
    {
        int[] tris = new int[(TeethCount * Resolution) * 6]; // Theres teethCount * resolution squares, with 2 triangled which need 3 indeces each. That is (2 * 3 * teethCount * resolution) indeces.
        int fanVerts = TeethCount * Resolution + 1; // The amount of vertices that make up a triangle fan.
        List<int> indeces = new List<int>();

        for (int i = 0; i < TeethCount * Resolution; i++)
        {
            if (i == TeethCount * Resolution - 1) // stitch the first and last vertices together.
            {
                indeces.AddRange(QuadToTri(i + 1, 1, fanVerts + i + 1, fanVerts + 1));
            } else
            {
                indeces.AddRange(QuadToTri(i + 1, i + 2, fanVerts + i + 1, fanVerts + i + 2));
            }
        }

        return indeces.ToArray();
    }

    /// <summary>
    /// Converts a quad to two triangles.
    /// </summary>
    /// <param name="v1">Top left vertex</param>
    /// <param name="v2">Top right vertex</param>
    /// <param name="v3">Bottom left vertex</param>
    /// <param name="v4">Bottom right vertex</param>
    /// <returns>An array of indeces, which represents the quad as triangles.</returns>
    private int[] QuadToTri(int v1, int v2, int v3, int v4)
    {
        return new int[] { v1, v3, v4, v1, v4, v2 };
    }

    /// <summary>
    /// Generates, using the unit circle, the vertices that makes up a triangle fan.
    /// </summary>
    /// <param name="depth">The width of the 'inner' profile of the gear.</param>
    /// <returns>An array of vertices.</returns>
    private Vector3[] TriangleFanVerts(float depth)
    {
        int vertCount = (TeethCount * Resolution) + 1; // Resolution * toothCount plus 1 for center
        Vector3[] verts = new Vector3[vertCount];
        verts[0] = Vector3.forward * depth; // Center cap

        float angleStep = 360f / (vertCount - 1);

        for (int i = 0; i < vertCount - 1; i++)
        {
            Vector3 angledVector = AngledVector(angleStep * i);
            angledVector = angledVector * RootDiameter; // Since the anged vector has a magnitude of 1 we can multiply it with the root diameter of the gear.
            angledVector.z = depth;
            verts[i + 1] = angledVector;
        }
        return verts;
    }

    /// <summary>
    /// The sin-cosine vector of the unity circle
    /// </summary>
    /// <param name="degreeAngle">The angle in degrees.</param>
    /// <returns>The angled vector, with length 1.</returns>
    private Vector2 AngledVector(float degreeAngle)
    {
        float radAngle = Mathf.Deg2Rad * degreeAngle;
        float sin = Mathf.Sin(radAngle);
        float cos = Mathf.Cos(radAngle);
        return new Vector2(cos, sin);
    }

    /// <summary>
    /// Removes the z component of a vector.
    /// </summary>
    /// <param name="vector">Vector to remove the z componen of.</param>
    /// <returns>The same vector, but vector.z = 0</returns>
    private Vector3 RemoveZ(Vector3 vector)
    {
        return new Vector3(vector.x, vector.y, 0);
    }

    public float Module
    {
        get;set;
    }

    public int TeethCount
    {
        get;set;
    }

    public int Resolution
    {
        get;set;
    }

    public float Thickness
    {
        get;set;
    }

    public float ReferenceDiameter                  // d
    {
        get { return Module * TeethCount; }
    }

    public float TipDiameter
    {
        get { return (2 * Module) + ReferenceDiameter; }
    }

    public float RootDiameter
    {
        get { return ReferenceDiameter - (2.5f * Module); }
    }

    public float ToothDepth                 // h. new value based on KHKGears calculations. https://khkgears.net/new/gear_knowledge/gear_technical_reference/calculation_gear_dimensions.html
    {
        get { return 2.25f * Module; }
    }

    public Material Material
    {
        get;set;
    }

    public struct VertexPair
    {
        public VertexPair(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

    }
}




/*
   // Offset vertex pairs.
   for (int i = 0; i < pairs.Length; i += 2*resolution)
   {
       // Get pair indices
       int v1Index = pairs[i];
       int v2Index = pairs[i + 1];

       // Calculate outwards direction.
       Vector3 v1 = verts[v1Index];
       Vector3 v2 = verts[v2Index];
       Vector3 v1Direction = v1 - verts[0];
       Vector3 v2Direction = v2 - verts[teethCount * resolution + 1];

       Debug.Log("v1Direction = " + v1Direction + " pair: " + v1Index + " & " + v2Index);
       Debug.Log("v2Direction = " + v2Direction);

       v1Direction = v1Direction.normalized;
       v2Direction = v2Direction.normalized;

       // Offset verts
       v1 += v1Direction;
       v2 += v2Direction;

       // Set verts
       verts[v1Index] = v1;
       verts[v2Index] = v2;
   } 
   */
