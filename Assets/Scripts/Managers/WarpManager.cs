using UnityEngine;

public class WarpManager : MonoBehaviour
{
    public static WarpManager Instance; 

    [Header("Referensi Utama")]
    [Tooltip("Karakter utama yang akan dipindahkan")]
    public Transform playerTransform;
    
    [Tooltip("Masukkan objek WarpUI (Grup UI yang berisi tombol Masuk)")]
    public GameObject warpUI; 

    private Transform currentDestination; 
    private RoomInfo currentRoomInfo;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetupWarp(Transform targetRoom, RoomInfo roomInfo = null)
    {
        currentDestination = targetRoom;
        currentRoomInfo = roomInfo;

        // Jangan tampilkan tombol "Masuk" jika sedang dalam mode Re-Explore
        if (QuizManager.Instance != null && QuizManager.Instance.IsReExploreActive)
            return;

        warpUI.SetActive(true);
    }

    public void CancelWarp()
    {
        currentDestination = null;
        currentRoomInfo = null;
        warpUI.SetActive(false);
    }
    
    public void ExecuteWarp()
    {
        if (currentDestination == null || playerTransform == null) return;

        ResetEnvironmentStates();
        UpdateRoomLighting();
        MovePlayerToDestination();
        UpdateUIAndVisuals();

        CancelWarp(); 
    }

    private void ResetEnvironmentStates()
    {
        // Reset navigasi waypoint sebelum warp (mencegah Ghost State Bug)
        if (WaypointManager.Instance != null) WaypointManager.Instance.ForceResetState();

        // Reset semua objek dither agar kembali solid (mencegah tembok transparan abadi)
        DitherTransparency[] allDithers = FindObjectsByType<DitherTransparency>(FindObjectsSortMode.None);
        foreach (DitherTransparency dither in allDithers)
        {
            dither.ForceSolid();
        }
    }

    private void UpdateRoomLighting()
    {
        // Atur cahaya seketika (Tanpa Fade) sesuai info ruangan baru
        if (currentRoomInfo != null && LightingController.Instance != null)
        {
            // Gunakan setting standar (Interior/Exterior)
            float targetIntensity = currentRoomInfo.isInterior ? 
                LightingController.Instance.interiorIntensity : 
                LightingController.Instance.defaultIntensity;

            LightingController.Instance.SetLightInstant(targetIntensity);

            // Kembalikan ke suhu warna default
            LightingController.Instance.SetTemperatureInstant(LightingController.Instance.defaultTemperature);
        }
    }

    private void MovePlayerToDestination()
    {
        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        playerTransform.position = currentDestination.position; 

        if (cc != null) cc.enabled = true;
    }

    private void UpdateUIAndVisuals()
    {
        if (currentRoomInfo == null) return;

        // Update Arah Hadap (Visual Override)
        PlayerVisual pv = playerTransform.GetComponentInChildren<PlayerVisual>();
        if (pv != null)
        {
            pv.ForceLookDirection(currentRoomInfo.spawnDirection);
        }

        // Update UI Nama Lokasi (Information Place)
        if (LocationUI.Instance != null)
        {
            LocationUI.Instance.TampilkanNamaLokasi(currentRoomInfo.roomName);
        }
    }

    /// <summary>
    /// Mencari ruangan di Scene berdasarkan nama dan langsung melakukan warp.
    /// Digunakan oleh fitur Re-Explore Quiz.
    /// </summary>
    public void WarpByRoomName(string roomName)
    {
        if (string.IsNullOrEmpty(roomName)) return;

        RoomInfo[] allRooms = FindObjectsByType<RoomInfo>(FindObjectsSortMode.None);
        foreach (RoomInfo room in allRooms)
        {
            if (room.roomName == roomName)
            {
                currentDestination = room.transform;
                currentRoomInfo = room;
                ExecuteWarp();
                return;
            }
        }

        Debug.LogWarning($"[WarpManager] Ruangan dengan nama '{roomName}' tidak ditemukan di Scene!");
    }

#if UNITY_EDITOR
    /// <summary>
    /// [DEBUG ONLY] Bypass seluruh sistem trigger dan melakukan warp instan.
    /// Script ini akan hilang saat Build Game.
    /// </summary>
    public void DebugTeleportTo(Transform targetRoom, RoomInfo roomInfo)
    {
        if (targetRoom == null)
        {
            Debug.LogError("[Debug Teleport] Target Room kosong! Gagal teleport.");
            return;
        }

        currentDestination = targetRoom;
        currentRoomInfo = roomInfo;
        
        ExecuteWarp();
        
        Debug.Log($"<color=cyan>[Debug] Berhasil Fast-Travel ke: {targetRoom.gameObject.name}</color>");
    }
#endif
}