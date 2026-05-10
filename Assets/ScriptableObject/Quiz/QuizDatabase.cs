using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Data struktur untuk satu butir soal quiz.
/// Setiap soal memiliki teks pertanyaan, 3 pilihan jawaban, dan index jawaban benar.
/// </summary>
[System.Serializable]
public class QuizQuestion
{
    [TextArea(2, 5)]
    public string pertanyaan;

    [Tooltip("Harus berisi tepat 3 pilihan jawaban.")]
    public string[] pilihan = new string[3];

    [Tooltip("Index jawaban benar (0, 1, atau 2)")]
    [Range(0, 2)]
    public int indexJawabanBenar;

    [Header("Re-Explore Target")]
    [Tooltip("Isi dengan nama ruangan persis seperti RoomInfo (misal: 'Ruangan 502'). Kosongkan jika tidak ada.")]
    public string targetRoomName;
}

/// <summary>
/// ScriptableObject container yang menyimpan koleksi soal untuk satu sesi quiz.
/// Dibuat via menu: Create → Quiz → Quiz Database.
/// Mengikuti pola QuestMasterDatabase.
/// </summary>
[CreateAssetMenu(fileName = "QuizBaru", menuName = "Quiz/Quiz Database")]
public class QuizDatabase : ScriptableObject
{
    [Header("Identitas Quiz")]
    public string judulQuiz = "Quiz Baru";

    [Header("Daftar Soal")]
    public List<QuizQuestion> daftarSoal = new List<QuizQuestion>();

    /// <summary>
    /// Mendapatkan jumlah total soal dalam database ini.
    /// </summary>
    public int JumlahSoal => daftarSoal != null ? daftarSoal.Count : 0;

    /// <summary>
    /// Mendapatkan soal berdasarkan index. Null jika index di luar jangkauan.
    /// </summary>
    public QuizQuestion GetSoal(int index)
    {
        if (daftarSoal == null || index < 0 || index >= daftarSoal.Count) return null;
        return daftarSoal[index];
    }
}
