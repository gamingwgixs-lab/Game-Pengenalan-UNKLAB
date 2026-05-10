using UnityEngine;

public enum SpawnDirection { NONE, UP, DOWN, LEFT, RIGHT }

public class RoomInfo : MonoBehaviour
{
    [Header("Informasi Ruangan")]
    public string roomName;
    public bool isInterior = false;

    [Header("Pengaturan Spawn")]
    [Tooltip("Arah hadap karakter saat pertama kali muncul di ruangan ini.")]
    public SpawnDirection spawnDirection = SpawnDirection.NONE;
}
