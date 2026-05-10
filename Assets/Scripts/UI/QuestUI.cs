using UnityEngine;
using TMPro;

public class QuestUI : MonoBehaviour
{
    public static QuestUI Instance;

    [Header("Referensi UI")]
    public GameObject questPanel;
    public TextMeshProUGUI questObjectiveText;

    [Header("Referensi UI Notifikasi POI")]
    public GameObject poiNotificationPanel;
    public TextMeshProUGUI poiNotificationText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        HideQuestPanel();
        if (poiNotificationPanel != null) poiNotificationPanel.SetActive(false);

        // Berlangganan event DialogueManager (Observer Pattern - Loose Coupling)
        if (DialogueManager.Instance != null)
        {
            // Sembunyikan Quest Panel saat System Dialogue aktif untuk mencegah UI overlap
            DialogueManager.Instance.OnSystemDialogueStart.AddListener(HideQuestPanel);
            DialogueManager.Instance.OnDialogueComplete.AddListener(OnDialogueSelesai);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe event saat objek dihancurkan untuk mencegah memory leak
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnSystemDialogueStart.RemoveListener(HideQuestPanel);
            DialogueManager.Instance.OnDialogueComplete.RemoveListener(OnDialogueSelesai);
        }
    }

    private void OnDialogueSelesai()
    {
        // Pulihkan tampilan Quest Panel setelah dialog selesai jika pemain memiliki Quest aktif
        if (QuestManager.Instance != null && QuestManager.Instance.activeQuest != null)
        {
            UpdateDisplay(QuestManager.Instance.activeQuest.judulQuest);
        }
    }

    public void TampilkanNotifikasiPOI(string pesan)
    {
        if (poiNotificationPanel == null) return;

        poiNotificationPanel.SetActive(true);
        if (poiNotificationText != null) poiNotificationText.text = pesan;

        // Jadwalkan penutupan notifikasi POI menggunakan Invoke
        Invoke("SembunyikanNotifikasiPOI", 3f);
    }

    private void SembunyikanNotifikasiPOI()
    {
        if (poiNotificationPanel != null) poiNotificationPanel.SetActive(false);
    }

    public void UpdateDisplay(string deskripsi)
    {
        if (questPanel != null) questPanel.SetActive(true);
        if (questObjectiveText != null) questObjectiveText.text = deskripsi;
    }

    public void HideQuestPanel()
    {
        if (questPanel != null) questPanel.SetActive(false);
    }
}
