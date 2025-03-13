using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkDropdown : NetworkBehaviour
{
    public TMP_Dropdown dropdown;
    private NetworkVariable<int> _netIndex = new NetworkVariable<int>(0);
    
    void Start()
    {
        if (!IsHost)
        {
            dropdown.interactable = false;
        }
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        Debug.Log("started");
        Debug.Log($"host: {IsHost}, client: {IsClient}, server: {IsServer}");
    }

    private void Awake()
    {
        _netIndex.OnValueChanged += (oldValue, newValue) =>
        {
            if (!IsServer)
            {
                //Debug.Log("i'm not a server, but changing value");
                dropdown.value = newValue;
                //Debug.Log("new value is: " + dropdown.value);
            }
        };
    }

    public void OnDropdownValueChanged(int index)
    {
        if (IsServer)
        {
            //Debug.Log("i'm a server, sending an rpc");
            UpdateDropdownSelectionRpc(index);
        }
    }

    [Rpc(SendTo.Server)]
    private void UpdateDropdownSelectionRpc(int index)
    {
        _netIndex.Value = index;
        //Debug.Log("i executed the rpc. net value is " + _netIndex.Value);
    }
    
}
