using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;

public class PythonCaller : MonoBehaviour
{
    [SerializeField] GameObject mainObj;
    MainLogic mainLogic;
    string scriptName;

    private void Awake()
    {
        mainLogic = mainObj?.GetComponent<MainLogic>();

        scriptName = "PythonScripts/main.py";
    }
    public async void CallPythonScript()
    {
        string scriptPath = Path.Join(Application.streamingAssetsPath, scriptName);
        string[] args = { "arg1", "arg2" };  // Example arguments for now
        string jsonString = mainLogic.worldFileManager.GetWorldSerialized(mainLogic.world);

        string output = await RunPythonScript(scriptPath, args, jsonString); // What python prints into StandardOutput
        UnityEngine.Debug.Log("Python script completed");
        UnityEngine.Debug.Log("Output of python script: " + output);
    }

    private async Task<string> RunPythonScript(string scriptPath, string[] args, string input)
    {
        ProcessStartInfo psi = new ProcessStartInfo("/usr/bin/python3")
        {
            ArgumentList = { scriptPath },
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        // Add arguments to the process start info
        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        TaskCompletionSource<bool> exitCompletionSource = new TaskCompletionSource<bool>();

        using (Process process = new Process())
        {
            process.StartInfo = psi;

            // Set up event handlers
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    UnityEngine.Debug.LogWarning("Python stderr: " + e.Data);
            };
            process.Exited += (sender, e) => exitCompletionSource.SetResult(true);

            process.EnableRaisingEvents = true;

            process.Start();

            process.BeginErrorReadLine();

            // this is how we are gona pass JSON file to that script for prediction / reading file training data will be from file
            Task writeTask = process.StandardInput.WriteLineAsync(input);
            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();

            // Wait for the process to exit
            await exitCompletionSource.Task;
            if (process.ExitCode != 0)
            {
                UnityEngine.Debug.LogError("Python process exited with code: " + process.ExitCode + " (problem occured)");
            }

            try
            {
                await writeTask;
            }
            catch (IOException ex)
            {
                UnityEngine.Debug.LogError("Writing python input failed");
                UnityEngine.Debug.LogException(ex);
            }

            try
            {
                return await outputTask;
            }
            catch (IOException ex)
            {
                UnityEngine.Debug.LogError("Reading python output failed");
                UnityEngine.Debug.LogException(ex);
                return null;
            }
        }
    }
}