using UnityEngine;
using SimpleFileBrowser;
using System.Collections;

public class FileBrowserHandler : MonoBehaviour
{
    public delegate void FileSelectedCallback(string filePath);

    // Configures and opens a file dialog window to import files. Also sets up filters to allow selecting just some files.
    public void ImportFile(FileSelectedCallback callback)
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Maps", ".json", ".png", ".jpg", ".jpeg"));
        FileBrowser.SetDefaultFilter(".json");

        StartCoroutine(ShowOpenDialogCoroutine(callback));
    }

    // Opens a dialog window for saving files, similar to ImportFile
    public void SaveFile(FileSelectedCallback callback)
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Maps", ".json"));
        FileBrowser.SetDefaultFilter(".json");

        StartCoroutine(ShowSaveDialogCoroutine(callback));
    }

    // A coroutine that displays the file load dialog window.
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

    // A coroutine that displays the file save dialog window.
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