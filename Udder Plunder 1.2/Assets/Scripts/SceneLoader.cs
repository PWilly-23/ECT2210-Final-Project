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
    public Button Setting_Button;
    public Canvas SettingCanvas;
    public Canvas MenuCanvas;

    void start()
    {
        MenuCanvas.gameObject.SetActive(true);  
        SettingCanvas.gameObject.SetActive(false);
    }
    public void On_Play()
    {
        SceneManager.LoadScene("PlayerMovement");
    }

    public void On_Quit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void On_Settings()
    {
        Debug.Log("setting");
        MenuCanvas.gameObject.SetActive(false);
        SettingCanvas.gameObject.SetActive(true);   
    }


    public void On_Back_Settings()
    {
        MenuCanvas.gameObject.SetActive(true);
        SettingCanvas.gameObject.SetActive(false);
    }

    
}
