using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FloatingSwords : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private float _originalY;
    private float _randomness;
    private NetworkVariable<bool> _isGrabbed = new(false);
    public float floatStrength = 0.05f;
    private XRGrabInteractable _grabInteractable;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _grabInteractable = GetComponent<XRGrabInteractable>();
    }
    
    void Start()
    {
        _originalY = _rigidbody.position.y;
        _randomness = Random.Range(0f, Mathf.PI * 2f);
        
        _grabInteractable.selectEntered.AddListener(OnGrab);
    }

    void FixedUpdate()
    {
        if (!_isGrabbed.Value)
        {
            float floatOffset = Mathf.Sin(Time.time + _randomness) * floatStrength;
            _rigidbody.MovePosition(new Vector3(_rigidbody.position.x, _originalY + floatOffset,
                _rigidbody.position.z));
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        RequestGrabServerRpc();
    }
    
    [Rpc(SendTo.Server)] 
    private void RequestGrabServerRpc()
    {
        _isGrabbed.Value = true;
    }
}
