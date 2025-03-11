using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ButtonPress : NetworkBehaviour
{
    [Header("Button Settings")] public GameObject button;
    public float pressDistance;
    private float _yPos;
    private NetworkVariable<float> _networkedYPos;
    private float _baselineY;

    private GameObject _pressingObject;
    private bool _isPressed = false;

    [Header("Events")] public UnityEvent onPress;
    public UnityEvent onRelease;


    // Start is called before the first frame update
    void Start()
    {
        _baselineY = button.transform.localPosition.y;
        _networkedYPos.OnValueChanged += UpdateButtonPosition;
    }

    void UpdateButtonPosition(float previousPosition, float newPosition)
    {
        _yPos = newPosition;
        button.transform.localPosition = new Vector3(button.transform.localPosition.x, _yPos, button.transform.localPosition.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_isPressed && other.CompareTag("User"))
        {
            if (IsOwner)
            {
                _networkedYPos.Value = _baselineY - pressDistance;
                _pressingObject = other.gameObject;
                onPress.Invoke();
                _isPressed = true;
            }
            else
            {
                _yPos = _networkedYPos.Value;
            }
            
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == _pressingObject)
        {
            if (IsOwner)
            {
                _networkedYPos.Value = _baselineY;
                onRelease.Invoke();
                _isPressed = false;
            }
            else
            {
                _yPos = _networkedYPos.Value;
            }

            
        }
    }
}
