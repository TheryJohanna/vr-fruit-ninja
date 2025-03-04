using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.VisualScripting;

public class FruitLauncher : MonoBehaviour
{
    #region Member Variables
    [Header("Prefab Settings")]
    public GameObject[] fruits;
    
    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public Button spawnButton;
    
    [Header("Physics Settings")]
    public float launchForce = 100f;
    public Transform target;
    
    #endregion

    #region Event Functions

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(SpawnAndLaunch), 5f, 10f);
    }
    #endregion
    
    #region Helper Functions
    
    [ServerRpc]
    void SpawnAndLaunch()
    {
        if (fruits.Length == 0 || spawnPoint.IsUnityNull() || target.IsUnityNull())
            return;
        
        var currentFruit = fruits[Random.Range(0, fruits.Length)];
        var spawnedFruit = Instantiate(currentFruit, spawnPoint.position, Quaternion.identity);
        spawnedFruit.GetComponent<NetworkObject>().Spawn();
        
        var fruitRigidbody = spawnedFruit.GetComponent<Rigidbody>();
        if (!fruitRigidbody.IsUnityNull())
        {
            var randomDirection = new Vector3(Random.Range(-1f, 1f), 1, 1).normalized;
            fruitRigidbody.AddForce(randomDirection * Random.Range(5f, 10f), ForceMode.Impulse);
        }
    }
    #endregion
}
