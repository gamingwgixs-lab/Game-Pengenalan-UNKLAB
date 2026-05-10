using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log($"[MainMenuManager] PlayGame pressed. HasActiveSession: {SessionManager.HasActiveSession}");
        
        if (SessionManager.HasActiveSession)
        {
            // Lanjutkan sesi game yang sedang di-pause di background
            SessionManager.ContinueGame();
        }
        else
        {
            // Mulai permainan baru
            TransitionManager.Instance.PindahScene(1); 
        }
    }

    public void QuitGame()
    {
        Debug.Log("Game ditutup!"); 
        Application.Quit(); 
    }
}
