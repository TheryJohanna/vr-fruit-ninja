using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;

public class FruitSlicer : NetworkBehaviour
{
    public float sliceForce = 5f;   // Force applied to sliced pieces
    public GameObject fruitSlices;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sword")) // Check if hit by sword
        {
            Debug.Log(other.name);
            SliceFruitRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void SliceFruitRpc()
    {
        var newSlice = Instantiate(fruitSlices, gameObject.transform.position, gameObject.transform.rotation);
        var netObject = newSlice.GetComponent<NetworkObject>();
        netObject.Spawn();
        newSlice.SetLayerRecursively(8);
        foreach (Transform child in newSlice.transform)
        {
            var netChildObject = child.AddComponent<NetworkObject>();
            netChildObject.Spawn();
            child.gameObject.SetLayerRecursively(8);
            child.AddComponent<NetworkTransform>();
            var childRigidbody = child.gameObject.AddComponent<Rigidbody>();
            child.gameObject.AddComponent<NetworkRigidbody>();
            var childMeshCollider = child.gameObject.AddComponent<MeshCollider>();
            childMeshCollider.convex = true;
            
            childRigidbody.AddForce(new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, 0f) * sliceForce, ForceMode.Impulse);
        }
        //var rigidbody = newSlice.GetComponent<Rigidbody>();
        Destroy(gameObject); // Destroy original object
    }
    
}
