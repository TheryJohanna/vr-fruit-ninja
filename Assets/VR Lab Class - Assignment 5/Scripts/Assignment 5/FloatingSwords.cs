using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FloatingSwords : NetworkBehaviour
{
    private Rigidbody _rigidbody;
    private float _originalY;
    private NetworkVariable<Vector3> _startPosition = new NetworkVariable<Vector3>(
        Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float _randomness;
    private NetworkVariable<bool> _isGrabbed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    public float floatStrength = 0.05f;
    private XRGrabInteractable _grabInteractable;
    private NetworkTransform _networkTransform;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _grabInteractable = GetComponent<XRGrabInteractable>();
        _networkTransform = GetComponent<NetworkTransform>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _startPosition.Value = transform.position;
        }
        else
        {
            Debug.Log(transform.position);
            transform.position = _startPosition.Value;
            Debug.Log(transform.position);
        }
    }
    
    void Start()
    {
        _originalY = _rigidbody.position.y;
        _randomness = Random.Range(0f, Mathf.PI * 2f);
        
        _grabInteractable.selectEntered.AddListener(OnGrab);
        _grabInteractable.selectExited.AddListener(OnRelease);
    }

    /*void FixedUpdate()
    {
        if (!IsServer) return;
        
        if (!_isGrabbed.Value)
        {
            float floatOffset = Mathf.Sin(Time.time + _randomness) * floatStrength;
            _rigidbody.MovePosition(new Vector3(_rigidbody.position.x, _originalY + floatOffset,
                _rigidbody.position.z));
        }

    }*/

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (!_isGrabbed.Value)
        {
            RequestGrabRpc(NetworkManager.LocalClientId);
        }
        
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
        
        _rigidbody.isKinematic = true;
    }

    [Rpc(SendTo.Server)]
    private void RequestReleaseRpc()
    {
        _isGrabbed.Value = false;
        GetComponent<NetworkObject>().RemoveOwnership();
        _originalY = _rigidbody.position.y;
        
        _rigidbody.isKinematic = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }
}
