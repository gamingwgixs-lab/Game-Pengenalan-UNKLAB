using UnityEngine;
using TMPro;

public class LocationUI : MonoBehaviour
{
    public static LocationUI Instance;

    [Header("Referensi UI")]
    public GameObject panelLokasi;
    public TextMeshProUGUI teksLokasi;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TampilkanNamaLokasi(string nama)
    {
        // Jika nama kosong, jangan ubah teksnya
        if (string.IsNullOrEmpty(nama)) return;

        // Pasang teks nama lokasi secara instan
        if (teksLokasi != null) teksLokasi.text = nama;

        // Pastikan panel menyala (sebagai cadangan)
        if (panelLokasi != null) panelLokasi.SetActive(true);
    }
}
