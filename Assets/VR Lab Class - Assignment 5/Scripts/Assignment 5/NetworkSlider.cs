using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NetworkSlider : NetworkBehaviour
{
    public Slider slider;
    private NetworkVariable<float> _netSliderValue = new NetworkVariable<float>(0f);

    private void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
        
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            slider.interactable = false;
        }
    }
    
    private void Awake()
    {
        _netSliderValue.OnValueChanged += (oldValue, newValue) =>
        {
            if (!IsServer)
            {
                slider.value = newValue;
            }
        };
    }

    public void OnSliderValueChanged(float value)
    {
        if (IsServer)
        {
            UpdateSliderValueRpc(value);
        }
    }

    [Rpc(SendTo.Server)]
    private void UpdateSliderValueRpc(float value)
    {
        _netSliderValue.Value = value;
    }
}
