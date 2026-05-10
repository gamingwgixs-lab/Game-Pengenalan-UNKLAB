using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller UI untuk sistem Quiz.
/// Menghubungkan elemen-elemen UI prototype (tracker, pertanyaan, tombol jawaban, panel hasil)
/// dengan logic di QuizManager.
/// Mengikuti pola QuestUI (Singleton + referensi via Inspector).
/// </summary>
public class QuizUI : MonoBehaviour
{
    public static QuizUI Instance;

    [Header("Panel Utama Quiz")]
    [Tooltip("Parent container seluruh quiz UI. Diaktifkan/dimatikan untuk show/hide.")]
    public GameObject quizPanel;

    [Header("Tracking Soal")]
    [Tooltip("Text untuk menampilkan progress soal, misal '1/10'.")]
    public TextMeshProUGUI trackerText;

    [Header("Pertanyaan")]
    [Tooltip("Text untuk menampilkan isi pertanyaan.")]
    public TextMeshProUGUI pertanyaanText;

    [Header("Tombol Pilihan Jawaban")]
    [Tooltip("3 tombol pilihan jawaban. Urutan harus sesuai dengan index pilihan di QuizDatabase.")]
    public Button[] tombolPilihan = new Button[3];

    [Tooltip("Text di dalam masing-masing tombol pilihan.")]
    public TextMeshProUGUI[] teksPilihan = new TextMeshProUGUI[3];

    [Header("Panel Hasil Akhir")]
    [Tooltip("Panel yang muncul setelah semua soal dijawab.")]
    public GameObject hasilPanel;

    [Tooltip("Text judul 'Hasil'.")]
    public TextMeshProUGUI hasilJudulText;

    [Tooltip("Image background di belakang judul 'Hasil'.")]
    public Image hasilJudulImage;

    [Header("Pengaturan Lulus / Tidak Lulus Keseluruhan")]
    [Tooltip("Batas skor minimal untuk dianggap lulus keseluruhan.")]
    public int minScorePassed = 8;

    [Tooltip("Teks judul saat lulus keseluruhan.")]
    public string teksJudulLulus = "Lulus";
    [Tooltip("Teks judul saat tidak lulus keseluruhan.")]
    public string teksJudulTidakLulus = "Tidak Lulus";

    [Tooltip("Warna background judul saat lulus keseluruhan.")]
    public Color warnaBgJudulLulus = new Color(0.2f, 0.6f, 0.2f, 1f);
    [Tooltip("Warna background judul saat tidak lulus keseluruhan.")]
    public Color warnaBgJudulTidakLulus = new Color(0.8f, 0.2f, 0.2f, 1f);

    [Tooltip("Text untuk menampilkan skor.")]
    public TextMeshProUGUI hasilScoreText;

    [Tooltip("Format teks skor. Gunakan {0} untuk skor pemain, dan {1} untuk total soal.")]
    public string formatTeksScore = "Score {0}/{1}";

    [Tooltip("Image background di belakang skor (mengikuti warna judul kelulusan).")]
    public Image hasilScoreImage;

    [Tooltip("10 text element untuk status per soal (Passed/Not Passed). Urutan sesuai nomor soal.")]
    public TextMeshProUGUI[] hasilPerSoalText = new TextMeshProUGUI[10];

    [Tooltip("10 Image background untuk setiap baris hasil soal. Warnanya berubah sesuai status.")]
    public Image[] hasilPerSoalImage = new Image[10];

    [Tooltip("Tombol Confirm untuk menutup panel hasil.")]
    public Button tombolConfirm;

    [Tooltip("Tombol Re-Explore (hanya muncul saat gagal).")]
    public Button tombolReExplore;

    [Tooltip("Tombol Next untuk memindahkan pemain ke ruangan salah berikutnya selama mode Re-Explore.")]
    public GameObject tombolNextExplore;

    [Header("Warna Status Hasil - Background Image")]
    [Tooltip("Warna background Image saat Passed.")]
    public Color warnaPassed = new Color(0.2f, 0.8f, 0.2f, 1f);   // Hijau
    [Tooltip("Warna background Image saat Not Passed.")]
    public Color warnaNotPassed = new Color(0.9f, 0.2f, 0.2f, 1f); // Merah

    [Header("Warna Status Hasil - Teks")]
    [Tooltip("Warna teks saat Passed.")]
    public Color warnaTextPassed = Color.white;
    [Tooltip("Warna teks saat Not Passed.")]
    public Color warnaTextNotPassed = Color.white;

    [Header("Label Status Hasil")]
    [Tooltip("Teks yang ditampilkan saat jawaban benar.")]
    public string labelPassed = "Passed";
    [Tooltip("Teks yang ditampilkan saat jawaban salah.")]
    public string labelNotPassed = "Not Passed";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Setup listener tombol pilihan
        for (int i = 0; i < tombolPilihan.Length; i++)
        {
            if (tombolPilihan[i] != null)
            {
                int index = i; // Capture untuk closure
                tombolPilihan[i].onClick.AddListener(() => OnPilihanDitekan(index));
            }
        }

        // Setup listener tombol Confirm
        if (tombolConfirm != null)
        {
            tombolConfirm.onClick.AddListener(OnTombolConfirmDitekan);
        }
        if (tombolReExplore != null)
        {
            tombolReExplore.onClick.RemoveAllListeners();
            tombolReExplore.onClick.AddListener(() => QuizManager.Instance.MulaiReExplore());
        }

        // Setup listener tombol Next Explore (di UI)
        Button btnNext = tombolNextExplore != null ? tombolNextExplore.GetComponent<Button>() : null;
        if (btnNext != null)
        {
            btnNext.onClick.RemoveAllListeners();
            btnNext.onClick.AddListener(() => QuizManager.Instance.LanjutReExplore());
        }
        // Sembunyikan semua panel saat mulai
        SembunyikanQuiz();
    }

    // ═══════════════════════════════════════════════
    //  MENAMPILKAN SOAL
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Menampilkan satu soal quiz beserta pilihan jawabannya.
    /// Dipanggil oleh QuizManager.
    /// </summary>
    public void TampilkanSoal(string pertanyaan, string[] pilihan, int nomorSoal, int totalSoal)
    {
        if (quizPanel != null) quizPanel.SetActive(true);
        if (hasilPanel != null) hasilPanel.SetActive(false);

        // Update tracker (misal: "1/10")
        if (trackerText != null)
            trackerText.text = $"{nomorSoal}/{totalSoal}";

        // Update pertanyaan
        if (pertanyaanText != null)
            pertanyaanText.text = pertanyaan;

        // Label prefix untuk setiap pilihan
        string[] label = { "A. ", "B. ", "C. " };

        // Update teks pilihan
        for (int i = 0; i < tombolPilihan.Length; i++)
        {
            if (i < pilihan.Length)
            {
                if (tombolPilihan[i] != null)
                {
                    tombolPilihan[i].gameObject.SetActive(true);
                    tombolPilihan[i].interactable = true;
                }

                if (teksPilihan[i] != null)
                    teksPilihan[i].text = label[i] + pilihan[i];
            }
            else
            {
                if (tombolPilihan[i] != null)
                    tombolPilihan[i].gameObject.SetActive(false);
            }
        }
    }

    // ═══════════════════════════════════════════════
    //  PANEL HASIL
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Menampilkan panel hasil akhir quiz.
    /// Menunjukkan status Passed/Not Passed per soal dan skor total.
    /// </summary>
    public void TampilkanHasil(int score, int totalSoal, bool[] hasilPerSoal, bool bolehSkip = true)
    {
        // Sembunyikan panel soal, tampilkan panel hasil
        if (quizPanel != null) quizPanel.SetActive(false);
        if (hasilPanel != null) hasilPanel.SetActive(true);

        // Evaluasi apakah lulus keseluruhan
        bool isLulus = score >= minScorePassed;

        // Tampilkan/sembunyikan tombol Re-Explore dan Confirm(Next/Skip)
        if (tombolReExplore != null)
        {
            tombolReExplore.gameObject.SetActive(!isLulus); // Muncul jika tidak lulus
        }
        
        if (tombolConfirm != null)
        {
            // Tombol muncul jika: lulus (Next) ATAU bolehSkip dicentang (Skip)
            bool tampilConfirm = isLulus || bolehSkip;
            tombolConfirm.gameObject.SetActive(tampilConfirm);
            
            // Ubah teks tombol menjadi Next (jika lulus) atau Skip (jika gagal)
            TextMeshProUGUI teksTombol = tombolConfirm.GetComponentInChildren<TextMeshProUGUI>();
            if (teksTombol != null)
            {
                teksTombol.text = isLulus ? "Next" : "Skip";
            }
        }

        // Update Judul Text & Judul Background berdasarkan status lulus
        if (hasilJudulText != null)
        {
            hasilJudulText.text = isLulus ? teksJudulLulus : teksJudulTidakLulus;
        }

        if (hasilJudulImage != null)
        {
            hasilJudulImage.color = isLulus ? warnaBgJudulLulus : warnaBgJudulTidakLulus;
        }

        if (hasilScoreImage != null)
        {
            hasilScoreImage.color = isLulus ? warnaBgJudulLulus : warnaBgJudulTidakLulus;
        }

        // Update skor
        if (hasilScoreText != null)
            hasilScoreText.text = string.Format(formatTeksScore, score, totalSoal);

        // Update status per soal (teks + warna background Image)
        UpdateStatusPerSoal(hasilPerSoal);
    }

    /// <summary>
    /// Mengupdate tampilan status per soal: teks dan warna background Image.
    /// </summary>
    private void UpdateStatusPerSoal(bool[] hasilPerSoal)
    {
        for (int i = 0; i < hasilPerSoalText.Length; i++)
        {
            if (i < hasilPerSoal.Length)
            {
                // Aktifkan elemen
                if (hasilPerSoalText[i] != null)
                    hasilPerSoalText[i].gameObject.SetActive(true);

                bool passed = hasilPerSoal[i];

                // Update teks dan warna teks
                if (hasilPerSoalText[i] != null)
                {
                    hasilPerSoalText[i].text = passed ? labelPassed : labelNotPassed;
                    hasilPerSoalText[i].color = passed ? warnaTextPassed : warnaTextNotPassed;
                }

                // Update warna background Image
                if (i < hasilPerSoalImage.Length && hasilPerSoalImage[i] != null)
                    hasilPerSoalImage[i].color = passed ? warnaPassed : warnaNotPassed;
            }
            else
            {
                // Sembunyikan jika soal kurang dari 10
                if (hasilPerSoalText[i] != null)
                    hasilPerSoalText[i].gameObject.SetActive(false);
                if (i < hasilPerSoalImage.Length && hasilPerSoalImage[i] != null)
                    hasilPerSoalImage[i].gameObject.SetActive(false);
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// [EDITOR ONLY] Preview panel hasil tanpa perlu Play.
    /// Dipanggil dari QuizUIEditor.
    /// </summary>
    public void PreviewHasil(bool[] previewData)
    {
        if (hasilPanel != null) hasilPanel.SetActive(true);
        if (quizPanel != null) quizPanel.SetActive(false);

        int previewScore = 0;
        foreach (bool b in previewData) if (b) previewScore++;

        bool isLulus = previewScore >= minScorePassed;

        if (tombolReExplore != null)
        {
            tombolReExplore.gameObject.SetActive(!isLulus);
        }

        if (tombolConfirm != null)
        {
            tombolConfirm.gameObject.SetActive(true);
            TextMeshProUGUI teksTombol = tombolConfirm.GetComponentInChildren<TextMeshProUGUI>();
            if (teksTombol != null)
            {
                teksTombol.text = isLulus ? "Next" : "Skip";
            }
        }

        if (hasilJudulText != null)
        {
            hasilJudulText.text = isLulus ? teksJudulLulus : teksJudulTidakLulus;
        }

        if (hasilJudulImage != null)
        {
            hasilJudulImage.color = isLulus ? warnaBgJudulLulus : warnaBgJudulTidakLulus;
        }

        if (hasilScoreImage != null)
        {
            hasilScoreImage.color = isLulus ? warnaBgJudulLulus : warnaBgJudulTidakLulus;
        }

        if (hasilScoreText != null)
            hasilScoreText.text = string.Format(formatTeksScore, previewScore, previewData.Length);

        UpdateStatusPerSoal(previewData);
    }

    /// <summary>
    /// [EDITOR ONLY] Menyembunyikan preview panel hasil.
    /// </summary>
    public void HidePreview()
    {
        if (hasilPanel != null) hasilPanel.SetActive(false);
    }
#endif

    // ═══════════════════════════════════════════════
    //  SHOW / HIDE
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Menyembunyikan seluruh UI quiz (panel soal dan panel hasil).
    /// </summary>
    public void SembunyikanQuiz()
    {
        if (quizPanel != null) quizPanel.SetActive(false);
        if (hasilPanel != null) hasilPanel.SetActive(false);
    }

    // ═══════════════════════════════════════════════
    //  CALLBACK TOMBOL
    // ═══════════════════════════════════════════════

    private void OnPilihanDitekan(int index)
    {
        if (QuizManager.Instance != null)
        {
            QuizManager.Instance.JawabSoal(index);
        }
    }

    private void OnTombolConfirmDitekan()
    {
        if (QuizManager.Instance != null)
        {
            QuizManager.Instance.TutupHasil();
        }
    }

    // ═══════════════════════════════════════════════
    //  FUNGSI BANTUAN UNTUK RE-EXPLORE
    // ═══════════════════════════════════════════════

    public void TampilkanTombolNextExplore(bool tampil)
    {
        if (tombolNextExplore != null)
        {
            tombolNextExplore.SetActive(tampil);
        }
    }
}
