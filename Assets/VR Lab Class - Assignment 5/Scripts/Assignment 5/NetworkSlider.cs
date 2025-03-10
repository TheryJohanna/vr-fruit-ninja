using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkSlider : MonoBehaviour
{
    public Slider slider; 
    private NetworkVariable<float> _sliderValue = new NetworkVariable<float>(0.5f);

    private void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
        
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        
        _sliderValue.OnValueChanged += (oldValue, newValue) =>
        {
            slider.value = newValue;
        };
        
        slider.value = _sliderValue.Value;
    }

    private void OnSliderValueChanged(float value)
    {
        UpdateSliderRpc(value);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void UpdateSliderRpc(float value)
    {
        _sliderValue.Value = value; 
    }
}
