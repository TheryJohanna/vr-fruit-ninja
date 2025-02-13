using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class VirtualHand : MonoBehaviour
{
    #region Enum

    private enum VirtualHandMode
    {
        Snap,
        Reparenting,
        NoReparenting
    }

    #endregion
    
    #region Member Variables

    public InputActionProperty toggleModeAction;
    [SerializeField] private VirtualHandMode virtualHandMode = VirtualHandMode.Snap;

    public InputActionProperty grabAction;
    public HandCollider handCollider;

    private GameObject _grabbedObject = null;
    private GameObject _grabbedObjectParent = null;
    private Matrix4x4 _offsetMatrix;

    private XRBaseController _controller;
    
    private MeshRenderer _meshRenderer;
    private Color _originalColor;
    public Color pressedColor = Color.green;

    private bool canGrab
    {
        get
        {
            if (handCollider.isColliding)
            {
                return handCollider.collidingObject.GetComponent<ObjectAccessHandler>().RequestAccess();
            }
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
        
        // from ChatGPT
        _controller = GetComponent<XRBaseController>();
        if (_controller.IsUnityNull())
        {
            Debug.LogError("XRBaseController not found on " + gameObject.name);
        }
    }

    private void Update()
    {
        if (toggleModeAction.action.WasPressedThisFrame())
            virtualHandMode = (VirtualHandMode)(((int)virtualHandMode + 1) % 3);

        switch (virtualHandMode)
        {
            case VirtualHandMode.Snap:
                SnapGrab();
                break;
            case VirtualHandMode.Reparenting:
                ReparentingGrab();
                break;
            case VirtualHandMode.NoReparenting:
                CalculationGrab();
                break;
        }
    }

    #endregion

    #region Custom Methods

    private void SnapGrab()
    {
        if (grabAction.action.IsPressed())
        {
            if (_grabbedObject.IsUnityNull() && canGrab)
            {
                _grabbedObject = handCollider.collidingObject;
                
                // from ChatGPT
                _controller.SendHapticImpulse(0.5f, 0.3f);
                SetNewColor(_grabbedObject.GetComponent<MeshRenderer>(), pressedColor);
            }

            if (!_grabbedObject.IsUnityNull())
            {
                _grabbedObject.transform.position = transform.position;
                _grabbedObject.transform.rotation = transform.rotation;
            }
        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if (!_grabbedObject.IsUnityNull())
            {
                RestoreColor();
                _grabbedObject.GetComponent<ObjectAccessHandler>().Release();
            }
            _grabbedObject = null;
        }
    }

    private void ReparentingGrab()
    {
        if (grabAction.action.IsPressed())
        {
            if (_grabbedObject.IsUnityNull() && canGrab)
            {
                _grabbedObject = handCollider.collidingObject;
                
                // from ChatGPT
                _controller.SendHapticImpulse(0.5f, 0.3f);
                SetNewColor(_grabbedObject.GetComponent<MeshRenderer>(), pressedColor);
            }
            
            if (!_grabbedObject.IsUnityNull())
            {
                // Utility functions suggested by ChatGPT
                _grabbedObjectParent = _grabbedObject.transform.parent?.gameObject;
                _grabbedObject.transform.SetParent(transform, true);
            }
        } 
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if (!_grabbedObject.IsUnityNull())
            {
                RestoreColor();
                // Utility functions suggested by ChatGPT
                _grabbedObject.transform.SetParent(null, true);
                _grabbedObject.GetComponent<ObjectAccessHandler>().Release();
            }
            _grabbedObject = null;
            _grabbedObjectParent = null;
        }
    }

    private void CalculationGrab()
    {
        if (grabAction.action.IsPressed())
        {
            if (_grabbedObject.IsUnityNull() && canGrab)
            {
                // Utility functions suggested by ChatGPT
                _grabbedObject = handCollider.collidingObject;
                _offsetMatrix = transform.worldToLocalMatrix * _grabbedObject.transform.localToWorldMatrix;
                
                // from ChatGPT
                _controller.SendHapticImpulse(0.5f, 0.3f);
                SetNewColor(_grabbedObject.GetComponent<MeshRenderer>(), pressedColor);
            }

            if (!_grabbedObject.IsUnityNull())
            {
                // Utility functions suggested by ChatGPT
                var newMatrix = transform.localToWorldMatrix * _offsetMatrix;
                _grabbedObject.transform.SetPositionAndRotation(newMatrix.GetPosition(), newMatrix.rotation);
            }
        } else if (grabAction.action.WasReleasedThisFrame())
        {
            if (!_grabbedObject.IsUnityNull())
            {
                RestoreColor();
                _grabbedObject.GetComponent<ObjectAccessHandler>().Release();
            }
            _grabbedObject = null;
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

    private void SetNewColor(MeshRenderer meshRenderer, Color color)
    {
        // with suggestions from ChatGPT
        _meshRenderer = meshRenderer;
        _originalColor = handCollider.originalColor;
        _meshRenderer.material.color = color;
    }

    private void RestoreColor()
    {
        // with suggestions from ChatGPT
        _meshRenderer.material.color = handCollider.hoverColor;
        _meshRenderer = null;
    }

    #endregion
}
