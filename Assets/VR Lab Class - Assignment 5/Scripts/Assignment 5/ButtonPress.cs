using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonPress : MonoBehaviour
{
    [Header("Button Settings")]
    public GameObject button;
    public float pressDistance;
    private Transform _buttonTransform;

    private GameObject _pressingObject;
    private bool isPressed = false;

    [Header("Events")] 
    public UnityEvent onPress;
    public UnityEvent onRelease;
    
    // Start is called before the first frame update
    void Start()
    {
        _buttonTransform = button.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isPressed)
        {
            Debug.Log(other.gameObject.name);
            button.transform.localPosition = new Vector3(_buttonTransform.localPosition.x, _buttonTransform.localPosition.y - pressDistance, _buttonTransform.localPosition.z);
            _pressingObject = other.gameObject;
            onPress.Invoke();
            isPressed = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == _pressingObject)
        {
            button.transform.localPosition = new Vector3(_buttonTransform.localPosition.x, _buttonTransform.localPosition.y + pressDistance, _buttonTransform.localPosition.z);
            onRelease.Invoke();
            isPressed = false;
        }
    }
}
