using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicingSword : MonoBehaviour
{
    [Header("Blade Tracker")]
    public GameObject bladeTip;
    public GameObject bladeBase;
    private Vector3 _tipPositionEnter;
    private Vector3 _tipPositionExit;
    private Vector3 _basePositionEnter;
    private Vector3 _basePositionExit;
    private Vector3 _tipPositionStart;
    private Vector3 _basePositionStart;
    
    // Start is called before the first frame update
    void Start()
    {
        _tipPositionStart = bladeTip.transform.position;
        _basePositionStart = bladeBase.transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        _tipPositionEnter = bladeTip.transform.position;
        _basePositionEnter = bladeBase.transform.position;
    }

    private void OnTriggerExit(Collider other)
    {
        _tipPositionExit = bladeTip.transform.position;
        
        // create triangle shape from swing
        var side1 = _tipPositionExit - _tipPositionEnter;
        var side2 = _tipPositionExit - _basePositionEnter;
    }
}
