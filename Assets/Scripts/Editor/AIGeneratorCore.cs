using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections.Generic;

public static class AIGeneratorCore
{
    private const string GEMINI_URL = "https://generativelanguage.googleapis.com/{0}/models/{1}:generateContent?key={2}";
    private const string OLLAMA_URL = "http://localhost:11434/api/generate";

    public delegate void OnNamesGenerated(string[] names);
    public delegate void OnError(string errorMessage);
    public delegate void OnTestResult(bool success, string message);

    private static readonly Dictionary<string, string> GEMINI_CONFIG = new Dictionary<string, string>
    {
        { "gemini-2.0-flash", "v1beta" },
        { "gemini-2.0-flash-lite-preview-02-05", "v1beta" },
        { "gemini-1.5-flash", "v1" },
        { "gemini-1.5-flash-latest", "v1beta" }
    };

    // ─────────────────────────────────────────
    // TEST CONNECTION
    // ─────────────────────────────────────────
    public static void TestConnection(bool isOllama, string apiKey, string modelId, OnTestResult resultCallback)
    {
        if (isOllama) TestOllama(modelId, resultCallback);
        else TestGemini(apiKey, modelId, resultCallback);
    }

    private static void TestGemini(string apiKey, string modelId, OnTestResult callback)
    {
        string version = GEMINI_CONFIG.ContainsKey(modelId) ? GEMINI_CONFIG[modelId] : "v1beta";
        string url = string.Format(GEMINI_URL, version, modelId, apiKey.Trim());
        string json = "{\"contents\":[{\"parts\":[{\"text\":\"OK\"}]}]}";
        SendRequest(url, json, (resp) => callback?.Invoke(true, "Gemini OK!"), (err) => callback?.Invoke(false, "Gemini Error."));
    }

    private static void TestOllama(string modelId, OnTestResult callback)
    {
        string json = "{\"model\":\"" + modelId + "\", \"prompt\":\"Katakan OK\", \"stream\":false}";
        SendRequest(OLLAMA_URL, json, (resp) => callback?.Invoke(true, "Ollama OK!"), (err) => callback?.Invoke(false, "Ollama Offline!"));
    }

    // ─────────────────────────────────────────
    // GENERATE NAMES
    // ─────────────────────────────────────────
    public static void RequestNames(bool isOllama, string apiKey, string modelId, string context, int count, OnNamesGenerated onSuccess, OnError onFailure)
    {
        string prompt = $"Berikan {count} nama unik untuk NPC game (konteks: {context}). Output CSV nama saja.";
        
        if (isOllama)
        {
            string json = "{\"model\":\"" + modelId + "\", \"prompt\":\"" + prompt + "\", \"stream\":false}";
            SendRequest(OLLAMA_URL, json, (resp) => onSuccess?.Invoke(ParseOllama(resp)), onFailure);
        }
        else
        {
            string version = GEMINI_CONFIG.ContainsKey(modelId) ? GEMINI_CONFIG[modelId] : "v1beta";
            string url = string.Format(GEMINI_URL, version, modelId, apiKey.Trim());
            string json = "{\"contents\":[{\"parts\":[{\"text\":\"" + prompt + "\"}]}]}";
            SendRequest(url, json, (resp) => onSuccess?.Invoke(ParseGemini(resp)), onFailure);
        }
    }

    private static void SendRequest(string url, string json, System.Action<string> onSuccess, OnError onFailure)
    {
        UnityWebRequest req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        var op = req.SendWebRequest();
        EditorApplication.CallbackFunction check = null;
        check = () => {
            if (op.isDone) {
                EditorApplication.update -= check;
                if (req.result == UnityWebRequest.Result.Success) onSuccess?.Invoke(req.downloadHandler.text);
                else { Debug.LogError(req.downloadHandler.text); onFailure?.Invoke(req.error); }
                req.Dispose();
            }
        };
        EditorApplication.update += check;
    }

    private static string[] ParseGemini(string json) {
        try {
            string key = "\"text\": \"";
            int start = json.IndexOf(key) + key.Length;
            int end = json.IndexOf("\"", start);
            return json.Substring(start, end - start).Replace("\\n", "").Trim().Split(',');
        } catch { return null; }
    }

    private static string[] ParseOllama(string json) {
        try {
            // Ollama JSON: {"model":"...","response":"Budi, Agus","done":true}
            string key = "\"response\":\"";
            int start = json.IndexOf(key) + key.Length;
            int end = json.IndexOf("\"", start);
            return json.Substring(start, end - start).Replace("\\n", "").Trim().Split(',');
        } catch { return null; }
    }
}
