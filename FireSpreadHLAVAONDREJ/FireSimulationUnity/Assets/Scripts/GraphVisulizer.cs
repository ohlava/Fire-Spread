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
    public Text last_maxLabel;

    private List<GameObject> points; // List to keep track of point instances

    private float width;
    private float height;

    void Start()
    {
        // Adjust panel to cover left half of screen if it previously was the whole screen
        panel.anchorMin = new Vector2(0, 0);
        panel.anchorMax = new Vector2(0.5f, 1);
        panel.offsetMin = new Vector2(0, 0);
        panel.offsetMax = new Vector2(0, 0);

        // Calculate the space available in the panel
        width = panel.rect.width * 0.85f;
        height = panel.rect.height * 0.85f;

        // hide the panel and texts
        HideGraph();

        points = new List<GameObject>();
    }

    public void DrawGraph(Dictionary<int, int> data, string Y_text = "Y axis", string X_text = "X axis")
    {
        // clear previous graph
        ClearGraph();

        if (data.Count == 0) // otherwise data.Values.Max() has a problem
        {
            return;
        }

        // rename the axes
        yAxisLabel.text = Y_text;
        xAxisLabel.text = X_text;
        last_maxLabel.text = data.Last().Value + " current" +  " / " + data.Values.Max() + " max";

        // show all
        panel.gameObject.SetActive(true);
        xAxisLabel.gameObject.SetActive(true);
        yAxisLabel.gameObject.SetActive(true);
        last_maxLabel.gameObject.SetActive(true);

        // Calculate the intervals for the x and y axes
        float xInterval = xInterval = width / (data.Count + 1);
        float yInterval = yInterval = height / data.Values.Max(); ; //  Max cant handle empty data, careful for zero division!

        // Calculate the size of the points based on the count
        Vector2 pointSize = AdjustPointSize(data.Count);

        // Draw the graph
        int index = 1;
        foreach (KeyValuePair<int, int> entry in data)
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
            pointTransform.anchoredPosition = new Vector2(x+25f, y+25f); // 25f is just for the offset from the screen sides

            index++;

        }
    }

    // Calculate the size of the points based on the width of the panel and the number of points
    private Vector2 AdjustPointSize(int count)
    {
        float newSize = width / (count * 1.2f); // scaling factor 1.2f to ensure points do not touch each other

        // Ensure that points do not become too small or too large
        newSize = Mathf.Clamp(newSize, 5f, 30f);

        return new Vector2(newSize, newSize);
    }


    public void HideGraph()
    {
        // hide all
        panel.gameObject.SetActive(false);
        xAxisLabel.gameObject.SetActive(false);
        yAxisLabel.gameObject.SetActive(false);
        last_maxLabel.gameObject.SetActive(false);
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

// TODO own graph object and storing bunch different ones to easily vizulize different ones
/*
public class Graph : Dictionary<int, int>
{
    // unique type of graph
    public Text xAxisLabel;
    public Text yAxisLabel;

    public void AddPoint(int x, int y)
    {
        this[x] = y;
    }

    public void RemovePoint(int x)
    {
        this.Remove(x);
    }
}
*/


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
