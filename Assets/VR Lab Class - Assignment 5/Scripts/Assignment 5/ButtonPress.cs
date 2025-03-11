using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ButtonPress : NetworkBehaviour
{
    [Header("Button Settings")]
    public GameObject button;
    public float pressDistance;
    private float _buttonTransformY;

    private GameObject _pressingObject;
    private bool _isPressed = false;

    [Header("Events")] 
    public UnityEvent onPress;
    public UnityEvent onRelease;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _buttonTransformY = button.transform.localPosition.y;
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateButtonPositionRpc(float newPosition)
    {
        button.transform.localPosition = new Vector3(button.transform.localPosition.x, newPosition, button.transform.localPosition.z);
    }
    
    [Rpc(SendTo.Server)]
    void OnTriggerEnterRpc(Collider other)
    {
        if (!_isPressed && other.CompareTag("User"))
        {
            float newPosition = _buttonTransformY - pressDistance;
            UpdateButtonPositionRpc(newPosition);
            _pressingObject = other.gameObject;
            onPress.Invoke();
            _isPressed = true;
        }
    }

    [Rpc(SendTo.Server)]
    void OnTriggerExitRpc(Collider other)
    {
        if (other.gameObject == _pressingObject)
        {
            float newPosition = _buttonTransformY;
            UpdateButtonPositionRpc(newPosition);
            onRelease.Invoke();
            _isPressed = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterRpc(other);
    }

    
    void OnTriggerExit(Collider other)
    {
       OnTriggerExitRpc(other);
    }
}
