using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MasterQuestDatabase", menuName = "Scriptable Objects/Quest Master Database")]
public class QuestMasterDatabase : ScriptableObject
{
    [Header("Pusat Data Seluruh Quest")]
    [Tooltip("Tentukan dari index mana game ini akan dimulai (0 = Quest Pertama).")]
    public int startQuestIndex = 0;

    [Tooltip("Tarik 36+ Quest Anda ke sini sesuai urutan timeline (0 = Awal, dst)")]
    public List<Quest> allQuests = new List<Quest>();

    [Header("Utility Helper (Optional)")]
    [TextArea(3, 5)]
    public string catatanDeveloper = "Gunakan database ini untuk memantau alur timeline 36+ quest skripsi Anda.";

    /// <summary>
    /// Mencari Quest berdasarkan index dalam list. 
    /// Sangat efisien untuk Save/Load sistem di Mobile (hanya simpan angka int).
    /// </summary>
    public Quest GetQuestByIndex(int index)
    {
        if (index >= 0 && index < allQuests.Count)
        {
            return allQuests[index];
        }
        return null;
    }

    /// <summary>
    /// Mencari Index dari sebuah Quest. 
    /// Berguna untuk menentukan 'Progress' pemain saat ini dalam angka.
    /// </summary>
    public int GetQuestIndex(Quest quest)
    {
        return allQuests.IndexOf(quest);
    }

    /// <summary>
    /// Menghitung total Quest yang terdaftar.
    /// </summary>
    public int TotalQuestCount => allQuests.Count;
}
