using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Toggle useSimplifiedWorldVisualizationToggle;
    [SerializeField] Toggle saveTerrainAutomaticallyToggle;

    // Start is called before the first frame update
    void Start()
    {
        LoadSettings();

        useSimplifiedWorldVisualizationToggle.onValueChanged.AddListener(OnUseSimplifiedWorldVisualizationChanged);
        saveTerrainAutomaticallyToggle.onValueChanged.AddListener(OnSaveTerrainAutomaticallyChanged);
    }

    // Update is called once per frame
    void Update()
    {
        HandleEscPressed();
    }



    void LoadSettings()
    {
        Settings currentSettings = SettingsManager.LoadSettings();
        useSimplifiedWorldVisualizationToggle.isOn = currentSettings.UseSimplifiedWorldVisualization;
        saveTerrainAutomaticallyToggle.isOn = currentSettings.SaveTerrainAutomatically;
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

    public void LoadPredictions()
    {
        SceneManager.LoadScene("Predictions");
        return;
    }
}
