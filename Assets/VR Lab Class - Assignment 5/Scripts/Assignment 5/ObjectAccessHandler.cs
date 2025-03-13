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
            ReleaseObjectRpc();
    }

    #endregion

    #region RPCs

    [Rpc(SendTo.Server)]
    private void GrabObjectRpc(ulong clientId)
    {
        _isGrabbed.Value = true;
        GetComponent<NetworkObject>().ChangeOwnership(clientId);
    }

    [Rpc(SendTo.Server)]
    private void ReleaseObjectRpc()
    {
        _isGrabbed.Value = false;
        GetComponent<NetworkObject>().RemoveOwnership();
    }

    #endregion
}