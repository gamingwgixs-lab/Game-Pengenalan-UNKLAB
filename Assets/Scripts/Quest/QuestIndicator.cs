using UnityEngine;

/// <summary>
/// Menampilkan indikator visual (Tanda Tanya/Seru) berdasarkan status misi.
/// Diperbarui agar bisa bekerja meskipun NpcDialogue dan Collider berada di GameObject berbeda.
/// </summary>
public class QuestIndicator : MonoBehaviour
{
    [Header("Visual Indicators (GameObject)")]
    public GameObject giverVisual;
    public GameObject goalVisual;

    [Header("Identitas Objek")]
    public bool isNpc = true;

    [Tooltip("Tarik script NpcDialogue ke sini jika posisinya terpisah dari Collider ini.")]
    public NpcDialogue npcReference;

    [Tooltip("Nama lokasi/pintu jika ini bukan NPC (Samakan dengan poiTarget di Quest SO).")]
    public string poiName;

    private string finalIdentity;
    private bool playerIsInside = false;

    private void Start()
    {
        InitializeIdentity();
        RefreshIndicator();
    }

    private void OnEnable()
    {
        QuestManager.OnQuestStatusChanged += RefreshIndicator;
    }

    private void OnDisable()
    {
        QuestManager.OnQuestStatusChanged -= RefreshIndicator;
    }

    private void InitializeIdentity()
    {
        if (isNpc)
        {
            // Jika slot kosong, coba cari di object yang sama dulu (antisipasi)
            if (npcReference == null) npcReference = GetComponent<NpcDialogue>();

            if (npcReference != null)
            {
                finalIdentity = npcReference.npcName    ;
            }
            else
            {
                Debug.LogError($"[QuestIndicator] {gameObject.name} diset NPC tapi NpcDialogue TIDAK DITEMUKAN! Tarik manual ke slot Inspector.");
                finalIdentity = "UNKNOWN_NPC";
            }
        }
        else
        {
            finalIdentity = poiName;
        }
    }

    public void RefreshIndicator()
    {
        if (QuestManager.Instance == null) return;

        bool showGiver = false;
        bool showGoal = false;

        if (QuestManager.Instance.activeQuest != null)
        {
            Quest active = QuestManager.Instance.activeQuest;
            
            if (isNpc)
            {
                if (active.npcTargets != null && active.npcTargets.Contains(finalIdentity))
                    showGoal = true;
            }
            else
            {
                if (active.poiTarget == finalIdentity)
                    showGoal = true;
            }
        }

        if (!showGoal && isNpc)
        {
            if (QuestManager.Instance.AdaQuestBaruNPC(finalIdentity))
                showGiver = true;
        }

        ApplyVisuals(showGiver, showGoal);
    }

    private void ApplyVisuals(bool giver, bool goal)
    {
        if (playerIsInside)
        {
            if (giverVisual != null) giverVisual.SetActive(false);
            if (goalVisual != null) goalVisual.SetActive(false);
            return;
        }

        if (giverVisual != null) giverVisual.SetActive(giver);
        if (goalVisual != null) goalVisual.SetActive(goal);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInside = true;
            ApplyVisuals(false, false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInside = false;
            RefreshIndicator();
        }
    }
}
