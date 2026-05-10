using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class NpcInteractionButton : MonoBehaviour
{
    public void TriggerDialogue()
    {
        // Debug.Log($"[NPC Button] Mencoba interaksi pada: {gameObject.name}");

        // Cari NpcDialogue di Parent (Root NPC)
        NpcDialogue dialogue = GetComponentInParent<NpcDialogue>();
        
        if (dialogue != null)
        {
            // Debug.Log($"[NPC Button] Mengirim data dari {dialogue.npcName} ke UI.");
            dialogue.KirimDataKeUI();
            
            // Sembunyikan bubble setelah interaksi berhasil
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("[NPC Button] ERROR: NpcDialogue TIDAK DITEMUKAN di parent!");
        }
    }
}
