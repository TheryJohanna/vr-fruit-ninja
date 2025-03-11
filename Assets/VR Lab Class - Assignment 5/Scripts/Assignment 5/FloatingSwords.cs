using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FloatingSwords : NetworkBehaviour
{
    private Rigidbody _rigidbody;
    private float _originalY;
    private float _randomness;
    private NetworkVariable<bool> _isGrabbed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
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
        _grabInteractable.selectExited.AddListener(OnRelease);
    }

    void FixedUpdate()
    {
        if (IsServer)
        {
            if (!_isGrabbed.Value)
            {
                float floatOffset = Mathf.Sin(Time.time + _randomness) * floatStrength;
                _rigidbody.MovePosition(new Vector3(_rigidbody.position.x, _originalY + floatOffset,
                    _rigidbody.position.z));
            }
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        RequestGrabRpc(NetworkManager.LocalClientId);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (IsOwner)
        {
            RequestReleaseRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestGrabRpc(ulong clientId)
    {
        _isGrabbed.Value = true;
        GetComponent<NetworkObject>().ChangeOwnership(clientId);
    }

    [Rpc(SendTo.Server)]
    private void RequestReleaseRpc()
    {
        _isGrabbed.Value = false;
        GetComponent<NetworkObject>().RemoveOwnership();
        _originalY = _rigidbody.position.y;
    }
}
