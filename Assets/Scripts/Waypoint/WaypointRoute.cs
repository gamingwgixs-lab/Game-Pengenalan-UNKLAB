using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Komponen Scene: Ditaruh di GameObject parent yang memiliki child Empty GameObject sebagai titik-titik rute.
/// Child akan otomatis terdaftar sebagai waypoint secara berurutan (berdasarkan Hierarchy order).
/// Gizmo akan menggambar jalur rute di Scene View untuk memudahkan visual debugging.
/// </summary>
public class WaypointRoute : MonoBehaviour
{
    [Header("Route Settings")]
    [Tooltip("Nama unik rute ini. Harus SAMA PERSIS dengan yang diisi di Quest ScriptableObject.")]
    public string routeName;

    [Tooltip("Jarak minimum pemain ke sebuah point agar dianggap 'sudah sampai'. Satuan unit Unity.")]
    public float arrivalThreshold = 2.5f;

    [Header("Debug")]
    [SerializeField] private List<Transform> points = new List<Transform>();

    /// <summary>
    /// List titik-titik waypoint yang sudah terurut.
    /// </summary>
    public List<Transform> Points => points;

    private void Awake()
    {
        CollectChildPoints();
    }

    /// <summary>
    /// Mengumpulkan semua child Transform langsung (bukan cucu) sebagai titik waypoint.
    /// Urutan mengikuti urutan di Hierarchy.
    /// </summary>
    public void CollectChildPoints()
    {
        points.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            points.Add(transform.GetChild(i));
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Validasi otomatis saat nilai berubah di Inspector.
    /// </summary>
    private void OnValidate()
    {
        CollectChildPoints();

        // Auto-fill routeName dari nama GameObject jika kosong
        if (string.IsNullOrEmpty(routeName))
        {
            routeName = gameObject.name;
        }
    }

    /// <summary>
    /// Menggambar garis jalur rute di Scene View (hanya terlihat di Editor).
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Kumpulkan ulang agar selalu up-to-date di Editor
        CollectChildPoints();

        if (points.Count < 2) return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < points.Count - 1; i++)
        {
            if (points[i] != null && points[i + 1] != null)
            {
                Gizmos.DrawLine(points[i].position, points[i + 1].position);
            }
        }

        // Gambar sphere kecil di setiap titik
        Gizmos.color = Color.yellow;
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i] != null)
            {
                Gizmos.DrawWireSphere(points[i].position, arrivalThreshold);
            }
        }

        // Sphere hijau di titik awal
        if (points[0] != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(points[0].position, 0.3f);
        }

        // Sphere merah di titik akhir
        if (points[points.Count - 1] != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(points[points.Count - 1].position, 0.3f);
        }
    }
#endif
}
