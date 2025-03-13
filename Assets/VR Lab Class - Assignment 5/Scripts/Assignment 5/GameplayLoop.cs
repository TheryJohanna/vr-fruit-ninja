using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameplayLoop : NetworkBehaviour
{
    [Header("Game Settings")] 
    public Slider roundTimeSlider;
    public Slider speedSlider;
    public Collider buttonCollider;
    public UnityEvent onLaunch;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Rpc(SendTo.Server)]
    public void StartGameRpc()
    {
        StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        var elapsedTime = 0f;
        buttonCollider.GetComponent<ButtonPress>().isPressable = false;
        while (elapsedTime < roundTimeSlider.value)
        {
            yield return new WaitForSeconds(speedSlider.value);
            onLaunch?.Invoke();
            elapsedTime += speedSlider.value;
        }
        buttonCollider.GetComponent<ButtonPress>().isPressable = true;
    }
}
