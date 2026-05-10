using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    [Header("Identitas Lokasi")]
    [Tooltip("Nama ini harus sama dengan npcPemberi atau poiTarget di ScriptableObject Quest")]
    public string poiName;

    [Header("Mode Trigger")]
    public bool bisaMemberiQuest = true;
    public bool bisaMenyelesaikanQuest = true;

    [Header("Visual Sistem")]
    [Tooltip("Opsional: Foto ruangan atau tempat untuk ditampilkan di System Dialogue.")]
    public Sprite gambarLokasi;
    [Tooltip("Opsional: Foto profil/ruangan kedua (misal peta miniatur).")]
    public Sprite gambarLokasi2;

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;

        // Jika ada QuizTrigger ending di objek yang sama, biarkan QuizTrigger yang handle
        QuizTrigger quizTrigger = GetComponent<QuizTrigger>();
        if (quizTrigger != null && quizTrigger.enabled && quizTrigger.isGameEndingQuiz) return;

        if (other.CompareTag("Player"))
        {
            if (string.IsNullOrEmpty(poiName)) return;

            // ═══ MODE RE-EXPLORE: Tampilkan info ruangan tanpa quest ═══
            if (QuizManager.Instance != null && QuizManager.Instance.IsReExploreActive)
            {
                if (DialogueManager.Instance != null)
                {
                    // Sembunyikan tombol Next selama dialog info berjalan
                    if (QuizUI.Instance != null) QuizUI.Instance.TampilkanTombolNextExplore(false);

                    // Cari dialog bawaan dari quest yang poiTarget-nya cocok dengan poiName ini
                    string[] dialogBawaan = CariDialogBawaanQuest();
                    DialogueManager.Instance.TampilkanDialog("", gambarLokasi, dialogBawaan, TipeVisualNPC.System, true, gambarLokasi2);

                    // Matikan GO dan nyalakan kembali tombol Next setelah dialog selesai
                    DialogueManager.Instance.OnDialogueComplete.AddListener(MatikanSetelahReExplore);
                }
                return;
            }

            // ═══ MODE NORMAL: Logika Quest ═══
            if (QuestManager.Instance == null) return;

            // 1. PRIORITAS: Selesaikan quest jika lokasi ini adalah target quest aktif
            if (bisaMenyelesaikanQuest && QuestManager.Instance.activeQuest != null)
            {
                if (QuestManager.Instance.activeQuest.poiTarget == poiName)
                {
                    QuestManager.Instance.CekPOI(poiName, gambarLokasi, gambarLokasi2);
                    // Matikan trigger agar tidak terpicu lagi setelah selesai
                    gameObject.SetActive(false); 
                    return;
                }
            }

            // 2. ALTERNATIF: Berikan quest baru jika lokasi ini adalah pemberi quest
            if (bisaMemberiQuest && QuestManager.Instance.activeQuest == null)
            {
                if (QuestManager.Instance.AdaQuestBaruNPC(poiName))
                {
                    // Kirim nama POI dan foto agar muncul di Panel System
                    QuestManager.Instance.AmbilQuest(poiName, gambarLokasi, TipeVisualNPC.System, gambarLokasi2);
                    
                    // Opsi: Matikan trigger jika quest ini hanya bisa diambil sekali lewat area ini
                    // gameObject.SetActive(false); 
                }
            }
        }
    }

    private void MatikanSetelahReExplore()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnDialogueComplete.RemoveListener(MatikanSetelahReExplore);
        
        // Nyalakan kembali tombol Next
        if (QuizUI.Instance != null) QuizUI.Instance.TampilkanTombolNextExplore(true);

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Mencari dialog penyelesaian bawaan dari Quest ScriptableObject yang poiTarget-nya cocok dengan poiName trigger ini.
    /// </summary>
    private string[] CariDialogBawaanQuest()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.questDatabase != null)
        {
            foreach (Quest q in QuestManager.Instance.allQuests)
            {
                if (q != null && q.poiTarget == poiName)
                {
                    // Ambil dialog penyelesaian pertama dari quest ini
                    if (q.listDialogPenyelesaian != null && q.listDialogPenyelesaian.Count > 0)
                    {
                        return q.listDialogPenyelesaian[0].kalimatDialog;
                    }
                }
            }
        }

        // Fallback jika tidak ditemukan
        return new string[] { poiName };
    }
}
