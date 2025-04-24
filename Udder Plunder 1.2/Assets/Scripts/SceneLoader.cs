using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public Button Play_Button;
    public Button Quit_Button;
    public void On_Play()
    {
        SceneManager.LoadScene("PlayerMovement");
    }

    public void On_Quit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    
}
