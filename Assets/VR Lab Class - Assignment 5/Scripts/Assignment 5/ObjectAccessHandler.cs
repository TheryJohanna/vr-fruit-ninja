using Unity.Netcode;

public class ObjectAccessHandler : NetworkBehaviour
{
    #region Member Variables

    private NetworkVariable<bool> _isGrabbed = new();

    #endregion

    #region Custom Methods

    public bool RequestAccess()
    {
        if (!_isGrabbed.Value)
        {
            GrabObjectRpc(NetworkManager.LocalClientId);
            return true;
        }
        
        return false;
    }

    public void Release()
    {
        if (IsOwner)
        {
            ReleaseObjectRpc();
            _isGrabbed.Value = false;
        }
            
    }

    #endregion

    #region RPCs

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void GrabObjectRpc(ulong clientId)
    {
        _isGrabbed.Value = true;
        GetComponent<NetworkObject>().ChangeOwnership(clientId);
    }

    [Rpc(SendTo.Everyone)]
    private void ReleaseObjectRpc()
    {
        GetComponent<NetworkObject>().RemoveOwnership();
    }

    #endregion
}
