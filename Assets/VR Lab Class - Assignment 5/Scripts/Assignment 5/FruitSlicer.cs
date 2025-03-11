using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;

public class FruitSlicer : NetworkBehaviour
{
    public float sliceForce = 5f;   // Force applied to sliced pieces
    public GameObject[] fruitSlices;

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
        if (fruitSlices.Length == 2)
        {
            foreach (var fruit in fruitSlices)
            {
                var newSlice = Instantiate(fruit, gameObject.transform.position, gameObject.transform.rotation);
                var netObject = newSlice.GetComponent<NetworkObject>();
                var rigidbody = newSlice.GetComponent<Rigidbody>();
                netObject.Spawn();
                rigidbody.AddForce(new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, 0f) * sliceForce, ForceMode.Impulse);
            }
        }
        Destroy(gameObject); // Destroy original object
    }
    
}
