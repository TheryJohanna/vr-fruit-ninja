using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FloatingSwords : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private float _originalY;
    private float _randomness;
    private bool _isGrabbed = false;
    public float floatStrength = 0.05f;
    private XRGrabInteractable _grabInteractable;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _originalY = _rigidbody.position.y;
        _randomness = Random.Range(0f, Mathf.PI * 2f);
        
        _grabInteractable = GetComponent<XRGrabInteractable>();
        _grabInteractable.onSelectEntered.AddListener(OnGrab);
    }

    void FixedUpdate()
    {
        if (!_isGrabbed)
        {
            float floatOffset = Mathf.Sin(Time.time + _randomness) * floatStrength;
            _rigidbody.MovePosition(new Vector3(_rigidbody.position.x, _originalY + floatOffset,
                _rigidbody.position.z));
        }
    }
    
    private void OnGrab(XRBaseInteractor interactor)
    {
        _isGrabbed = true;
    }
}
