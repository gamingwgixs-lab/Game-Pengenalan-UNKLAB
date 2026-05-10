using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    public void Button_Resume()
    {
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.ResumeGame();
        }
    }

    public void Button_Restart()
    {
        // Reset time scale agar proses transisi antar scene berjalan dengan kecepatan normal
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Button_MainMenu()
    {
        // Transisi ke Main Menu menggunakan Additive Loading, mempertahankan state scene saat ini di memori agar dapat dilanjutkan.
        SessionManager.ReturnToMainMenu();
    }

    public void Button_Exit()
    {
        Debug.Log("Exiting Game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
