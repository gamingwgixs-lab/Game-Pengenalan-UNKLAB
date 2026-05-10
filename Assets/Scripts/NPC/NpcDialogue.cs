using UnityEngine;

public class NpcDialogue : MonoBehaviour
{
    [Header("Identitas NPC")]
    public string npcName;
    public Sprite npcPortrait;
    public TipeVisualNPC tipeVisual; // Dropdown Pemandu/Dosen

    [TextArea(3, 10)]
    public string[] dialogueParagraphs;

    public void KirimDataKeUI()
    {
        // 1. Cek apakah ada quest aktif (Quest Selesai)
        if (QuestManager.Instance != null && QuestManager.Instance.CekTargetNPC(npcName))
        {
            if (QuestManager.Instance.SelesaikanQuest(npcName, npcPortrait, dialogueParagraphs, tipeVisual)) 
            {
                return; 
            }
        }

        // 2. Cek apakah ada quest baru (Quest Ambil)
        if (QuestManager.Instance != null && QuestManager.Instance.AdaQuestBaruNPC(npcName))
        {
            QuestManager.Instance.AmbilQuest(npcName, npcPortrait, tipeVisual);
            return;
        }

        // 3. Jika tidak ada quest, jalankan dialog biasa
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.TampilkanDialog(npcName, npcPortrait, dialogueParagraphs, tipeVisual);
        }
    }
}