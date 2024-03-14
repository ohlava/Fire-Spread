using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Toggle useSimplifiedWorldVisualizationToggle;
    public Toggle saveTerrainAutomaticallyToggle;

    void Start()
    {
        LoadSettings();

        // Add listeners for toggle changes
        useSimplifiedWorldVisualizationToggle.onValueChanged.AddListener(OnUseSimplifiedWorldVisualizationChanged);
        saveTerrainAutomaticallyToggle.onValueChanged.AddListener(OnSaveTerrainAutomaticallyChanged);
    }

    void Update()
    {
        HandleEscPressed();
    }



    void LoadSettings()
    {
        Settings currentSettings = SettingsManager.LoadSettings();
        useSimplifiedWorldVisualizationToggle.isOn = currentSettings.useSimplifiedWorldVisualization;
        saveTerrainAutomaticallyToggle.isOn = currentSettings.saveTerrainAutomatically;
    }

    void OnUseSimplifiedWorldVisualizationChanged(bool isOn)
    {
        UpdateSettings();
    }

    void OnSaveTerrainAutomaticallyChanged(bool isOn)
    {
        UpdateSettings();
    }

    void UpdateSettings()
    {
        Settings updatedSettings = new Settings(useSimplifiedWorldVisualizationToggle.isOn, saveTerrainAutomaticallyToggle.isOn);
        SettingsManager.SaveSettings(updatedSettings);
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
        SceneManager.LoadScene("SandBox");
        return;
    }
}
