using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class HandCollider : MonoBehaviour
{
    #region Member Variables

    public bool isColliding = false;
    public GameObject collidingObject = null;
    
    [HideInInspector]
    public Color originalColor;
    public Color hoverColor = Color.blue;

    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        if (!GetComponentInParent<NetworkObject>().IsOwner)
        {
            Destroy(this);
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isColliding)
        {
            collidingObject = other.gameObject;
            isColliding = true;
            
            // with suggestions from ChatGPT
            originalColor = collidingObject.GetComponent<MeshRenderer>().material.color;
            collidingObject.GetComponent<MeshRenderer>().material.color = hoverColor;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isColliding && other.gameObject == collidingObject)
        {
            // with suggestions from ChatGPT
            collidingObject.GetComponent<MeshRenderer>().material.color = originalColor;
            originalColor = new Color();
            
            collidingObject = null;
            isColliding = false;
        }
    }

    #endregion
}
