using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene("__Prospector_Scene_0"); 
    }
}
