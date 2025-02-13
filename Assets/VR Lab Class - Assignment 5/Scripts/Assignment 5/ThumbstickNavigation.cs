using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using VRSYS.Core.Avatar;
using VRSYS.Core.Networking;

public class ThumbstickNavigation : MonoBehaviour
{
    public enum InputMapping
    {
        PositionControl,
        VelocityControl,
        AccelerationControl
    }

    [Header("General Configuration")]
    public InputMapping inputMapping = InputMapping.PositionControl;
    public Transform head;
    public InputActionReference steeringAction;
    public InputActionReference rotationAction;
    
    [Header("Navigation Configuration")]
    private Vector3 startingPosition;
    public float positionOffset = 10f; // max position offset during position control 
    [Range(0.1f, 10.0f)] public float steeringSpeed = 2f; // max steering speed for rate control
    [Range(0.1f, 10.0f)] public float acceleration = 2f; // max acceleration during acceleration control
    [Range(0.1f, 10.0f)] public float maxVelocity = 5; // max velocity reached during acceleration control
    private Vector3 _currentVelocity = Vector3.zero;
    
    [Header("Rotation Configuration")]
    [Range(1.0f, 180.0f)] // Draws a slider with range in the inspector 
    public float rotationSpeed = 30f; // In angle degrees per second
    public bool snapRotation = false;
    public float snapAngles = 30f; // In angle degrees per snap
    private float _prevThumstickReading = 0f;

    [Header("Groundfollowing Configuration")]
    public LayerMask groundLayerMask;
    private RaycastHit _hit;
    [Range(1.2f, 2.0f)]
    public float height = 1.2f;

    private void Start()
    {
        if (head == null)
            head = GetComponent<AvatarHMDAnatomy>().head;
        
        // Reference point for computing position control
        startingPosition = transform.position;
        
        // get first ray casting hit as the initial height that should be maintained
        //Physics.Raycast(head.position, Vector3.down, out _hit, Mathf.Infinity, groundLayerMask);
        
    }

    void Update()
    {
        ApplyDisplacement();
        ApplyRotation();
        ApplyGroundfollowing();
    }

    private void ApplyDisplacement()
    {
        Vector2 input = steeringAction.action.ReadValue<Vector2>();
        switch (inputMapping)
        {
            case InputMapping.PositionControl:
                PositionControl(input);
                break;
            case InputMapping.VelocityControl:
                VelocityControl(input);
                break;
            case InputMapping.AccelerationControl:
                AccelerationControl(input);
                break;
        }
    }

    private void PositionControl(Vector2 input)
    {
        transform.position = new Vector3(
            startingPosition.x + input.x * positionOffset, 
            startingPosition.y, 
            startingPosition.z + input.y * positionOffset
            );
    }
    
    private void VelocityControl(Vector2 input)
    {
        var playerForward = head.forward;
        var playerRight = head.right;
        playerForward.Normalize();
        playerRight.Normalize();
        
        var velocity = (playerForward * input.y + playerRight * input.x) * steeringSpeed;
        
        transform.position += velocity * Time.deltaTime;
    }
    
    private void AccelerationControl(Vector2 input)
    {
        var playerForward = head.forward;
        var playerRight = head.right;
        playerForward.Normalize();
        playerRight.Normalize();

        var acc = (playerForward * input.y + playerRight * input.x).normalized * acceleration;
        var clampedVelocity = Vector3.ClampMagnitude(acc * Time.deltaTime, maxVelocity);

        _currentVelocity += clampedVelocity;
        
        transform.position += _currentVelocity * Time.deltaTime;
    }
    
    private void ApplyRotation()
    {
        var thumbstickReading = rotationAction.action.ReadValue<Vector2>();
        
        switch (snapRotation)
        {
            case false:
            {
                if (thumbstickReading.x != 0)
                {
                    transform.RotateAround(head.position, Vector3.up, (rotationSpeed * thumbstickReading.x) * Time.deltaTime);
                }
                break;
            }
            case true:
                if (_prevThumstickReading == 0 && thumbstickReading.x != 0)
                {
                    if (thumbstickReading.x > 0)
                    {
                        transform.RotateAround(head.position, Vector3.up, snapAngles);
                    }
                    else if (thumbstickReading.x < 0)
                    {
                        transform.RotateAround(head.position, Vector3.up, -snapAngles);
                    }
                }

                // Update the previous thumbstick x-axis value
                _prevThumstickReading = thumbstickReading.x;
                break;
        }
    }
    
    private void ApplyGroundfollowing()
    {
        var clampedPlayerHeight = Math.Clamp(head.transform.localPosition.y, 1f, 3f);
        Physics.Raycast(head.position, Vector3.down, out _hit, Mathf.Infinity, groundLayerMask);
        transform.Translate(0, clampedPlayerHeight - _hit.distance, 0);
    }
}