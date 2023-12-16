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
        tutorialSections[0] =
            " - hold W-A-S-D keys for movement left, right, up, and down around the center of the world" +
            "\n - hold I-K keys for zooming in and out to get a closer or broader view" +
            "\n - press R key for resetting the camera to its default position" +
            "\n - press ESC key to return to the main menu at any time" +
            "\n Real-World Fact: Understanding the terrain and having a clear view of the area is crucial for strategic planning and effective fire control.";

        tutorialSections[1] =
            " - Click any tile with your left mouse key to ignite fire in designated areas, you can also click and drag" +
            "\n - after you ignite tiles you want, start the simulation by clicking the play button!" +
            "\n - pause or adjust the simulation speed anytime you want" +
            "\n - click reset if you want to start over" + 
            "\n Real-World Fact: In reality, water bodies act as natural barriers to fire spread due to their non-flammable nature, often used in creating firebreaks.";

        tutorialSections[2] =
            " In this world, the wind plays a significant role " +
            "\n Notice how the wind direction influences fire spread" +
            "\n Adjust the wind strength and observe the changes in fire behavior" +
            "\n Real-World Fact: Wind is a critical factor in wildfire spread. Strong winds can rapidly increase fire spread by carrying heat and embers to new areas.";

        tutorialSections[3] =
            " Watch how fire spreads more quickly uphill than downhill. Start the fire!" +
            "\n Real-World Fact: Fires tend to spread faster uphill due to the preheating of uphill vegetation and the rising hot air, which aids in igniting the vegetation above.";

        tutorialSections[4] =
            " Explore different vegetation types and their flammability." +
            "\n See how the type and flammability of material affect the burning duration and the chance of spreading to adjacent tiles." +
            "\n Real-World Fact: In wildfires, the type of vegetation significantly influences fire behavior. Some plants contain oils and resins that burn intensely, while others are less flammable.";

        tutorialSections[5] =
            " Fire Behavior can be very unpredictable" +
            "\n Understand how combining all aspects (wind, terrain, vegetation) can lead to unpredictable and complex fire behaviors." +
            "\n Use the ESC key or the main menu button to transition to either Play or Sandbox mode for further experimentation." +
            "\n Real-World Fact: Wildfires are complex and often unpredictable due to the interplay of various factors like weather, topography, and fuel types.";

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