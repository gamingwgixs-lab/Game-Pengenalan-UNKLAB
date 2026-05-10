using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public enum TipeVisualNPC { Pemandu, Dosen, System }

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("Events")]
    public UnityEvent OnDialogueStart;
    public UnityEvent OnSystemDialogueStart;
    public UnityEvent OnDialogueComplete;

    [Header("Pengaturan Player")]
    public PlayerMovement playerMovement;
    public GameObject joystickUI;
    public GameObject placeInfoUI;
    public GameObject hudPanel; // Referensi wadah tombol Pause dsb.

    [Header("Kontainer Jalur NPC")]
    [Tooltip("Objek Parent Utama untuk NPC (Dialogue UI).")]
    public GameObject rootDialogueUI;
    [Tooltip("Wadah khusus untuk foto Pemandu.")]
    public GameObject wadahPemandu;
    [Tooltip("Wadah khusus untuk foto Dosen.")]
    public GameObject wadahDosen;
    [Tooltip("Background Kotak Teks untuk NPC.")]
    public GameObject panelTeksNpc;

    [Header("Kontainer Jalur System")]
    [Tooltip("Objek Parent Utama untuk Pesan Sistem (System Dialogue).")]
    public GameObject panelSystem;

    [Header("Referensi Image NPC")]
    public Image fotoPemandu;
    public Image fotoDosen;

    [Header("Referensi Wadah System")]
    [Tooltip("Objek Parent 'Image 1' (berisi Background & Placeholder foto 1).")]
    public GameObject wadahFotoSystem;
    [Tooltip("Objek Parent 'Image 2' (berisi Background & Placeholder foto 2).")]
    public GameObject wadahFotoSystem2;
    [Tooltip("Komponen Image Placeholder foto 1 di dalam wadah.")]
    public Image fotoSystem;
    [Tooltip("Komponen Image Placeholder foto 2 di dalam wadah.")]
    public Image fotoSystem2;

    [Header("Referensi Teks")]
    public TextMeshProUGUI namaNpc;
    public TextMeshProUGUI teksNpc;
    public TextMeshProUGUI teksSystem;

    [Header("Pengaturan Ketikan")]
    public float kecepatanKetik = 0.04f;
    public AudioClip typingSFX; // Suara beep saat ngetik

    [Header("Pengaturan Navigasi")]
    public bool globalCanSkip = true; 
    private bool currentCanSkip;
    private float inputCooldown = 0.2f;
    private float nextInputTime;

    private string[] paragrafSaatIni;
    private int indexParagraf;
    private bool isTyping;
    private Coroutine typingCoroutine;
    
    private TextMeshProUGUI teksAktif;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ForceCloseAllUI();
    }

    // private void Update()
    // {
    //     // Cek apakah ada UI dialog yang aktif (baik NPC maupun System)
    //     bool isAnyDialogueActive = (rootDialogueUI != null && rootDialogueUI.activeSelf) || 
    //                                (panelSystem != null && panelSystem.activeSelf);

    //     // if (isAnyDialogueActive && Input.GetKeyDown(KeyCode.E) && Time.time >= nextInputTime)
    //     // {
    //     //     LanjutAtauSkip();
    //     // }
    // }

    public void TampilkanDialog(string nama, Sprite foto, string[] paragraf, TipeVisualNPC tipe = TipeVisualNPC.Pemandu, bool canSkip = true, Sprite foto2 = null)
    {
        currentCanSkip = canSkip && globalCanSkip;
        paragrafSaatIni = paragraf;
        indexParagraf = 0;
        nextInputTime = Time.time + inputCooldown;

        if (playerMovement != null) playerMovement.SetMoveState(false);
        if (joystickUI != null) joystickUI.SetActive(false);
        if (placeInfoUI != null) placeInfoUI.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false); 
        // 3. SAPU BERSIH: Matikan SEMUA jalur UI terlebih dahulu
        ForceCloseAllUI();

        // Pancarkan sinyal bahwa Dialog sudah dimulai
        OnDialogueStart?.Invoke();

        // 4. Pilih Jalur Visual (NPC vs SYSTEM)
        if (tipe == TipeVisualNPC.System || (string.IsNullOrEmpty(nama) && foto == null))
        {
            // --- JALUR SYSTEM ---
            if (panelSystem != null) panelSystem.SetActive(true);
            teksAktif = teksSystem;

            // Pancarkan sinyal khusus untuk System Dialogue
            OnSystemDialogueStart?.Invoke();
            
            // Handle foto 1 opsional di System Dialogue
            if (wadahFotoSystem != null)
            {
                if (foto != null)
                {
                    wadahFotoSystem.SetActive(true);
                    if (fotoSystem != null) fotoSystem.sprite = foto;
                }
                else
                {
                    wadahFotoSystem.SetActive(false);
                }
            }

            // Handle foto 2 opsional di System Dialogue
            if (wadahFotoSystem2 != null)
            {
                if (foto2 != null)
                {
                    wadahFotoSystem2.SetActive(true);
                    if (fotoSystem2 != null) fotoSystem2.sprite = foto2;
                }
                else
                {
                    wadahFotoSystem2.SetActive(false);
                }
            }
        }
        else
        {
            // --- JALUR NPC ---
            if (rootDialogueUI != null) rootDialogueUI.SetActive(true);
            if (panelTeksNpc != null) panelTeksNpc.SetActive(true);
            teksAktif = teksNpc;

            if (namaNpc != null) namaNpc.text = nama;

            // Pilih Wadah Foto sesuai tipe
            if (tipe == TipeVisualNPC.Dosen)
            {
                if (wadahDosen != null) wadahDosen.SetActive(true);
                if (fotoDosen != null) fotoDosen.sprite = foto;
                if (wadahPemandu != null) wadahPemandu.SetActive(false);
            }
            else
            {
                if (wadahPemandu != null) wadahPemandu.SetActive(true);
                if (fotoPemandu != null) fotoPemandu.sprite = foto;
                if (wadahDosen != null) wadahDosen.SetActive(false);
            }
        }

        MulaiKetik();
    }

    public void LanjutAtauSkip()
    {
        if (Time.time < nextInputTime) return;
        nextInputTime = Time.time + inputCooldown;

        if (isTyping)
        {
            if (!currentCanSkip) return;
            StopTyping();
            if (teksAktif != null) teksAktif.text = paragrafSaatIni[indexParagraf];
            isTyping = false;
        }
        else
        {
            indexParagraf++;
            CheckNextAvailableDialogue();
        }
    }

    /// <summary>
    /// Memeriksa apakah masih ada paragraf dialog yang tersedia untuk ditampilkan.
    /// Jika ada, mulai mengetik paragraf tersebut. Jika habis, tutup obrolan.
    /// </summary>
    private void CheckNextAvailableDialogue()
    {
        if (indexParagraf < paragrafSaatIni.Length)
        {
            MulaiKetik();
        }
        else
        {
            TutupDialog();
        }
    }

    private void MulaiKetik()
    {
        StopTyping();
        typingCoroutine = StartCoroutine(KetikHuruf(paragrafSaatIni[indexParagraf]));
    }

    private IEnumerator KetikHuruf(string kalimat)
    {
        isTyping = true;
        if (teksAktif != null) teksAktif.text = "";

        foreach (char huruf in kalimat.ToCharArray())
        {
            if (teksAktif != null) teksAktif.text += huruf;
            
            // Putar SFX Ketikan
            if (typingSFX != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(typingSFX, 0.5f);
            }

            yield return new WaitForSeconds(kecepatanKetik);
        }
        isTyping = false;
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    private void ForceCloseAllUI()
    {
        // Matikan Jalur NPC
        if (rootDialogueUI != null) rootDialogueUI.SetActive(false);
        if (wadahPemandu != null) wadahPemandu.SetActive(false);
        if (wadahDosen != null) wadahDosen.SetActive(false);
        if (panelTeksNpc != null) panelTeksNpc.SetActive(false);

        // Matikan Jalur System (GameObject Terpisah Anda)
        if (panelSystem != null) panelSystem.SetActive(false);
        if (wadahFotoSystem != null) wadahFotoSystem.SetActive(false);
        if (wadahFotoSystem2 != null) wadahFotoSystem2.SetActive(false);
        
        // Reset Teks
        if (namaNpc != null) namaNpc.text = "";
        if (teksNpc != null) teksNpc.text = "";
        if (teksSystem != null) teksSystem.text = "";
    }

    public void ToggleGameplayUI(bool active)
    {
        if (playerMovement != null) playerMovement.SetMoveState(active);
        if (joystickUI != null) joystickUI.SetActive(active);
        if (placeInfoUI != null) placeInfoUI.SetActive(active);
    }

    private void TutupDialog()
    {
        ForceCloseAllUI();
        
        // Kembalikan Kontrol Player
        if (playerMovement != null) playerMovement.SetMoveState(true);
        if (joystickUI != null) joystickUI.SetActive(true);
        if (placeInfoUI != null) placeInfoUI.SetActive(true);
        if (hudPanel != null) hudPanel.SetActive(true); // Munculkan kembali tombol Pause dsb.
        
        OnDialogueComplete?.Invoke();
    }

    /// <summary>
    /// Dipanggil oleh sistem Debug untuk menutup paksa dialog yang sedang berjalan.
    /// Mengembalikan kontrol player dan membersihkan semua UI.
    /// </summary>
    public void ForceCloseFromDebug()
    {
        StopTyping();
        ForceCloseAllUI();

        // Kembalikan Kontrol Player
        if (playerMovement != null) playerMovement.SetMoveState(true);
        if (joystickUI != null) joystickUI.SetActive(true);
        if (placeInfoUI != null) placeInfoUI.SetActive(true);
        if (hudPanel != null) hudPanel.SetActive(true);
    }
}
