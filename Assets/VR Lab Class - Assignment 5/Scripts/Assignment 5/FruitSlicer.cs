using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;

public class FruitSlicer : MonoBehaviour
{
    public Material insideMaterial; // Material for the sliced part
    public float sliceForce = 2f;   // Force applied to sliced pieces
    public GameObject[] fruitSlices;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sword")) // Check if hit by sword
        {
            Debug.Log(other.name);
            if (fruitSlices is { Length: 2 })
            {
                foreach (var slice in fruitSlices)
                {
                    var newSlice = Instantiate(slice, gameObject.transform.position, gameObject.transform.rotation);
                    var netObject = newSlice.AddComponent<NetworkObject>();
                    var netTransform = newSlice.AddComponent<NetworkTransform>();
                    var rigidbody = newSlice.AddComponent<Rigidbody>();
                    var netRigidbody = newSlice.AddComponent<NetworkRigidbody>();
                    
                    netObject.Spawn();
                    //rigidbody.AddForce();
                }
            }

            Destroy(gameObject); // Destroy original object
        }
        
    }

    GameObject[] SliceObject(Plane plane)
    {
        Mesh originalMesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = originalMesh.vertices;
        int[] triangles = originalMesh.triangles;

        List<Vector3> leftVertices = new List<Vector3>();
        List<Vector3> rightVertices = new List<Vector3>();
        List<int> leftTriangles = new List<int>();
        List<int> rightTriangles = new List<int>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v1 = transform.TransformPoint(vertices[triangles[i]]);
            Vector3 v2 = transform.TransformPoint(vertices[triangles[i + 1]]);
            Vector3 v3 = transform.TransformPoint(vertices[triangles[i + 2]]);

            bool v1Left = plane.GetSide(v1);
            bool v2Left = plane.GetSide(v2);
            bool v3Left = plane.GetSide(v3);

            if (v1Left && v2Left && v3Left)
            {
                Debug.Log("left vertices");
                AddTriangle(leftVertices, leftTriangles, v1, v2, v3);
            }
            else if (!v1Left && !v2Left && !v3Left)
            {
                Debug.Log("right vertices");
                AddTriangle(rightVertices, rightTriangles, v1, v2, v3);
            }
            else
            {
                // Handle slicing when a triangle is split by the plane
                Debug.Log("left and right vertices");
            }
        }

        GameObject leftObject = CreateSlice(leftVertices, leftTriangles);
        GameObject rightObject = CreateSlice(rightVertices, rightTriangles);

        return new GameObject[] { leftObject, rightObject };
    }

    void AddTriangle(List<Vector3> vertices, List<int> triangles, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int baseIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(baseIndex);
        triangles.Add(baseIndex + 1);
        triangles.Add(baseIndex + 2);
    }

    GameObject CreateSlice(List<Vector3> vertices, List<int> triangles)
    {
        if (vertices.Count == 0) return null;

        GameObject slice = new GameObject("Slice")
        {
            transform =
            {
                position = transform.position,
                rotation = transform.rotation
            }
        };

        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };
        mesh.RecalculateNormals();

        slice.AddComponent<MeshFilter>().mesh = mesh;
        slice.AddComponent<MeshRenderer>().material = insideMaterial;
        
        var netObject = slice.AddComponent<NetworkObject>();
        slice.AddComponent<NetworkTransform>();
        
        netObject.Spawn();
       
        Debug.Log("slice created");

        return slice;
    }
}
