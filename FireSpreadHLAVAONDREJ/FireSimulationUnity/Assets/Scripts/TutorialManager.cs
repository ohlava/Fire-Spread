using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] GameObject previousButton;
    [SerializeField] GameObject nextButton;
    [SerializeField] TextMeshProUGUI tutorialText;

    private int currentSection = 0;
    private string[] tutorialSections = new string[6];

    void Start()
    {
        // Initialize tutorial sections content
        tutorialSections[0] = "Section 1 content...";
        tutorialSections[1] = "Section 2 content...";
        tutorialSections[2] = "Section 3 content...";
        tutorialSections[3] = "Section 4 content...";
        tutorialSections[4] = "Section 5 content...";
        tutorialSections[5] = "Section 6 content...";

        UpdateTutorialSection();
    }

    public void HandlePrevious()
    {
        if (currentSection > 0)
        {
            currentSection--;
            UpdateTutorialSection();
        }
    }

    public void HandleNext()
    {
        if (currentSection < tutorialSections.Length - 1)
        {
            currentSection++;
            UpdateTutorialSection();
        }
    }

    private void UpdateTutorialSection()
    {
        tutorialText.text = tutorialSections[currentSection];
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        previousButton.SetActive(currentSection != 0);
        nextButton.SetActive(currentSection != tutorialSections.Length - 1);
    }
}