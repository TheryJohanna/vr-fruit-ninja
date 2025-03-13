using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NetworkSlider : NetworkBehaviour, ISelectHandler, IDeselectHandler
{
    public Slider slider;
    private float _sliderValue;
    private NetworkVariable<float> _netSliderValue = new NetworkVariable<float>(0f);
    private bool _isSelected = false;
    private NetworkVariable<bool> _netIsSelected = new(false);
    public bool IsSelected => _isSelected;

    private void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
        
        if (IsOwner)
        {
            slider.onValueChanged.AddListener(OnSliderChanged);
        }
    }

    private void Awake()
    {
        _netSliderValue.OnValueChanged += OnSliderValueChanged;
        slider.value = _netSliderValue.Value;
    }

    public void OnSelect(BaseEventData eventData)
    {
        _isSelected = GetComponent<ObjectAccessHandler>().RequestAccess();
        Debug.Log(GetComponent<NetworkObject>().OwnerClientId);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        GetComponent<ObjectAccessHandler>().Release();
        _isSelected = false;
    }

    private void OnSliderValueChanged(float oldValue, float newValue)
    {
        slider.value = newValue;
    }

    public void OnSliderChanged(float value)
    {
        if (IsServer && IsSelected)
        {
            _netSliderValue.Value = value;
        }
        else
        {
            UpdateSliderServerRpc(slider.value);
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void UpdateSliderServerRpc(float value)
    {
        _netSliderValue.Value = value;
        UpdateSliderClientRpc();
        Debug.Log("server value changed to " + value);
    }

    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    private void UpdateSliderClientRpc()
    {
        Debug.Log("previous slider value " + _sliderValue);
        //_sliderValue = _netSliderValue.Value;
        slider.value = _netSliderValue.Value;
        Debug.Log("new net slider value " + _sliderValue);
    }
}
