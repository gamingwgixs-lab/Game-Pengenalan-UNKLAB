using UnityEngine;

/// <summary>
/// Komponen trigger untuk memulai quiz di dunia game.
/// Mendukung dua mode: Proximity (OnTriggerEnter) dan Manual (dipanggil via tombol).
/// Mengikuti pola QuestTrigger dan ProximityTrigger yang sudah ada.
/// </summary>
public class QuizTrigger : MonoBehaviour
{
    [Header("Data Quiz")]
    [Tooltip("Database soal yang akan dimainkan saat trigger aktif.")]
    public QuizDatabase quizDatabase;

    [Header("Pengaturan Trigger")]
    [Tooltip("Jika dicentang, trigger hanya bisa digunakan sekali.")]
    public bool sekaliPakai = false;

    [Tooltip("Jika dicentang, quiz akan dimulai otomatis saat player masuk area collider.")]
    public bool autoTrigger = true;

    [Tooltip("Jika dicentang, tombol Skip akan muncul saat pemain gagal. Jika tidak, hanya Re-Explore yang tersedia.")]
    public bool tampilkanTombolSkip = true;

    [Header("Integrasi Quest (Opsional)")]
    [Tooltip("Jika diisi, quiz yang selesai dengan skor >= minScoreLulus akan menyelesaikan POI quest ini.")]
    public string poiNameUntukQuest;

    [Tooltip("Skor minimum untuk dianggap lulus dan memicu penyelesaian quest.")]
    public int minScoreLulus = 0;

    [Tooltip("Jika dicentang, menyelesaikan kuis akan memicu penyelesaian quest (berapapun skor akhirnya). Gunakan ini untuk kuis penutup/terakhir.")]
    public bool abaikanSkorUntukQuest = false;

    [Header("Game Ending Sequence (Khusus Kuis Penutup)")]
    [Tooltip("Jika dicentang, menyelesaikan kuis ini akan memutar dialog khusus dan menamatkan game secara langsung tanpa lewat QuestManager.")]
    public bool isGameEndingQuiz = false;
    public string namaNPCEnding = "System";
    public Sprite portraitEnding;
    public TipeVisualNPC tipeEnding = TipeVisualNPC.System;
    [TextArea(2,5)]
    public string[] dialogEnding = new string[] { "Terima kasih telah bermain!" };

    [Header("Visual Sistem (Opsional)")]
    [Tooltip("Opsional: Gambar yang ditampilkan di System Dialogue saat quest selesai via quiz.")]
    public Sprite gambarLokasi;

    private bool sudahDigunakan = false;

    private void OnEnable()
    {
        // Berlangganan event quiz selesai untuk integrasi quest
        QuizManager.OnQuizCompleted += OnQuizSelesai;
    }

    private void OnDisable()
    {
        // Unsubscribe untuk mencegah memory leak
        QuizManager.OnQuizCompleted -= OnQuizSelesai;
    }

    // ═══════════════════════════════════════════════
    //  PROXIMITY TRIGGER
    // ═══════════════════════════════════════════════

    private void OnTriggerEnter(Collider other)
    {
        if (!autoTrigger) return;
        if (other.CompareTag("Player"))
        {
            // Jika poiNameUntukQuest diisi, quiz hanya aktif saat quest yang tepat sedang berjalan
            if (!string.IsNullOrEmpty(poiNameUntukQuest) && QuestManager.Instance != null)
            {
                if (QuestManager.Instance.activeQuest == null || 
                    QuestManager.Instance.activeQuest.poiTarget != poiNameUntukQuest)
                {
                    return; // Quest belum aktif atau bukan quest yang tepat, abaikan
                }
            }

            // Jika ini kuis penutup, cegah QuestTrigger di objek yang sama agar tidak tabrakan
            if (isGameEndingQuiz)
            {
                QuestTrigger qt = GetComponent<QuestTrigger>();
                if (qt != null) qt.enabled = false;

                // Block QuestManager dari trigger ending sendiri
                if (QuestManager.Instance != null)
                    QuestManager.Instance.blockGameEnding = true;
            }

            MulaiQuiz();
        }
    }

    // ═══════════════════════════════════════════════
    //  MANUAL TRIGGER (untuk dipanggil via Button/Event)
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Memulai quiz secara manual. Bisa dipanggil dari UnityEvent pada Button.
    /// </summary>
    public void MulaiQuiz()
    {
        if (sudahDigunakan && sekaliPakai) return;
        if (quizDatabase == null)
        {
            Debug.LogWarning("[QuizTrigger] QuizDatabase belum di-assign!");
            return;
        }

        if (QuizManager.Instance == null)
        {
            Debug.LogError("[QuizTrigger] QuizManager.Instance tidak ditemukan di scene!");
            return;
        }

        if (QuizManager.Instance.IsQuizActive)
        {
            Debug.LogWarning("[QuizTrigger] Quiz sudah sedang berjalan!");
            return;
        }

        QuizManager.Instance.MulaiQuiz(quizDatabase, tampilkanTombolSkip);
    }

    // ═══════════════════════════════════════════════
    //  INTEGRASI QUEST & GAME ENDING
    // ═══════════════════════════════════════════════

    private void OnQuizSelesai(int score, int totalSoal)
    {
        // Pastikan hanya trigger ini yang memproses jika database-nya cocok
        if (QuizManager.Instance != null && QuizManager.Instance.CurrentDatabase != this.quizDatabase) return;

        // Tandai sebagai sudah digunakan
        if (sekaliPakai) sudahDigunakan = true;

        if (isGameEndingQuiz)
        {
            // ═══ MODE GAME ENDING ═══
            // 1. Selesaikan quest secara senyap (tanpa dialog quest, tanpa GameEnding otomatis)
            if (!string.IsNullOrEmpty(poiNameUntukQuest) && QuestManager.Instance != null)
            {
                QuestManager.Instance.SelesaikanQuestSenyap(poiNameUntukQuest);
            }

            // 2. Tampilkan dialog penutup dari QuizTrigger ini
            if (DialogueManager.Instance != null && dialogEnding != null && dialogEnding.Length > 0)
            {
                DialogueManager.Instance.TampilkanDialog(namaNPCEnding, portraitEnding, dialogEnding, tipeEnding, true);
            }

            // 3. Trigger Game Ending (menunggu dialog selesai, lalu fade → End Credit)
            if (GameEndingManager.Instance != null)
            {
                GameEndingManager.Instance.TriggerGameEndingSequence();
            }
        }
        else
        {
            // ═══ MODE NORMAL ═══
            if (!string.IsNullOrEmpty(poiNameUntukQuest) && (score >= minScoreLulus || abaikanSkorUntukQuest))
            {
                if (QuestManager.Instance != null)
                {
                    QuestManager.Instance.CekPOI(poiNameUntukQuest, gambarLokasi);
                }
            }
        }

        // Matikan trigger jika sekali pakai
        if (sekaliPakai)
        {
            gameObject.SetActive(false);
        }
    }
}
