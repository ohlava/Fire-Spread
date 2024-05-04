using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System;

// Handles script execution with support for input/output redirection and timeout management.
public class PythonCaller
{
    public string ScriptName = "PythonScripts/main.py";
    public string PythonPath = "/usr/bin/python3";

    public PythonCaller()
    {
    }

    public PythonCaller(string script, string python)
    {
        ScriptName = script;
        PythonPath = python;
    }

    // Asynchronously calls a Python script using the configured paths and input data while handling errors and script timeouts, logging details for troubleshooting.
    public async Task<string> CallPythonScript(InputDataSerializationPackage inputData)
    {
        if (!File.Exists(PythonPath))
        {
            UnityEngine.Debug.LogError("Python executable not found at specified path.");
            return null;
        }

        string scriptPath = Path.Join(Application.streamingAssetsPath, ScriptName);
        if (!File.Exists(scriptPath))
        {
            UnityEngine.Debug.LogError("Python script not found at specified path.");
            return null;
        }

        try
        {
            int timeout = 5000; // Script timeout
            string[] args = { "predict" }; // Can be modified, no use right now
            string jsonString = JsonUtility.ToJson(inputData);
            string output = await RunPythonScript(scriptPath, args, jsonString, timeout); // What python prints into StandardOutput
            UnityEngine.Debug.Log("Python script completed");
            UnityEngine.Debug.Log("Output of python script: " + output);
            return output;
        }
        catch (TimeoutException)
        {
            UnityEngine.Debug.LogError("Python script execution timed out.");
            return null;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"An error occurred while calling the Python script: {ex.Message}");
            return null;
        }
    }

    private async Task<string> RunPythonScript(string scriptPath, string[] args, string input, int timeout)
    {
        ProcessStartInfo psi = new ProcessStartInfo(PythonPath)
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
            process.EnableRaisingEvents = true;

            // Set up event handlers
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    UnityEngine.Debug.LogWarning("Python stderr: " + e.Data);
            };
            process.Exited += (sender, e) => exitCompletionSource.SetResult(true);

            process.Start();
            process.BeginErrorReadLine();

            Task writeTask = process.StandardInput.WriteLineAsync(input); // Pass JSON string to that script
            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();

            var timeoutTask = Task.Delay(timeout); // Start the timeout task

            // Wait for either the process to exit or the timeout
            var completedTask = await Task.WhenAny(exitCompletionSource.Task, timeoutTask);

            if (completedTask == timeoutTask && !process.HasExited)
            {
                // Timeout logic
                process.Kill();
                throw new TimeoutException("Python script execution exceeded the time limit.");
            }
            else if (process.ExitCode != 0)
            {
                // Process exited before timeout but with an error
                UnityEngine.Debug.LogError("Python process exited with code: " + process.ExitCode + " (problem occured)");
                throw new Exception("Python script exited with error.");
            }

            try
            {
                await writeTask;
            }
            catch (IOException writeException)
            {
                throw new Exception($"Failed to write input to Python script: {writeException.Message}");
            }

            try
            {
                return await outputTask;
            }
            catch (IOException readException)
            {
                throw new Exception($"Failed to read output from Python script: {readException.Message}");
            }
        }
    }
}