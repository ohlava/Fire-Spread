using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Linq;

public class GraphVisulizer : MonoBehaviour
{
    public RectTransform panel;
    public GameObject pointPrefab;
    public Text xAxisLabel;
    public Text yAxisLabel;
    private List<GameObject> points; // List to keep track of point instances

    private float width;
    private float height;

    void Start()
    {
        // TODO add axis with numbers, make points smaller over time, so graph is well visible (they stay the same size now)

        // Adjust panel to cover left half of screen
        panel.anchorMin = new Vector2(0, 0);
        panel.anchorMax = new Vector2(0.5f, 1);
        panel.offsetMin = new Vector2(0, 0);
        panel.offsetMax = new Vector2(0, 0);

        // Calculate the space available in the panel
        width = panel.rect.width * 0.9f;
        height = panel.rect.height * 0.8f;

        // hide the panel and texts
        HideGraph();

        points = new List<GameObject>();

        // texts are manually set in the editor
    }

    public void DrawGraph(Dictionary<int, int> data, string Y_text = "Y axis")
    {
        // clear previous graph
        ClearGraph();

        if (data.Count == 0)
        {
            return;
        }

        // rename the Y axis
        yAxisLabel.text = Y_text;

        // show all
        panel.gameObject.SetActive(true);
        xAxisLabel.gameObject.SetActive(true);
        yAxisLabel.gameObject.SetActive(true);

        // Calculate the intervals for the x and y axes
        float xInterval = xInterval = width / (data.Count + 1);
        float yInterval = yInterval = height / data.Values.Max(); // devided by data max value

        // Draw the graph
        int index = 1;
        foreach (KeyValuePair<int, int> entry in data)
        {
            GameObject point = Instantiate(pointPrefab, panel);
            RectTransform pointTransform = point.GetComponent<RectTransform>();
            points.Add(point);

            float x = xInterval * index;
            float y = yInterval * entry.Value;

            // Adjust the position to anchor at bottom left corner and make sure all points fall within panel
            pointTransform.anchorMin = new Vector2(0, 0);
            pointTransform.anchorMax = new Vector2(0, 0);
            pointTransform.pivot = new Vector2(0.5f, 0.5f); // Center the pivot point
            pointTransform.anchoredPosition = new Vector2(x, y);

            index++;
        }
    }

    public void HideGraph()
    {
        // hide all
        panel.gameObject.SetActive(false);
        xAxisLabel.gameObject.SetActive(false);
        yAxisLabel.gameObject.SetActive(false);
    }

    public void ClearGraph()
    {
        // Remove previous points
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


// Helper class for serialization
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
