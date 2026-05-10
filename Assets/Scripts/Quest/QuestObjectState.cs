using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct QuestStateStep
{
    [Tooltip("Pilih Index Quest dari Master Database (0, 1, 2, dst).")]
    public int questIndex;
    
    [Tooltip("Status misi minimal yang harus dicapai agar instruksi ini aktif.")]
    public StatusQuest triggerStatus;

    [Tooltip("Apakah objek harus Aktif atau Mati saat status ini tercapai?")]
    public bool targetState;

    public Quest GetQuest(QuestMasterDatabase db)
    {
        if (db == null) return null;
        return db.GetQuestByIndex(questIndex);
    }
}

public class QuestObjectState : MonoBehaviour
{
    [Header("Pengaturan Objek")]
    [Tooltip("GameObject yang akan diaktifkan/dimatikan.")]
    public GameObject objectToToggle;
    
    [Tooltip("Status awal objek saat game baru dimulai (sebelum ada instruksi misi yang terpenuhi).")]
    public bool initialState = false;

    [Header("Urutan Peristiwa (Timeline)")]
    [Tooltip("Daftar instruksi. Instruksi yang LEBIH BAWAH akan menimpa instruksi di atasnya jika keduanya terpenuhi.")]
    public List<QuestStateStep> sequenceSteps = new List<QuestStateStep>();

    private void OnEnable()
    {
        // Berlangganan event OnQuestStatusChanged untuk update visibilitas secara otomatis
        QuestManager.OnQuestStatusChanged += RefreshState;
    }

    private void OnDisable()
    {
        // Unsubscribe event untuk mencegah pemanggilan saat objek tidak aktif
        QuestManager.OnQuestStatusChanged -= RefreshState;
    }

    private void Start()
    {
        // Evaluasi state awal pada siklus Start
        RefreshState();
    }

    [ContextMenu("Force Refresh State")]
    public void RefreshState()
    {
        // Ambil referensi database dari Singleton QuestManager
        if (QuestManager.Instance == null || QuestManager.Instance.questDatabase == null || objectToToggle == null) return;

        QuestMasterDatabase questDatabase = QuestManager.Instance.questDatabase;
        
        bool finalState = initialState;

        foreach (var step in sequenceSteps)
        {
            Quest targetQuest = step.GetQuest(questDatabase);
            if (targetQuest == null) continue;

            // Ambil status misi saat ini dari manager
            StatusQuest currentStatus = QuestManager.Instance.GetQuestStatus(targetQuest);

            // Evaluasi secara progresif: Jika status misi saat ini telah mencapai atau melewati trigger target, perbarui final state.
            if ((int)currentStatus >= (int)step.triggerStatus)
            {
                // Instruksi ini berlaku. Update keputusan final.
                finalState = step.targetState;
            }
        }

        // Terapkan perubahan GameObject state hanya jika ada perbedaan (Optimasi redundant call)
        if (objectToToggle.activeSelf != finalState)
        {
            objectToToggle.SetActive(finalState);
        }
    }
}
