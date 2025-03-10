using Unity.Netcode;
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

    [Rpc(SendTo.Everyone)]
    void UpdateButtonPositionRpc(Vector3 newPosition)
    {
        button.transform.localPosition = newPosition;
    }
    
    [Rpc(SendTo.Server)]
    void OnTriggerEnterRpc(Collider other)
    {
        if (!isPressed && other.CompareTag("User"))
        {
            Vector3 newPosition = new Vector3(_buttonTransform.localPosition.x, _buttonTransform.localPosition.y - pressDistance, _buttonTransform.localPosition.z);
            UpdateButtonPositionRpc(newPosition);
            _pressingObject = other.gameObject;
            onPress.Invoke();
            isPressed = true;
        }
    }

    [Rpc(SendTo.Server)]
    void OnTriggerExitRpc(Collider other)
    {
        if (other.gameObject == _pressingObject)
        {
            Vector3 newPosition = new Vector3(_buttonTransform.localPosition.x, _buttonTransform.localPosition.y + pressDistance, _buttonTransform.localPosition.z);
            UpdateButtonPositionRpc(newPosition);
            onRelease.Invoke();
            isPressed = false;
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
