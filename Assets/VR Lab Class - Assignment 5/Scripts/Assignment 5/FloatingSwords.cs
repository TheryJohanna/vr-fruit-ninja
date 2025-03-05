using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingSwords : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private float _originalY;
    private float _randomness;
    public float floatStrength = 0.1f; 

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _originalY = _rigidbody.position.y;
        _randomness = Random.Range(0f, Mathf.PI * 2f);
    }

    void FixedUpdate()
    {
        float floatOffset = Mathf.Sin(Time.time + _randomness) * floatStrength;
        _rigidbody.MovePosition(new Vector3(_rigidbody.position.x, _originalY + floatOffset, _rigidbody.position.z));
    }
}
