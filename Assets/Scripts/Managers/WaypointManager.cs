using UnityEngine;

/// <summary>
/// Singleton Manager: Mengelola navigasi panah waypoint secara sekuensial.
/// Panah menunjuk ke titik-titik route satu per satu berdasarkan jarak pemain.
/// Dioptimasi untuk mobile: InvokeRepeating (bukan Update), reuse Vector3, lazy caching.
/// </summary>
public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance;

    [Header("Referensi Utama")]
    [Tooltip("Tarik Transform Player ke sini (Sumber Arah).")]
    public Transform playerTransform;

    [Header("Referensi UI/Visual")]
    [Tooltip("Objek Utama Panah (Parent) untuk Aktif/Mati.")]
    public GameObject arrowObject;
    [Tooltip("Transform Sprite Panah 2D yang akan berputar.")]
    public Transform arrowSprite;

    [Header("Navigasi Settings")]
    [Tooltip("Interval update navigasi (dalam detik). Semakin kecil = semakin responsif tapi lebih berat.")]
    [SerializeField] private float updateInterval = 0.05f;

    [Tooltip("Offset rotasi sprite panah (derajat). Atur sampai panah menunjuk arah yang benar.\n0 = sprite default menghadap KIRI\n90 = sprite default menghadap ATAS\n-90 = sprite default menghadap BAWAH\n180 = sprite default menghadap KANAN")]
    [SerializeField] private float arrowRotationOffset = 0f;

    [Header("Debug (Read Only)")]
    [SerializeField] private bool isNavigating = false;
    [SerializeField] private int currentPointIndex = 0;
    [SerializeField] private string activeRouteName = "";

    // Cache internal
    private WaypointRoute activeRoute;
    private Quest activeQuest;
    private Vector3 directionBuffer; // Reuse untuk mengurangi GC di mobile
    private Camera cachedCamera;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        cachedCamera = Camera.main;
    }

    private void OnEnable()
    {
        QuestManager.OnQuestStatusChanged += SyncWithQuest;
    }

    private void OnDisable()
    {
        QuestManager.OnQuestStatusChanged -= SyncWithQuest;
        CancelInvoke(nameof(UpdateNavigation));
    }

    private void Start()
    {
        // Pastikan panah mati saat awal game dimulai
        if (arrowObject != null) arrowObject.SetActive(false);
        SyncWithQuest();

        // Auto-activate: Jika quest sudah berjalan saat game dimulai (misal dari startQuestIndex)
        // dan quest tersebut menggunakan waypoint, langsung nyalakan navigasi.
        if (activeQuest != null && activeQuest.useWaypoints)
        {
            SetNavigation(true);
        }
    }

    // =========================================================================
    // PUBLIC API - Dipanggil dari luar (NPC Dialog, QuestManager, WarpManager)
    // =========================================================================

    /// <summary>
    /// Menyalakan/mematikan navigasi panah. Panggil dari akhir dialog NPC.
    /// </summary>
    public void SetNavigation(bool state)
    {
        if (state)
        {
            if (!SetupRoute())
            {
                Debug.LogError("[WaypointMgr] SetupRoute() GAGAL. Arrow tidak dinyalakan.");
                return;
            }

            isNavigating = true;
            if (arrowObject != null)
            {
                arrowObject.SetActive(true);
            }
            else
            {
                Debug.LogError("[WaypointMgr] arrowObject adalah NULL di Inspector!");
            }

            // Mulai periodic update (hemat baterai dibanding Update)
            CancelInvoke(nameof(UpdateNavigation));
            InvokeRepeating(nameof(UpdateNavigation), 0f, updateInterval);
        }
        else
        {
            StopNavigation();
        }
    }

    /// <summary>
    /// WAJIB dipanggil oleh WarpManager sebelum memindahkan player.
    /// Mencegah "Ghost State" karena perpindahan posisi instan.
    /// </summary>
    public void ForceResetState()
    {
        StopNavigation();
        currentPointIndex = 0;
        activeRoute = null;
        activeRouteName = "";
    }

    // =========================================================================
    // INTERNAL - Logika navigasi sekuensial
    // =========================================================================

    /// <summary>
    /// Sinkronisasi otomatis saat quest berubah (via event OnQuestStatusChanged).
    /// </summary>
    private void SyncWithQuest()
    {
        if (QuestManager.Instance == null) return;

        activeQuest = QuestManager.Instance.activeQuest;

        // Reset navigasi setiap kali quest berubah
        ForceResetState();
    }

    /// <summary>
    /// Mencari dan meng-cache WaypointRoute di Scene berdasarkan nama di Quest SO.
    /// Dipanggil SEKALI saat SetNavigation(true).
    /// </summary>
    private bool SetupRoute()
    {
        if (activeQuest == null || !activeQuest.useWaypoints || string.IsNullOrEmpty(activeQuest.waypointRouteName))
        {
            Debug.LogWarning("[Waypoint] Quest tidak memiliki konfigurasi waypoint yang valid.");
            return false;
        }

        // Cari WaypointRoute di Scene (hanya sekali, lalu di-cache)
        WaypointRoute[] allRoutes = FindObjectsByType<WaypointRoute>(FindObjectsSortMode.None);
        activeRoute = null;

        for (int i = 0; i < allRoutes.Length; i++)
        {
            if (allRoutes[i].routeName == activeQuest.waypointRouteName)
            {
                activeRoute = allRoutes[i];
                break;
            }
        }

        if (activeRoute == null || activeRoute.Points.Count == 0)
        {
            Debug.LogError($"[Waypoint] Route '{activeQuest.waypointRouteName}' tidak ditemukan atau kosong di Scene!");
            return false;
        }

        currentPointIndex = 0;
        activeRouteName = activeRoute.routeName;

        Debug.Log($"[Waypoint] Navigasi dimulai: Route '{activeRouteName}' ({activeRoute.Points.Count} titik)");
        return true;
    }

    /// <summary>
    /// Dipanggil secara periodik oleh InvokeRepeating.
    /// Menghitung jarak ke titik saat ini dan memajukan index jika sudah dekat.
    /// </summary>
    private void UpdateNavigation()
    {
        if (!isNavigating || activeRoute == null || playerTransform == null) return;

        // Safety check: index tidak boleh melebihi jumlah titik
        if (currentPointIndex >= activeRoute.Points.Count)
        {
            OnRouteCompleted();
            return;
        }

        Transform currentTarget = activeRoute.Points[currentPointIndex];
        if (currentTarget == null) return;

        // --- CEK JARAK (tanpa alokasi Vector3 baru) ---
        directionBuffer.x = currentTarget.position.x - playerTransform.position.x;
        directionBuffer.y = 0f; // Abaikan ketinggian untuk 2.5D
        directionBuffer.z = currentTarget.position.z - playerTransform.position.z;

        float distanceSqr = directionBuffer.x * directionBuffer.x + directionBuffer.z * directionBuffer.z;
        float thresholdSqr = activeRoute.arrivalThreshold * activeRoute.arrivalThreshold;

        // Jika sudah cukup dekat -> maju ke titik berikutnya
        if (distanceSqr <= thresholdSqr)
        {
            currentPointIndex++;

            if (currentPointIndex >= activeRoute.Points.Count)
            {
                OnRouteCompleted();
                return;
            }

            // Update target baru untuk rotasi
            currentTarget = activeRoute.Points[currentPointIndex];
        }

        // --- UPDATE ROTASI PANAH ---
        if (currentTarget != null)
        {
            RotateArrowTo(currentTarget.position);
        }
    }

    /// <summary>
    /// Dipanggil saat pemain menyelesaikan semua titik di route.
    /// </summary>
    private void OnRouteCompleted()
    {
        Debug.Log("<color=green>[Waypoint] Route selesai! Navigasi dimatikan.</color>");
        StopNavigation();
    }

    /// <summary>
    /// Menghentikan navigasi dan menyembunyikan panah.
    /// </summary>
    private void StopNavigation()
    {
        isNavigating = false;
        CancelInvoke(nameof(UpdateNavigation));
        if (arrowObject != null) arrowObject.SetActive(false);
    }

    /// <summary>
    /// Memutar sprite panah agar selalu menghadap kamera (billboard) dan
    /// menunjuk arah target dalam screen space (cocok untuk 2.5D dengan kamera miring).
    /// </summary>
    private void RotateArrowTo(Vector3 targetPos)
    {
        // Hitung arah relatif (2.5D: Hanya X dan Z, abaikan Y)
        directionBuffer.x = targetPos.x - playerTransform.position.x;
        directionBuffer.z = targetPos.z - playerTransform.position.z;

        // Atan2 → sudut dalam radian → konversi ke derajat
        float angle = Mathf.Atan2(directionBuffer.x, directionBuffer.z) * Mathf.Rad2Deg;

        // Terapkan rotasi pada sumbu Z panah 2D
        arrowSprite.rotation = Quaternion.Euler(0, 0, -angle + arrowRotationOffset);
    }
}
