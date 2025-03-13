using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FloatingSwords : NetworkBehaviour
{
    private Rigidbody _rigidbody;
    private float _originalY;
    private NetworkVariable<Vector3> _startPosition = new NetworkVariable<Vector3>(
        Vector3.zero, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
        );
    
    private NetworkVariable<Vector3> _startScale = new NetworkVariable<Vector3>(
        Vector3.zero, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
        );
    private float _randomness;
    private NetworkVariable<bool> _isGrabbed = new NetworkVariable<bool>(
        false, 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );
    
    public float floatStrength = 0.05f;
    private XRGrabInteractable _grabInteractable;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _grabInteractable = GetComponent<XRGrabInteractable>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            //Debug.Log($"Spawning FloatingSwords at {transform.position} with scale: {transform.localScale}");
            _startPosition.Value = transform.position;
            _startScale.Value = transform.localScale;
            //Debug.Log($"net variables: {_startPosition.Value}, {_startScale.Value} ");
        }
        else
        { 
            //Debug.Log($"Client Spawning FloatingSwords at {transform.position} with scale: {transform.localScale}");
           transform.position = _startPosition.Value;
           transform.localScale = _startScale.Value;
           //Debug.Log($"client net variables: {_startPosition.Value}, {_startScale.Value} ");
        }
    }
    
    void Start()
    {
        _originalY = transform.position.y;
        _randomness = Random.Range(0f, Mathf.PI * 2f);
        
        _grabInteractable.selectEntered.AddListener(OnGrab);
        _grabInteractable.selectExited.AddListener(OnRelease);
    }

    void FixedUpdate()
    {
        if (!IsServer) return;
        
        if (!_isGrabbed.Value)
        {
            var floatOffset = Mathf.Sin(Time.time + _randomness) * floatStrength;
            _rigidbody.MovePosition(new Vector3(transform.position.x, _originalY + floatOffset,
                transform.position.z));
        }

    }

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
        _originalY = transform.position.y;
        _rigidbody.isKinematic = false;
    }
    
}
