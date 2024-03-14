using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button runButton, pauseButton;
    [SerializeField] private TextMeshProUGUI InfoPanel;

    // Method to update InfoPanel text
    public void UpdateInfoPanel(string text)
    {
        InfoPanel.text = text;
    }

    // Method to set run and pause button states
    public void UpdateRunPauseButtons(bool isRunning)
    {
        runButton.interactable = !isRunning;
        pauseButton.interactable = isRunning;
    }

    // Additional methods to control other UI elements...
}
