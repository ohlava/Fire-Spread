using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] Button runButton, pauseButton;
    [SerializeField] TextMeshProUGUI InfoPanel;

    // Update InfoPanel text when connected
    public void UpdateInfoPanel(string text)
    {
        InfoPanel.text = text;
    }

    // Set run and pause button states - allowing or not clicking on them
    public void UpdateRunPauseButtons(bool isRunning)
    {
        runButton.interactable = !isRunning;
        pauseButton.interactable = isRunning;
    }
}