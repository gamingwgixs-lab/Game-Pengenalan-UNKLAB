using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndingManager : MonoBehaviour
{
    public static GameEndingManager Instance;

    [Header("Settings")]
    public int endCreditSceneIndex = 2; // Index scene End Credit
    public float transitionDelay = 1f; // Jeda sebelum pindah scene setelah dialog selesai

    private bool isEndingSequenceActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Dipanggil oleh QuestManager saat quest terakhir berhasil diselesaikan.
    /// Ini akan menyiapkan sistem untuk pindah ke EndCredit saat dialog terakhir selesai.
    /// </summary>
    public void TriggerGameEndingSequence()
    {
        if (isEndingSequenceActive) return;

        isEndingSequenceActive = true;
        
        // Daftarkan listener ke DialogueManager untuk menunggu dialog benar-benar selesai
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueComplete.AddListener(OnFinalDialogueFinished);
        }
        else
        {
            // Jika tidak ada dialogue manager, langsung tamat
            StartCoroutine(TransitionToEndCredit());
        }
    }

    private void OnFinalDialogueFinished()
    {
        // Unsubscribe agar tidak dipanggil dua kali
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueComplete.RemoveListener(OnFinalDialogueFinished);

            // Sembunyikan UI gameplay (joystick, placeInfo) tapi JANGAN hudPanel
            // karena panel transisi fade (panelHitam) mungkin berada di dalam hierarchy-nya
            DialogueManager.Instance.ToggleGameplayUI(false);
        }

        // Sembunyikan Quest UI
        if (QuestUI.Instance != null)
        {
            QuestUI.Instance.HideQuestPanel();
        }

        StartCoroutine(TransitionToEndCredit());
    }

    private IEnumerator TransitionToEndCredit()
    {
        // Kunci pergerakan player 
        if (FindAnyObjectByType<PlayerMovement>() != null)
        {
            FindAnyObjectByType<PlayerMovement>().SetMoveState(false);
        }

        // Sembunyikan HUD (tombol pause, dll.) setelah jeda singkat
        // agar tidak terlihat, tapi sebelum fade dimulai
        if (DialogueManager.Instance != null && DialogueManager.Instance.hudPanel != null)
        {
            DialogueManager.Instance.hudPanel.SetActive(false);
        }

        // Tunggu sebentar (opsional, agar tidak kaget/terlalu instan)
        yield return new WaitForSeconds(transitionDelay);

        // Fade to black → Load End Credit scene
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.PindahScene(endCreditSceneIndex);
        }
        else
        {
            // Fallback jika TransitionManager tidak ada
            SceneManager.LoadScene(endCreditSceneIndex);
        }
    }
}