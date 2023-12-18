using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Update()
    {
        HandleEscPressed();
    }

    private void HandleEscPressed()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void QuitApp()
    {
        Application.Quit();
        return;
    }

    public void LoadTutorial()
    {
        SceneManager.LoadScene("Tutorial");
        return;
    }

    public void LoadSandBox()
    {
        SceneManager.LoadScene("SampleScene");
        return;
    }
}
