using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ToggleInteractRay : MonoBehaviour
{
    #region Member Variables
    public XRRayInteractor xrRayInteractor;
    public XRInteractorLineVisual lineVisual;
    public InputActionReference joystickPress;
    #endregion
    
    #region Unity Callbacks

    void Start()
    {
        xrRayInteractor.enabled = false;
        lineVisual.enabled = false;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (joystickPress.action.WasPressedThisFrame())
        {
            xrRayInteractor.enabled = !xrRayInteractor.enabled;
            lineVisual.enabled = !lineVisual.enabled;
        }
    }
    #endregion
}
