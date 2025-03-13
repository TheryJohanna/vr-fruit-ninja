using TMPro;
using Unity.Netcode;
using UnityEngine;


public class NetworkDropdown : NetworkBehaviour
{
    public TMP_Dropdown dropdown;
    private NetworkVariable<int> _netIndex = new NetworkVariable<int>(0);
    
    void Start()
    {
        if (dropdown == null)
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            dropdown.interactable = false;
        }
    }

    private void Awake()
    {
        _netIndex.OnValueChanged += (oldValue, newValue) =>
        {
            if (!IsServer)
            {
                dropdown.value = newValue;
            }
        };
    }

    public void OnDropdownValueChanged(int index)
    {
        if (IsServer)
        {
            UpdateDropdownSelectionRpc(index);
        }
    }

    [Rpc(SendTo.Server)]
    private void UpdateDropdownSelectionRpc(int index)
    {
        _netIndex.Value = index;
    }
    
}
