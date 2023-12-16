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

    MainLogic mainLogic;

    void Start()
    {
        // Just one mainLogic should exist
        mainLogic = FindObjectOfType<MainLogic>();

        // set default sizes for worldGenerator
        mainLogic.ApplyInputValues();

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

        UpdateTutorialWorld();
    }

    private void UpdateTutorialWorld()
    {
        switch (currentSection)
        {
            case 0:
                Debug.Log("tut0");
                mainLogic.GenereteNewWorld();
                break;
            case 1:
                Debug.Log("tut1");
                // TODO load specific worlds
                mainLogic.OnImportClicked();
                break;
            case 2:
                Debug.Log("tut2");
                break;
            case 3:
                Debug.Log("tut3");
                break;
            case 4:
                Debug.Log("tut4");
                break;
            case 5:
                Debug.Log("tut5");
                break;
            default:
                Debug.LogError("There is no implementation for this section");
                break;
        }
    }

    private void UpdateButtons()
    {
        GameObject previousButtonParent = previousButton.transform.parent.gameObject;
        previousButtonParent.SetActive(currentSection != 0);

        GameObject nextButtonParent = nextButton.transform.parent.gameObject;
        nextButtonParent.SetActive(currentSection != tutorialSections.Length - 1);
    }
}