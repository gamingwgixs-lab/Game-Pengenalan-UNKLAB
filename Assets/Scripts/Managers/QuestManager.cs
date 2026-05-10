using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Daftar Quest (Database)")]
    public QuestMasterDatabase questDatabase;
    
    public List<Quest> allQuests => questDatabase != null ? questDatabase.allQuests : new List<Quest>();

    private Dictionary<string, List<Quest>> npcQuestLookup = new Dictionary<string, List<Quest>>();
    private Dictionary<Quest, StatusQuest> questProgress = new Dictionary<Quest, StatusQuest>();

    public Quest activeQuest;

    [Header("Pengaturan UI")]
    public Sprite defaultPortrait;
    public AudioClip questAcceptedSFX; // SFX saat misi dimulai
    public AudioClip questCompleteSFX; // SFX saat misi selesai
    [Tooltip("Jika dicentang, dialog Misi (Quest) bisa di-skip saat teks sedang berjalan.")]
    public bool canSkipQuestDialogue = false; 

    /// <summary>Jika true, SelesaikanQuest tidak akan memanggil GameEndingManager. Digunakan oleh QuizTrigger.</summary>
    [HideInInspector] public bool blockGameEnding = false;

    private Quest pendingNextQuest;
    private string pendingNpcName;
    private Sprite pendingPortrait;
    private TipeVisualNPC pendingTipe;
    private Sprite pendingFoto2;

    public static System.Action OnQuestStatusChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        PrepareQuests();
    }

    private void Start()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueComplete.AddListener(HandlePendingQuest);
            DialogueManager.Instance.OnDialogueComplete.AddListener(OnDialogueFinished);
        }
    }

    private void PrepareQuests()
    {
        if (questDatabase == null) return;

        npcQuestLookup.Clear();
        questProgress.Clear();
        activeQuest = null;
        
        for (int i = 0; i < allQuests.Count; i++)
        {
            Quest q = allQuests[i];
            if (q == null) continue;

            // Setup Lookup untuk NPC Givers
            if (q.npcGivers != null && q.npcGivers.Count > 0)
            {
                foreach (string giversName in q.npcGivers)
                {
                    string cleanName = giversName.Trim();
                    if (!npcQuestLookup.ContainsKey(cleanName))
                        npcQuestLookup[cleanName] = new List<Quest>();
                    npcQuestLookup[cleanName].Add(q);
                }
            }

            // Setel status Quest awal berdasarkan Start Index di database
            if (i < questDatabase.startQuestIndex)
            {
                questProgress[q] = StatusQuest.Selesai;
            }
            else if (i == questDatabase.startQuestIndex)
            {
                // Tetapkan sebagai Quest aktif secara instan tanpa memicu dialog UI
                activeQuest = q;
                questProgress[q] = StatusQuest.SedangBerjalan;
                
                // Update UI jika ada (Secara instan)
                if (QuestUI.Instance != null) QuestUI.Instance.UpdateDisplay(q.judulQuest);
            }
            else
            {
                questProgress[q] = StatusQuest.BelumMulai;
            }
        }

        // Publikasikan event perubahan status untuk mensinkronisasi objek-objek dunia yang mendengarkan (Observer Pattern)
        OnQuestStatusChanged?.Invoke();

        // Bypass inisiasi Waypoint untuk Quest awal tanpa menunggu dialog
        AktifkanWaypointSenyap(activeQuest);
    }

    public void AmbilQuest(string namaNPC, Sprite portrait, TipeVisualNPC tipe, Sprite foto2 = null)
    {
        if (activeQuest != null) return;

        string cleanName = namaNPC.Trim();
        if (npcQuestLookup.TryGetValue(cleanName, out List<Quest> quests))
        {
            foreach (Quest q in quests)
            {
                if (questProgress[q] == StatusQuest.BelumMulai)
                {
                    AktifkanQuest(q, namaNPC, portrait, tipe, foto2);
                    return;
                }
            }
        }
    }

    private void AktifkanQuest(Quest q, string namaNPC, Sprite portrait, TipeVisualNPC tipe, Sprite foto2 = null)
    {
        if (q == null) return;
        activeQuest = q;
        questProgress[q] = StatusQuest.SedangBerjalan;

        if (questAcceptedSFX != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(questAcceptedSFX);
        }

        string[] finalDialog = null;
        if (q.listDialogPenerimaan != null && q.listDialogPenerimaan.Count > 0)
        {
            foreach (DialogTarget target in q.listDialogPenerimaan)
            {
                if (target.namaNPC.Trim().Equals(namaNPC.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    finalDialog = target.kalimatDialog;
                    break;
                }
            }
            if (finalDialog == null) finalDialog = q.listDialogPenerimaan[0].kalimatDialog;
        }

        if (DialogueManager.Instance != null && finalDialog != null)
        {
            DialogueManager.Instance.TampilkanDialog(namaNPC, portrait, finalDialog, tipe, canSkipQuestDialogue, foto2);
        }

        if (QuestUI.Instance != null)
        {
            QuestUI.Instance.UpdateDisplay(q.judulQuest);
        }

        OnQuestStatusChanged?.Invoke();

        // Aktifkan sistem navigasi Waypoint menggunakan callback setelah dialog penerimaan misi selesai
        if (q.useWaypoints && DialogueManager.Instance != null && finalDialog != null)
        {
            UnityAction waypointActivator = null;
            waypointActivator = () =>
            {
                if (WaypointManager.Instance != null)
                    WaypointManager.Instance.SetNavigation(true);
                DialogueManager.Instance.OnDialogueComplete.RemoveListener(waypointActivator);
            };
            DialogueManager.Instance.OnDialogueComplete.AddListener(waypointActivator);
        }
    }

 

    private void OnDialogueFinished()
    {
        if (activeQuest != null && questProgress[activeQuest] == StatusQuest.SedangBerjalan)
        {
            if (QuestUI.Instance != null) QuestUI.Instance.UpdateDisplay(activeQuest.judulQuest);
        }
    }

    private void AktifkanWaypointSenyap(Quest q)
    {
        // Force start navigasi Waypoint untuk skenario fast-forward atau debugging scene
        if (q != null && q.useWaypoints && WaypointManager.Instance != null)
        {
            WaypointManager.Instance.SetNavigation(true);
        }
    }

    private void HandlePendingQuest()
    {
        if (pendingNextQuest != null)
        {
            Quest next = pendingNextQuest;
            string name = pendingNpcName;
            Sprite port = pendingPortrait;
            TipeVisualNPC tipe = pendingTipe;

            pendingNextQuest = null;
            pendingNpcName = "";
            pendingPortrait = null;
            Sprite f2 = pendingFoto2;
            pendingFoto2 = null;

            AktifkanQuest(next, name, port, tipe, f2);
        }
    }

    public bool CekTargetNPC(string namaNPC)
    {
        if (activeQuest != null && questProgress[activeQuest] == StatusQuest.SedangBerjalan)
        {
            if (activeQuest.npcTargets != null && activeQuest.npcTargets.Contains(namaNPC))
            {
                return true;
            }
        }
        return false;
    }

    public bool SelesaikanQuest(string namaNPC, Sprite portrait, string[] fallbackDialog, TipeVisualNPC tipe, Sprite foto2 = null)
    {
        if (activeQuest == null) return false;

        if (CekTargetNPC(namaNPC) || string.IsNullOrEmpty(namaNPC))
        {
            Quest completedQuest = activeQuest;
            questProgress[completedQuest] = StatusQuest.Selesai;
            activeQuest = null;

            if (questCompleteSFX != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(questCompleteSFX);
            }

            string[] finalDialog = null;
            if (completedQuest.listDialogPenyelesaian != null && completedQuest.listDialogPenyelesaian.Count > 0)
            {
                foreach (DialogTarget target in completedQuest.listDialogPenyelesaian)
                {
                    if (target.namaNPC.Trim().Equals(namaNPC.Trim(), System.StringComparison.OrdinalIgnoreCase))
                    {
                        finalDialog = target.kalimatDialog;
                        break;
                    }
                }
                if (finalDialog == null) finalDialog = completedQuest.listDialogPenyelesaian[0].kalimatDialog;
            }

            if (finalDialog == null) finalDialog = fallbackDialog;
            if (finalDialog == null) finalDialog = new string[] { "[Sistem: Dialog belum diatur]" };

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.TampilkanDialog(namaNPC, portrait, finalDialog, tipe, canSkipQuestDialogue, foto2);
            }

            if (completedQuest.nextQuest != null)
            {
                pendingNextQuest = completedQuest.nextQuest;
                pendingNpcName = namaNPC; 
                pendingPortrait = portrait;
                pendingTipe = tipe;
                pendingFoto2 = foto2;
            }
            else
            {
                if (QuestUI.Instance != null) QuestUI.Instance.HideQuestPanel();
                
                // Cek penyelesaian Master Quest Database untuk men-trigger urutan Game Ending
                if (allQuests.Count > 0 && completedQuest == allQuests[allQuests.Count - 1])
                {
                    if (GameEndingManager.Instance != null && !blockGameEnding)
                    {
                        GameEndingManager.Instance.TriggerGameEndingSequence();
                    }
                }
            }

            OnQuestStatusChanged?.Invoke();
            return true;
        }
        return false;
    }

    public StatusQuest GetQuestStatus(Quest q)
    {
        if (q == null) return StatusQuest.BelumMulai;
        if (questProgress.TryGetValue(q, out StatusQuest status)) return status;
        return StatusQuest.BelumMulai;
    }

    public StatusQuest GetQuestStatusByTitle(string judul)
    {
        foreach(var pair in questProgress)
        {
            if(pair.Key.judulQuest == judul) return pair.Value;
        }
        return StatusQuest.BelumMulai;
    }

    public bool AdaQuestBaruNPC(string namaNPC)
    {
        if (activeQuest != null) return false;

        string cleanName = namaNPC.Trim();
        if (npcQuestLookup.TryGetValue(cleanName, out List<Quest> quests))
        {
            foreach (Quest q in quests)
            {
                if (questProgress[q] == StatusQuest.BelumMulai) return true;
            }
        }
        return false;
    }

    public void CekPOI(string namaPOI, Sprite foto = null, Sprite foto2 = null)
    {
        if (activeQuest != null && questProgress[activeQuest] == StatusQuest.SedangBerjalan)
        {
            if (activeQuest.poiTarget == namaPOI) SelesaikanQuest("", foto, null, TipeVisualNPC.System, foto2); 
        }
    }

    /// <summary>
    /// Menyelesaikan quest secara senyap (tanpa dialog dan tanpa GameEnding).
    /// Digunakan oleh QuizTrigger agar quiz bisa berjalan dulu sebelum ending.
    /// </summary>
    public void SelesaikanQuestSenyap(string namaPOI)
    {
        if (activeQuest == null) return;
        if (activeQuest.poiTarget != namaPOI) return;

        Quest completedQuest = activeQuest;
        questProgress[completedQuest] = StatusQuest.Selesai;
        activeQuest = null;

        if (QuestUI.Instance != null) QuestUI.Instance.HideQuestPanel();
        OnQuestStatusChanged?.Invoke();
    }

#if UNITY_EDITOR
    /// <summary>
    /// [DEBUG ONLY] Melompat ke quest pada index tertentu.
    /// Semua quest sebelumnya = Selesai, quest target = SedangBerjalan, sisanya = BelumMulai.
    /// </summary>
    public void DebugJumpToQuest(int targetIndex)
    {
        if (questDatabase == null || allQuests.Count == 0) return;
        targetIndex = Mathf.Clamp(targetIndex, 0, allQuests.Count - 1);

        // 1. Tutup paksa dialog yang sedang berjalan
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ForceCloseFromDebug();
        }

        // 2. Matikan waypoint navigasi
        if (WaypointManager.Instance != null)
        {
            WaypointManager.Instance.ForceResetState();
        }

        // 3. Bersihkan pending quest
        pendingNextQuest = null;
        pendingNpcName = "";
        pendingPortrait = null;
        pendingFoto2 = null;

        // 4. Reset seluruh state quest
        activeQuest = null;
        for (int i = 0; i < allQuests.Count; i++)
        {
            Quest q = allQuests[i];
            if (q == null) continue;

            if (i < targetIndex)
                questProgress[q] = StatusQuest.Selesai;
            else if (i == targetIndex)
            {
                activeQuest = q;
                questProgress[q] = StatusQuest.SedangBerjalan;
            }
            else
                questProgress[q] = StatusQuest.BelumMulai;
        }

        // 5. Update UI
        if (activeQuest != null && QuestUI.Instance != null)
        {
            QuestUI.Instance.UpdateDisplay(activeQuest.judulQuest);
        }

        // 6. Sinkronkan seluruh objek di dunia
        OnQuestStatusChanged?.Invoke();

        // 7. Aktifkan Waypoint jika Quest tujuan membutuhkan
        AktifkanWaypointSenyap(activeQuest);

        Debug.Log($"<color=cyan>[Debug] Jumped to Quest [{targetIndex}]: {(activeQuest != null ? activeQuest.judulQuest : "NULL")}</color>");
    }

    /// <summary>
    /// [DEBUG ONLY] Mendapatkan index quest yang sedang aktif saat ini.
    /// </summary>
    public int DebugGetActiveQuestIndex()
    {
        if (activeQuest == null) return -1;
        return allQuests.IndexOf(activeQuest);
    }
#endif
}
