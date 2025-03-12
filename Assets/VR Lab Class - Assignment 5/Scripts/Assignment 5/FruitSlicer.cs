using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine.Events;

public class FruitSlicer : NetworkBehaviour
{
    public float sliceForce = 5f;   // Force applied to sliced pieces
    public GameObject[] fruitSlices;
    

    //[HideInInspector] 
    public GameObject launcher;
    private ulong _launcherId;
    public NetworkVariable<ulong> netLauncherId = new NetworkVariable<ulong>(0);

    void Awake()
    {
        netLauncherId.OnValueChanged += UpdateLauncher;
    }

    public void SetLauncher(NetworkVariable<ulong> netId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netId.Value, out var l))
        {
            Debug.Log("Launcher set");
            launcher = l.gameObject;
        }
    }

    private void UpdateLauncher(ulong oldId, ulong newId)
    {
        if (IsServer)
        {
            netLauncherId.Value = newId;
        }
        else
        {
            _launcherId = netLauncherId.Value;
        }
        SetLauncher(netLauncherId);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sword")) // Check if hit by sword
        {
            SliceFruitRpc();
            launcher.GetComponent<FruitLauncher>().UpdateScoreSign(1);
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
                
                Destroy(newSlice, 10f);
            }
        }
        Destroy(gameObject); // Destroy original object
    }
    
}
