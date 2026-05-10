using UnityEngine;
using System.Collections.Generic;

public enum StatusQuest
{
    BelumMulai,
    SedangBerjalan,
    Selesai
}

[System.Serializable]
public class DialogTarget
{
    [Tooltip("Nama NPC yang bersangkutan.")]
    public string namaNPC;
    
    [TextArea(3, 5)]
    [Tooltip("Rangkaian kalimat khusus untuk NPC ini.")]
    public string[] kalimatDialog;
}



[CreateAssetMenu(fileName = "Quest_", menuName = "Scriptable Objects/Quest")]
public class Quest : ScriptableObject
{
    [Header("Informasi Dasar")]
    public string judulQuest;
    [TextArea(3, 5)]
    public string deskripsiQuest;

    [Header("Pengaturan Waypoint (Opsional)")]
    public bool useWaypoints;
    [Tooltip("Nama route di Scene (harus sama persis dengan routeName di komponen WaypointRoute).")]
    public string waypointRouteName;

    [Header("Pemberi Quest (Multiple Opsional)")]
    [Tooltip("Daftar NPC yang bisa memberikan misi ini.")]
    public List<string> npcGivers; 

    [Header("Target Quest (Multiple Opsional)")]
    [Tooltip("Daftar NPC yang bisa menerima penyelesaian misi ini.")]
    public List<string> npcTargets; 
    public string poiTarget; 

    [Header("Dialog Penerimaan Unik (Per Pemberi)")]
    [Tooltip("Daftar dialog khusus untuk masing-masing NPC pemberi.")]
    public List<DialogTarget> listDialogPenerimaan;

    [Header("Dialog Penyelesaian Unik (Per Target)")]
    [Tooltip("Daftar dialog khusus untuk masing-masing NPC target.")]
    public List<DialogTarget> listDialogPenyelesaian;

    [Header("Status")]
    public StatusQuest status = StatusQuest.BelumMulai;

    [Header("Chain Quest (Opsional)")]
    public Quest nextQuest;
}
