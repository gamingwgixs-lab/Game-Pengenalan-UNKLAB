using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manager utama yang mengontrol seluruh alur quiz.
/// Menggunakan pola Singleton yang sama dengan QuestManager, DialogueManager, dan PauseManager.
/// </summary>
public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance;

    [Header("Pengaturan Quiz")]
    [Tooltip("Jika dicentang, urutan soal akan diacak setiap kali quiz dimulai.")]
    public bool acakSoal = false;

    [Header("Audio (Opsional)")]
    [Tooltip("Opsional: SFX saat quiz selesai. Kosongkan jika tidak diperlukan.")]
    public AudioClip quizCompleteSFX;

    [Header("Referensi Player")]
    public PlayerMovement playerMovement;
    public GameObject joystickUI;
    public GameObject hudPanel;

#if UNITY_EDITOR
    [Header("DEBUG")]
    [Tooltip("Assign database di sini dan centang untuk langsung memulai quiz saat Play.")]
    public QuizDatabase debugAutoStartQuiz;
#endif

    // ─── State Internal ───────────────────────────
    private QuizDatabase currentDatabase;
    private List<QuizQuestion> soalAktif;
    private int indexSoalSaatIni;
    private int score;
    private bool isQuizActive = false;
    private bool[] hasilPerSoal;
    private bool bolehSkip = true;

    // ─── Re-Explore Internal ──────────────────────
    private List<string> antreanRuangan = new List<string>();
    private int indexAntrean = 0;
    private bool isReExploreActive = false;

    // ─── Events (Observer Pattern) ────────────────
    /// <summary>Dipanggil saat quiz dimulai.</summary>
    public static System.Action OnQuizStarted;

    /// <summary>Dipanggil saat quiz selesai. Parameter: (score, totalSoal).</summary>
    public static System.Action<int, int> OnQuizCompleted;

    // ─── Properties Publik ────────────────────────
    public bool IsQuizActive => isQuizActive;
    public bool IsReExploreActive => isReExploreActive;
    public int Score => score;
    public int IndexSoalSaatIni => indexSoalSaatIni;
    public int TotalSoal => soalAktif != null ? soalAktif.Count : 0;
    public QuizDatabase CurrentDatabase => currentDatabase;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

#if UNITY_EDITOR
    private void Start()
    {
        if (debugAutoStartQuiz != null)
        {
            MulaiQuiz(debugAutoStartQuiz);
        }
    }
#endif


    // ═══════════════════════════════════════════════
    //  MEMULAI QUIZ
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Memulai sesi quiz baru dari database yang diberikan.
    /// Dipanggil oleh QuizTrigger atau sistem lain.
    /// </summary>
    public void MulaiQuiz(QuizDatabase database, bool allowSkip = true)
    {
        if (database == null || database.JumlahSoal == 0)
        {
            Debug.LogWarning("[QuizManager] Database quiz kosong atau null!");
            return;
        }

        if (isQuizActive)
        {
            Debug.LogWarning("[QuizManager] Quiz sudah sedang berjalan!");
            return;
        }

        // Simpan referensi database
        currentDatabase = database;

        // Siapkan daftar soal (copy agar tidak mengubah asset asli)
        soalAktif = new List<QuizQuestion>(database.daftarSoal);

        // Acak urutan soal jika diaktifkan (Fisher-Yates Shuffle)
        if (acakSoal) AcakDaftarSoal();

        // Reset state
        indexSoalSaatIni = 0;
        score = 0;
        isQuizActive = true;
        hasilPerSoal = new bool[soalAktif.Count];
        bolehSkip = allowSkip;

        // Kunci gameplay (pola yang sama dengan DialogueManager.TampilkanDialog)
        KunciGameplay();

        // Pancarkan event mulai
        OnQuizStarted?.Invoke();

        // Tampilkan soal pertama via UI
        TampilkanSoalSaatIni();

        Debug.Log($"[QuizManager] Quiz dimulai: {database.judulQuiz} ({soalAktif.Count} soal)");
    }

    // ═══════════════════════════════════════════════
    //  MENJAWAB SOAL
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Dipanggil oleh QuizUI saat pemain memilih jawaban.
    /// </summary>
    public void JawabSoal(int indexPilihan)
    {
        if (!isQuizActive || soalAktif == null) return;

        QuizQuestion soal = soalAktif[indexSoalSaatIni];
        bool benar = (indexPilihan == soal.indexJawabanBenar);

        // Catat hasil per soal
        hasilPerSoal[indexSoalSaatIni] = benar;
        if (benar) score++;

        // Langsung lanjut ke soal berikutnya
        indexSoalSaatIni++;

        if (indexSoalSaatIni < soalAktif.Count)
        {
            TampilkanSoalSaatIni();
        }
        else
        {
            SelesaikanQuiz();
        }
    }

    // ═══════════════════════════════════════════════
    //  MENAMPILKAN SOAL
    // ═══════════════════════════════════════════════

    private void TampilkanSoalSaatIni()
    {
        if (QuizUI.Instance == null) return;

        QuizQuestion soal = soalAktif[indexSoalSaatIni];
        QuizUI.Instance.TampilkanSoal(
            soal.pertanyaan,
            soal.pilihan,
            indexSoalSaatIni + 1,   // nomor soal (1-based)
            soalAktif.Count         // total soal
        );
    }

    // ═══════════════════════════════════════════════
    //  MENYELESAIKAN QUIZ
    // ═══════════════════════════════════════════════

    private void SelesaikanQuiz()
    {
        isQuizActive = false;

        // SFX selesai
        if (quizCompleteSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(quizCompleteSFX);

        // Tampilkan panel hasil dengan status per soal
        if (QuizUI.Instance != null)
        {
            QuizUI.Instance.TampilkanHasil(score, soalAktif.Count, hasilPerSoal, bolehSkip);
        }

        Debug.Log($"[QuizManager] Quiz selesai! Skor: {score}/{soalAktif.Count}");
    }

    /// <summary>
    /// Dipanggil oleh QuizUI saat pemain menekan tombol Confirm di panel hasil.
    /// Mengembalikan kontrol gameplay ke pemain.
    /// </summary>
    public void TutupHasil()
    {
        // Sembunyikan UI
        if (QuizUI.Instance != null) QuizUI.Instance.SembunyikanQuiz();

        // Kembalikan kontrol gameplay
        BukaGameplay();

        // Pancarkan event selesai (untuk integrasi dengan QuestManager dll.)
        OnQuizCompleted?.Invoke(score, soalAktif.Count);

        // Bersihkan referensi
        currentDatabase = null;
        soalAktif = null;
        hasilPerSoal = null;

        Debug.Log("[QuizManager] Panel hasil ditutup, kontrol dikembalikan ke pemain.");
    }

    // ═══════════════════════════════════════════════
    //  LOGIKA RE-EXPLORE
    // ═══════════════════════════════════════════════

    public void MulaiReExplore()
    {
        antreanRuangan.Clear();
        indexAntrean = 0;

        // Kumpulkan targetRoomName dari soal yang salah
        for (int i = 0; i < hasilPerSoal.Length; i++)
        {
            if (!hasilPerSoal[i]) // Jika salah
            {
                string target = soalAktif[i].targetRoomName;
                if (!string.IsNullOrEmpty(target) && !antreanRuangan.Contains(target))
                {
                    antreanRuangan.Add(target);
                }
            }
        }

        if (antreanRuangan.Count == 0)
        {
            Debug.Log("[QuizManager] Tidak ada ruangan valid untuk di-explore ulang. Langsung tutup hasil.");
            TutupHasil();
            return;
        }

        isReExploreActive = true;
        
        // Hidupkan kembali semua QuestTrigger GO yang mati agar pemain bisa melihat info ruangan
        ReaktivasiQuestTriggers();

        // Tutup UI Quiz, biarkan HUD hilang (gameplay terkunci sebagian)
        if (QuizUI.Instance != null)
        {
            QuizUI.Instance.SembunyikanQuiz();
            QuizUI.Instance.TampilkanTombolNextExplore(true); // Munculkan tombol Next UI
        }

        // Mulai warp pertama
        WarpKeAntreanSekarang();
    }

    /// <summary>
    /// Menghidupkan kembali semua GameObject yang memiliki QuestTrigger (termasuk yang sudah mati)
    /// agar pemain bisa melihat info ruangan saat Re-Explore.
    /// </summary>
    private void ReaktivasiQuestTriggers()
    {
        QuestTrigger[] allTriggers = FindObjectsByType<QuestTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (QuestTrigger qt in allTriggers)
        {
            if (!qt.gameObject.activeSelf)
            {
                qt.gameObject.SetActive(true);
            }
        }
    }

    public void LanjutReExplore()
    {
        if (!isReExploreActive) return;

        indexAntrean++;

        if (indexAntrean < antreanRuangan.Count)
        {
            // Pindah ke ruangan berikutnya
            WarpKeAntreanSekarang();
        }
        else
        {
            // Selesai re-explore, restart quiz
            isReExploreActive = false;
            
            if (QuizUI.Instance != null)
            {
                QuizUI.Instance.TampilkanTombolNextExplore(false);
            }

            Debug.Log("[QuizManager] Selesai re-explore. Mengulang quiz...");
            
            // Re-start quiz dengan database yang sama
            QuizDatabase dbTemp = currentDatabase;
            currentDatabase = null; 
            isQuizActive = false;
            MulaiQuiz(dbTemp);
        }
    }

    private void WarpKeAntreanSekarang()
    {
        if (indexAntrean < 0 || indexAntrean >= antreanRuangan.Count) return;

        string targetRoom = antreanRuangan[indexAntrean];
        Debug.Log($"[QuizManager] Re-Explore: Pindah ke {targetRoom}");

        // Pindahkan pemain (pastikan BukaGameplay agar pemain bisa jalan di ruangan)
        BukaGameplay();
        
        if (WarpManager.Instance != null)
        {
            WarpManager.Instance.WarpByRoomName(targetRoom);
        }
    }

    // ═══════════════════════════════════════════════
    //  GAMEPLAY LOCK/UNLOCK
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Mengunci pergerakan dan menyembunyikan HUD gameplay.
    /// Pola yang identik dengan DialogueManager.TampilkanDialog().
    /// </summary>
    private void KunciGameplay()
    {
        if (playerMovement != null) playerMovement.SetMoveState(false);
        if (joystickUI != null) joystickUI.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (QuestUI.Instance != null) QuestUI.Instance.HideQuestPanel();

        // Sembunyikan WarpUI (tombol "Masuk") dan Place Info (nama lokasi)
        if (WarpManager.Instance != null && WarpManager.Instance.warpUI != null)
            WarpManager.Instance.warpUI.SetActive(false);
        if (DialogueManager.Instance != null && DialogueManager.Instance.placeInfoUI != null)
            DialogueManager.Instance.placeInfoUI.SetActive(false);
    }

    /// <summary>
    /// Mengembalikan kontrol pergerakan dan HUD gameplay.
    /// Pola yang identik dengan DialogueManager.TutupDialog().
    /// </summary>
    private void BukaGameplay()
    {
        if (playerMovement != null) playerMovement.SetMoveState(true);
        if (joystickUI != null) joystickUI.SetActive(true);
        if (hudPanel != null) hudPanel.SetActive(true);

        // Tampilkan kembali Place Info (WarpUI tidak perlu karena dia muncul otomatis saat dekat pintu)
        if (DialogueManager.Instance != null && DialogueManager.Instance.placeInfoUI != null)
            DialogueManager.Instance.placeInfoUI.SetActive(true);
    }

    // ═══════════════════════════════════════════════
    //  UTILITAS
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Fisher-Yates Shuffle — mengacak urutan soal secara efisien O(n).
    /// </summary>
    private void AcakDaftarSoal()
    {
        for (int i = soalAktif.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            QuizQuestion temp = soalAktif[i];
            soalAktif[i] = soalAktif[j];
            soalAktif[j] = temp;
        }
    }
}
