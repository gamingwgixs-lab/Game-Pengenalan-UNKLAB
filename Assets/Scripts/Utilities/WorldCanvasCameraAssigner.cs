using UnityEngine;

public class WorldCanvasCameraAssigner : MonoBehaviour
{
    private void Start()
    {
        AssignToAllCanvases();
    }

    private void Update()
    {
        // Tetap berjaga-jaga jika kamera berganti
        AssignToAllCanvases();
    }

    public void AssignToAllCanvases()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) mainCam = Object.FindAnyObjectByType<Camera>();

        if (mainCam == null) return;

        // Cari semua Canvas di dalam hierarki NPC ini
        Canvas[] allCanvases = GetComponentsInChildren<Canvas>(true);
        foreach (Canvas c in allCanvases)
        {
            if (c.renderMode == RenderMode.WorldSpace && c.worldCamera == null)
            {
                c.worldCamera = mainCam;
            }
        }
    }
}
