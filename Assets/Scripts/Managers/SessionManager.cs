using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Mengelola sesi permainan saat player kembali ke Main Menu dari Pause.
/// Menggunakan Additive Scene Loading agar progress game tetap tersimpan di memori.
/// Saat aplikasi ditutup, sesi hilang dan game dimulai dari awal.
/// </summary>
public static class SessionManager
{
    public static bool HasActiveSession { get; private set; } = false;
    private static int _gameSceneBuildIndex = -1;

    // Menyimpan musik gameplay agar bisa di-restore saat continue
    private static AudioClip _savedMusicClip = null;

    // Menyimpan state aktif/nonaktif setiap root object sebelum dinonaktifkan
    private static Dictionary<GameObject, bool> _savedActiveStates = new Dictionary<GameObject, bool>();

    public static void ReturnToMainMenu()
    {
        _gameSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
        HasActiveSession = true;
        _savedActiveStates.Clear();

        // Simpan musik gameplay yang sedang diputar
        if (AudioManager.Instance != null)
        {
            _savedMusicClip = AudioManager.Instance.GetCurrentMusic();
        }

        Debug.Log($"[SessionManager] ReturnToMainMenu - Saving game scene index: {_gameSceneBuildIndex}");

        SceneManager.sceneLoaded += OnMainMenuLoaded;
        SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);
    }

    private static void OnMainMenuLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != 0) return;
        SceneManager.sceneLoaded -= OnMainMenuLoaded;

        Debug.Log($"[SessionManager] MainMenu loaded. Total scenes: {SceneManager.sceneCount}");

        SceneManager.SetActiveScene(scene);

        // Simpan state asli setiap root object, LALU nonaktifkan semuanya
        Scene gameScene = SceneManager.GetSceneByBuildIndex(_gameSceneBuildIndex);
        if (gameScene.IsValid() && gameScene.isLoaded)
        {
            GameObject[] roots = gameScene.GetRootGameObjects();
            foreach (GameObject go in roots)
            {
                // Catat apakah object ini aktif atau tidak SEBELUM dimatikan
                _savedActiveStates[go] = go.activeSelf;
                go.SetActive(false);
            }
            Debug.Log($"[SessionManager] Saved & disabled {roots.Length} root objects.");
        }
        else
        {
            Debug.LogError("[SessionManager] Game scene NOT found! Additive loading may have failed.");
            HasActiveSession = false;
        }

        Time.timeScale = 1f;
    }

    public static void ContinueGame()
    {
        if (!HasActiveSession)
        {
            Debug.LogWarning("[SessionManager] ContinueGame called but no active session!");
            return;
        }

        Scene gameScene = SceneManager.GetSceneByBuildIndex(_gameSceneBuildIndex);
        if (!gameScene.IsValid() || !gameScene.isLoaded)
        {
            Debug.LogWarning("[SessionManager] Game scene lost! Starting fresh.");
            ClearSession();
            SceneManager.LoadScene(1);
            return;
        }

        // Kembalikan setiap root object ke STATE ASLINYA (bukan nyalakan semua)
        GameObject[] roots = gameScene.GetRootGameObjects();
        foreach (GameObject go in roots)
        {
            if (_savedActiveStates.TryGetValue(go, out bool wasActive))
            {
                go.SetActive(wasActive);
            }
            else
            {
                go.SetActive(true); // Fallback jika tidak ada data
            }
        }
        Debug.Log($"[SessionManager] Restored {roots.Length} root objects to original states.");

        SceneManager.SetActiveScene(gameScene);
        SceneManager.UnloadSceneAsync(0);

        // Kembalikan musik gameplay
        if (_savedMusicClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(_savedMusicClip);
        }

        // Lanjutkan game melalui PauseManager
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.ResumeGame();
        }
        else
        {
            Time.timeScale = 1f;
        }

        ClearSession();
        Debug.Log("[SessionManager] Game continued successfully!");
    }

    public static void ClearSession()
    {
        HasActiveSession = false;
        _gameSceneBuildIndex = -1;
        _savedActiveStates.Clear();
        _savedMusicClip = null;
    }
}
