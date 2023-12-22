using UnityEngine;
using SimpleFileBrowser;
using System.Collections;

public class FileBrowserHandler : MonoBehaviour
{
    public delegate void FileSelectedCallback(string filePath);

    public void ImportFile(FileSelectedCallback callback)
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Maps", ".json", ".png"));
        FileBrowser.SetDefaultFilter(".json");

        StartCoroutine(ShowOpenDialogCoroutine(callback));
    }

    public void SaveFile(FileSelectedCallback callback)
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Maps", ".json"));
        FileBrowser.SetDefaultFilter(".json");

        StartCoroutine(ShowSaveDialogCoroutine(callback));
    }

    private IEnumerator ShowOpenDialogCoroutine(FileSelectedCallback callback)
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, Application.streamingAssetsPath, "worldSave", "Load Map", "Load");

        if (FileBrowser.Success)
        {
            callback?.Invoke(FileBrowser.Result[0]);
        }
        else
        {
            callback?.Invoke(null);
        }
    }

    private IEnumerator ShowSaveDialogCoroutine(FileSelectedCallback callback)
    {
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, Application.streamingAssetsPath, "worldSave", "Save Map", "Save");

        if (FileBrowser.Success)
        {
            string filePath = FileBrowser.Result[0];
            if (!filePath.EndsWith(".json"))
            {
                filePath += ".json";
            }

            callback?.Invoke(filePath);
        }
        else
        {
            callback?.Invoke(null);
        }
    }
}
