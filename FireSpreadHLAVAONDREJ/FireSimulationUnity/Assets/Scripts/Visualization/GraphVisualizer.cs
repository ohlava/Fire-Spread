using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using TMPro;

public class GraphVisualizer : MonoBehaviour
{
    [SerializeField] RectTransform panel;
    [SerializeField] GameObject pointPrefab;
    [SerializeField] TextMeshProUGUI xAxisLabel;
    [SerializeField] TextMeshProUGUI yAxisLabel;
    [SerializeField] TextMeshProUGUI lastMaxLabel;

    private List<GameObject> points = new List<GameObject>();
    private Dictionary<int, int> currentData = new Dictionary<int, int> { { -1, 0 } };

    private float width;
    private float height;

    void Start()
    {
        panel.anchorMin = new Vector2(0, 0);
        panel.anchorMax = new Vector2(0.5f, 1); // First value is percentage of screen panel width for background panel to take, second is for screen panel height
        panel.offsetMin = new Vector2(0, 0);
        panel.offsetMax = new Vector2(0, 0);

        PositionLabels();

        // Calculate the space available in the panel with some margin for the point
        width = panel.rect.width * 0.85f;
        height = panel.rect.height * 0.85f;

        HideGraph();
    }

    public void SetData(Dictionary<int, int> data)
    {
        currentData = data;
    }

    public void SetAxes(string Y_text = "Y axis", string X_text = "X axis")
    {
        xAxisLabel.text = X_text;
        yAxisLabel.text = Y_text;
    }

    private void PositionLabels()
    {
        float margin = 15f; // Margin from the edges of the panel

        // yAxisLabel at the left top
        yAxisLabel.rectTransform.anchorMin = new Vector2(0, 1);
        yAxisLabel.rectTransform.anchorMax = new Vector2(0, 1);
        yAxisLabel.rectTransform.pivot = new Vector2(0, 1); // Top left pivot
        yAxisLabel.rectTransform.anchoredPosition = new Vector2(margin, -margin);

        // lastMaxLabel directly below yAxisLabel
        lastMaxLabel.rectTransform.anchorMin = new Vector2(0, 1);
        lastMaxLabel.rectTransform.anchorMax = new Vector2(0, 1);
        lastMaxLabel.rectTransform.pivot = new Vector2(0, 1); // Top left pivot
        lastMaxLabel.rectTransform.anchoredPosition = new Vector2(margin, -margin - yAxisLabel.preferredHeight - 5f);

        // xAxisLabel at the bottom right
        xAxisLabel.rectTransform.anchorMin = new Vector2(1, 0);
        xAxisLabel.rectTransform.anchorMax = new Vector2(1, 0);
        xAxisLabel.rectTransform.pivot = new Vector2(1, 0); // Bottom right pivot
        xAxisLabel.rectTransform.anchoredPosition = new Vector2(-margin, margin);
    }

    public void UpdateGraph()
    {
        ClearGraph();

        if (currentData.Count == 0) // otherwise data.Values.Max() has a problem
        {
            return;
        }

        lastMaxLabel.text = currentData.Last().Value + " current" + " / " + currentData.Values.Max() + " max";

        // Calculate the intervals for the x and y axes
        float xInterval = width / (currentData.Count + 1);
        float yInterval = height / currentData.Values.Max(); ; //  Max cant handle empty data, careful for zero division!

        // Calculate the size of the points based of their counts
        Vector2 pointSize = AdjustPointSize(currentData.Count);

        // Draw the graph
        int index = 1;
        foreach (KeyValuePair<int, int> entry in currentData)
        {
            GameObject point = Instantiate(pointPrefab, panel);
            RectTransform pointTransform = point.GetComponent<RectTransform>();
            points.Add(point);

            // Adjust the size of the points
            pointTransform.sizeDelta = pointSize;

            float x = xInterval * index;
            float y = yInterval * entry.Value;

            // Adjust the position to anchor at bottom left corner and make sure all points fall within panel
            pointTransform.anchorMin = new Vector2(0, 0);
            pointTransform.anchorMax = new Vector2(0, 0);
            pointTransform.pivot = new Vector2(0.5f, 0.5f); // Center the pivot point
            pointTransform.anchoredPosition = new Vector2(x + 25f, y + 25f); // 25f is just for the offset from the screen sides

            index++;
        }
    }

    // Calculate the size of the points based on the width of the panel and the number of points
    private Vector2 AdjustPointSize(int count)
    {
        float newSize = width / (count * 1.2f); // scaling factor 1.2f to ensure points do not touch each other

        newSize = Mathf.Clamp(newSize, 5f, 30f); // Ensure that points do not become too small or too large

        return new Vector2(newSize, newSize);
    }

    // Show the panel and labels
    public void ShowGraph()
    {
        if (currentData == null || currentData.Count == 0)
        {
            Debug.LogWarning("No data to display in graph.");
            return;
        }

        panel.gameObject.SetActive(true);
        xAxisLabel.gameObject.SetActive(true);
        yAxisLabel.gameObject.SetActive(true);
        lastMaxLabel.gameObject.SetActive(true);
    }

    // Hide the panel and labels
    public void HideGraph()
    {
        xAxisLabel.gameObject.SetActive(false);
        yAxisLabel.gameObject.SetActive(false);
        lastMaxLabel.gameObject.SetActive(false);
        panel.gameObject.SetActive(false);
    }

    // Remove previous points
    public void ClearGraph()
    {
        foreach (GameObject point in points)
        {
            Destroy(point);
        }
        points.Clear();
    }

    public static void SaveToFile(Dictionary<int, int> dict, string pathAndName = "Assets/graph.json")
    {
        string json = JsonUtility.ToJson(new Serialization<int, int>(dict));
        File.WriteAllText(pathAndName, json);
    }

}

[System.Serializable]
public class Serialization<T1, T2>
{
    [SerializeField]
    List<T1> keys;
    [SerializeField]
    List<T2> values;

    public Serialization(Dictionary<T1, T2> dict)
    {
        keys = new List<T1>(dict.Keys);
        values = new List<T2>(dict.Values);
    }
}