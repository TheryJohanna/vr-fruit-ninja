using Unity.Netcode;

public class ObjectAccessHandler : NetworkBehaviour
{
    #region Member Variables

    private NetworkVariable<bool> isGrabbed = new();

    #endregion

    #region Custom Methods

    public bool RequestAccess()
    {
        if (!isGrabbed.Value)
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
            isGrabbed.Value = false;
        }
            
    }

    #endregion

    #region RPCs

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void GrabObjectRpc(ulong clientId)
    {
        isGrabbed.Value = true;
        GetComponent<NetworkObject>().ChangeOwnership(clientId);
    }

    [Rpc(SendTo.Everyone)]
    private void ReleaseObjectRpc()
    {
        GetComponent<NetworkObject>().RemoveOwnership();
    }

    #endregion
}
