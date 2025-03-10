using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;

public class FruitSlicer : MonoBehaviour
{
    public Material insideMaterial; // Material for the sliced part
    public float sliceForce = 5f;   // Force applied to sliced pieces
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
                    newSlice.SetLayerRecursively(8);
                    var netObject = newSlice.AddComponent<NetworkObject>();
                    newSlice.AddComponent<NetworkTransform>();
                    var rigidbody = newSlice.AddComponent<Rigidbody>();
                    var meshCollider = newSlice.AddComponent<MeshCollider>();
                    meshCollider.convex = true;
                    newSlice.AddComponent<NetworkRigidbody>();
                    
                    netObject.Spawn();
                    rigidbody.AddForce(new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, 0f) * sliceForce, ForceMode.Impulse);
                }
            }
            Destroy(gameObject); // Destroy original object
        }
        
    }
    
}
