using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using VRSYS.Core.Avatar;

public class GoGo : MonoBehaviour
{
    #region Member Variables

    private Transform head;

    public Transform handVisual;
    public float originHeadOffset = 0.2f;
    public float distanceThreshold = .55f;
    public float k = .167f;

    public InputActionProperty grabAction;
    public HandCollider handCollider;

    private GameObject grabbedObject;
    private Matrix4x4 offsetMatrix;
    
    private bool canGrab
    {
        get
        {
            if (handCollider.isColliding)
                return handCollider.collidingObject.GetComponent<ObjectAccessHandler>().RequestAccess();
            return false;
        }
    }
    

    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        if (!GetComponentInParent<NetworkObject>().IsOwner)
        {
            Destroy(this);
            return;
        }

        head = GetComponentInParent<AvatarHMDAnatomy>().head;
    }

    private void Update()
    {
        ApplyGoGo();
        CalculationGrab();
    }

    #endregion

    #region Custom Methods

    private void ApplyGoGo()
    {
        Vector3 origin = head.position;
        origin.y -= originHeadOffset;

        float distance = Vector3.Distance(origin, transform.position);

        if (distance < distanceThreshold)
            handVisual.position = transform.position;
        else
        {
            Vector3 direction = transform.position - origin;

            float offsetDistance = k * Mathf.Pow((direction.magnitude - distanceThreshold) * 100, 2);
            offsetDistance = direction.magnitude + offsetDistance / 100;

            handVisual.position = origin + direction.normalized * offsetDistance;
        }

        handVisual.rotation = transform.rotation;
    }
    
    private void CalculationGrab()
    {
        if (grabAction.action.WasPressedThisFrame())
        {
            if (grabbedObject == null && canGrab)
            {
                grabbedObject = handCollider.collidingObject;
                offsetMatrix = GetTransformationMatrix(handVisual, true).inverse *
                               GetTransformationMatrix(grabbedObject.transform, true);
            }
        }
        else if (grabAction.action.IsPressed())
        {
            if (grabbedObject != null)
            {
                Matrix4x4 newTransform = GetTransformationMatrix(handVisual, true) * offsetMatrix;

                grabbedObject.transform.position = newTransform.GetColumn(3);
                grabbedObject.transform.rotation = newTransform.rotation;
            }
        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if(grabbedObject != null)
                grabbedObject.GetComponent<ObjectAccessHandler>().Release();
            grabbedObject = null;
            offsetMatrix = Matrix4x4.identity;
        }
    }

    #endregion
    
    #region Utility Functions

    public Matrix4x4 GetTransformationMatrix(Transform t, bool inWorldSpace = true)
    {
        if (inWorldSpace)
        {
            return Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
        }
        else
        {
            return Matrix4x4.TRS(t.localPosition, t.localRotation, t.localScale);
        }
    }

    #endregion
}
