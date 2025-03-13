using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.VisualScripting;
using TMPro;

public class FruitLauncher : NetworkBehaviour
{
    #region Member Variables
    [Header("Prefab Settings")]
    public GameObject[] fruits;
    public GameObject[] fruitsSmall;
    public GameObject[] fruitsLarge;
    public GameObject[] fruitsDebug;
    public static FruitLauncher Instance;
    private GameObject[] _currentList;
    
    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public float coneAngle = 30f;
    public GameObject scoreSign;
    private int _score = 0;
    private NetworkVariable<int> _netScore = new NetworkVariable<int>();
    private ulong _launcherId;
    
    
    [Header("Physics Settings")]
    public float launchForce = 6f;

    [Header("Settings Setup")] 
    public TMP_Dropdown dropdown;
    
    #endregion

    #region Event Functions

    // Start is called before the first frame update
    void Start()
    {
        // get dropdown value at start of lobby
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        GetListFromDropdown(dropdown.options[0].text);
        
        // get score at start of lobby
        var startScore = scoreSign.transform.Find("Score");
        _score = Int32.Parse(scoreSign.transform.Find("Score").GetComponent<TextMeshPro>().text);
    }

    private void Awake()
    {
        Instance = this;
        // network update listener
        _netScore.OnValueChanged += UpdateScore;
    }

    #endregion
    
    #region Helper Functions
    
    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void SpawnAndLaunchRpc()
    {
        if (_currentList.Length == 0 || spawnPoint.IsUnityNull())
            return;
        
        var currentFruit =  _currentList[UnityEngine.Random.Range(0, _currentList.Length)];
        var spawnedFruit = Instantiate(currentFruit, spawnPoint.position, Quaternion.identity);
        spawnedFruit.GetComponent<NetworkObject>().Spawn();
        var fruitSlicer = spawnedFruit.GetComponent<FruitSlicer>();
        if (!fruitSlicer.IsUnityNull())
        {
            fruitSlicer.netLauncherId.Value = gameObject.GetComponent<NetworkObject>().NetworkObjectId;
        }
        
        var fruitRigidbody = spawnedFruit.GetComponent<Rigidbody>();
        if (!fruitRigidbody.IsUnityNull())
        {
            var randomDirection = GetRandomDirectionInCone(spawnPoint.forward, spawnPoint.up, coneAngle);
            fruitRigidbody.AddForce(randomDirection * UnityEngine.Random.Range(5.5f, 6f), ForceMode.Impulse);
        }
        Destroy(spawnedFruit, 10f);
    }
    
    private void UpdateScore(int previousScore, int newScore)
    {
        Debug.Log($"updated score: {previousScore} -> {newScore}");
        _score = newScore;
        scoreSign.transform.Find("Score").GetComponent<TextMeshPro>().text = _netScore.Value.ToString();
        Debug.Log("updated sign");
    }

    public void ResetScoreSign()
    {
        if (IsServer)
        {
            _netScore.Value = 0;
            Debug.Log($"Score on server: {_netScore.Value}");
        }
        else
        {
            _score = _netScore.Value;
            Debug.Log($"Score on client: {_score}");
        }
    }
    public void UpdateScoreSign(int score)
    {
        if (IsServer)
        {
            _netScore.Value = _score + score;
            Debug.Log($"Score on server: {_netScore.Value}");
        }
        else
        {
            _score = _netScore.Value;
            Debug.Log($"Score on client: {_score}");
        }
    }
    
    // Code from ChatGPT
    Vector3 GetRandomDirectionInCone(Vector3 forward, Vector3 up, float coneAngle)
    {
        float angleInRadians = coneAngle * Mathf.Deg2Rad;

        // Generate a random rotation within the cone
        Quaternion randomRotation = Quaternion.AngleAxis(UnityEngine.Random.Range(-coneAngle, coneAngle), Vector3.up) *
                                    Quaternion.AngleAxis(UnityEngine.Random.Range(-coneAngle, coneAngle), Vector3.right);

        return randomRotation * (forward + up);
    }

    private void OnDropdownValueChanged(int index)
    {
        GetListFromDropdown(dropdown.options[index].text);
    }
    
    public void GetListFromDropdown(string dropdownValue)
    {
        _currentList = dropdownValue switch
        {
            "Big" => fruitsLarge,
            "Small" => fruitsSmall,
            "Mixed" => fruits,
            "Debug" => fruitsDebug,
            _ => fruits
        };
    }
    #endregion
}
