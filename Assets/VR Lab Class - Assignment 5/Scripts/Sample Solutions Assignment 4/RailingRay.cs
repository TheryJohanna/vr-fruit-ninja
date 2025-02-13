using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class RailingRay : MonoBehaviour
{
    #region Member Variables
    
    public LineRenderer ray;
    public Transform rayOrigin;
    public float rayLength = 100f;
    public LayerMask layers;
    private GameObject selectedObject;
    private RaycastHit hit;

    public InputActionProperty grabAction;
    private GameObject grabbedObject;
    private Matrix4x4 offsetMatrix;

    private bool canGrab
    {
        get
        {
            if (selectedObject != null)
                return selectedObject.GetComponent<ObjectAccessHandler>().RequestAccess();
            return false;
        }
    }

    public InputActionProperty railingAction;
    public float movementSpeed = 3f;
    public float rotationSpeed = 180f;

    public InputActionProperty toggleAction;
    private bool enabled = false;

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

    private void Update()
    {
        if (toggleAction.action.WasPressedThisFrame())
            ToggleRay();
        
        if (!enabled)
            return;
        
        UpdateRay();
        ApplyGrabbing();

        if (grabbedObject != null)
            ApplyRailing();
    }

    #endregion

    #region Custom Methods

    private void ToggleRay()
    {
        enabled = !enabled;
        ray.enabled = enabled;
    }

    private void UpdateRay()
    {
        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward * rayLength, out hit, layers))
        {
            ray.SetPosition(0, rayOrigin.position);
            ray.SetPosition(1, hit.point);
            ray.startColor = Color.green;
            ray.endColor = Color.green;

            selectedObject = hit.collider.gameObject;
        }
        else
        {
            ray.SetPosition(0, rayOrigin.position);
            ray.SetPosition(1, rayOrigin.position + rayOrigin.forward * rayLength);
            ray.startColor = Color.red;
            ray.endColor = Color.red;

            selectedObject = null;
        }
    }

    private void ApplyGrabbing()
    {
        if (grabAction.action.WasPressedThisFrame())
        {
            if (grabbedObject == null && canGrab)
            {
                grabbedObject = selectedObject;
                offsetMatrix = GetTransformationMatrix(transform, true).inverse *
                               GetTransformationMatrix(grabbedObject.transform, true);
            }
        }
        else if (grabAction.action.IsPressed())
        {
            if (grabbedObject != null)
            {
                Matrix4x4 newTransform = GetTransformationMatrix(transform, true) * offsetMatrix;

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

    private void ApplyRailing()
    {
        float movementInput = railingAction.action.ReadValue<Vector2>().y;
        Vector3 movement = rayOrigin.forward * (movementSpeed * movementInput * Time.deltaTime);
        grabbedObject.transform.Translate(movement, Space.World);

        float rotationInput = railingAction.action.ReadValue<Vector2>().x;
        float rotation = rotationSpeed * rotationInput * Time.deltaTime;
        grabbedObject.transform.Rotate(Vector3.up, rotation, Space.Self);
        
        offsetMatrix = GetTransformationMatrix(transform, true).inverse *
                       GetTransformationMatrix(grabbedObject.transform, true);
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
