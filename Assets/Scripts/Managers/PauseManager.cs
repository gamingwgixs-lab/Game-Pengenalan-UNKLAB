using UnityEngine;
using UnityEngine.Events;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuPanel;
    
    [Header("Events")]
    public UnityEvent OnGamePaused;
    public UnityEvent OnGameResumed;

    private bool isPaused = false;
    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Pastikan menu pause tertutup saat mulai
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Menghentikan waktu fisika & animasi standar
        
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);

        // Matikan Gameplay lewat DialogueManager yang sudah kita buat tadi
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ToggleGameplayUI(false);
        }

        OnGamePaused?.Invoke();
        Debug.Log("[PauseManager] Game Paused.");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Jalankan waktu kembali
        
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        // Aktifkan kembali Gameplay (Hanya jika tidak sedang berdialog)
        // Kita butuh sedikit logika tambahan di DialogueManager untuk mengecek ini
        if (DialogueManager.Instance != null)
        {
            // Jika dialog sedang aktif, biarkan Gameplay UI tetap mati
            bool isDialogueActive = DialogueManager.Instance.rootDialogueUI.activeSelf;
            if (!isDialogueActive)
            {
                DialogueManager.Instance.ToggleGameplayUI(true);
            }
        }

        OnGameResumed?.Invoke();
        Debug.Log("[PauseManager] Game Resumed.");
    }
}
