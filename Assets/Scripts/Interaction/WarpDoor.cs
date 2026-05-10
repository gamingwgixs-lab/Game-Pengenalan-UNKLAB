using UnityEngine;

public class WarpDoor : MonoBehaviour
{
    [Tooltip("Titik pendaratan Empty GameObject di dalam ruangan untuk pintu INI saja")]
    public Transform destinationPoint;
    public RoomInfo destinationInfo; // Masukkan RoomInfo dari titik tujuan di sini

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WarpManager.Instance.SetupWarp(destinationPoint, destinationInfo);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WarpManager.Instance.CancelWarp();
        }
    }
}