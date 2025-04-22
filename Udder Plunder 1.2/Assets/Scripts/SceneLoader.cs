using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    public void MoveToScene(int sceneId)
    {
        Debug.Log("Fuck me in the ass MOVE");
        SceneManager.LoadScene(sceneId);
    }
}
