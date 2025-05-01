using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public bool BuildState = false;
    private UFOControls controls;
    private void Awake()
    {
        controls = new UFOControls();

        

        // Subscribe to the beam input.
        controls.Player.Mode.performed += ctx => SetBuildMode();
        
    }
    private void OnEnable() { controls.Enable(); }
    private void OnDisable() { controls.Disable(); }

    private void SetBuildMode()
    {
        if (BuildState == false){

            BuildState = true;

        } else{

            BuildState = false;
        }
    }

}
