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
    public GameObject[] fruitsSmall;
    public GameObject[] fruitsLarge;
    
    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public float coneAngle = 30f;
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
            var randomDirection = GetRandomDirectionInCone(spawnPoint.forward, spawnPoint.up, coneAngle);
            fruitRigidbody.AddForce(randomDirection * Random.Range(5.5f, 7f), ForceMode.Impulse);
        }
    }
    
    // Code from ChatGPT
    Vector3 GetRandomDirectionInCone(Vector3 forward, Vector3 up, float coneAngle)
    {
        float angleInRadians = coneAngle * Mathf.Deg2Rad;

        // Generate a random rotation within the cone
        Quaternion randomRotation = Quaternion.AngleAxis(Random.Range(-coneAngle, coneAngle), Vector3.up) *
                                    Quaternion.AngleAxis(Random.Range(-coneAngle, coneAngle), Vector3.right);

        return randomRotation * (forward + up);
    }
    #endregion
}
