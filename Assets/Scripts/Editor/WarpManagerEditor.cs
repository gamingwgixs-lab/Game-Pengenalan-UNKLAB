using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WarpManager))]
public class WarpManagerEditor : Editor
{
    private int selectedWarpIndex = 0;
    private WarpDoor[] availableDoors;
    private string[] dropDownOptions;

    private GUIStyle headerStyle;
    private GUIStyle subHeaderStyle;
    private GUIStyle infoBoxStyle;
    private bool stylesInitialized = false;

    private void InitStyles()
    {
        if (stylesInitialized) return;

        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(0, 0, 4, 4)
        };

        subHeaderStyle = new GUIStyle(EditorStyles.miniBoldLabel)
        {
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter
        };
        subHeaderStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);

        infoBoxStyle = new GUIStyle(EditorStyles.helpBox)
        {
            fontSize = 11,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(8, 8, 6, 6)
        };

        stylesInitialized = true;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (!Application.isPlaying) return;

        WarpManager manager = (WarpManager)target;
        InitStyles();

        EditorGUILayout.Space(20);

        // --- Main UI Panel ---
        Color defaultBg = GUI.backgroundColor;

        Rect separatorRect = EditorGUILayout.GetControlRect(false, 2);
        EditorGUI.DrawRect(separatorRect, new Color(0.7f, 0.2f, 0.9f, 0.8f));

        EditorGUILayout.Space(4);

        EditorGUILayout.LabelField("WARP TELEPORTER", headerStyle);
        EditorGUILayout.LabelField("Runtime Fast-Travel Tool", subHeaderStyle);

        EditorGUILayout.Space(2);

        // Force refresh WarpDoor references
        if (GUILayout.Button("🔄 Scan Pintu (Refresh)", GUILayout.Height(24)))
        {
            ScanDoors();
        }

        if (availableDoors == null || availableDoors.Length == 0)
        {
            ScanDoors(); // Fallback scan
        }

        EditorGUILayout.Space(8);

        // --- Dropdown Selection ---
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"Daftar Pintu di Scene ({availableDoors.Length})", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.Space(4);

        if (availableDoors.Length == 0)
        {
            EditorGUILayout.HelpBox("Tidak ada WarpDoor yang ditemukan di Scene ini.", MessageType.Warning);
            EditorGUILayout.EndVertical();
            return;
        }

        selectedWarpIndex = Mathf.Clamp(selectedWarpIndex, 0, availableDoors.Length - 1);
        selectedWarpIndex = EditorGUILayout.Popup("Target Lokasi", selectedWarpIndex, dropDownOptions);

        EditorGUILayout.Space(8);

        // --- Destination Info Panel ---
        WarpDoor selectedDoor = availableDoors[selectedWarpIndex];
        
        string destinationName = selectedDoor.destinationPoint != null 
            ? selectedDoor.destinationPoint.name 
            : "<color=red>KOSONG</color>";

        string roomInfoName = selectedDoor.destinationInfo != null 
            ? $"{selectedDoor.destinationInfo.roomName} ({(selectedDoor.destinationInfo.isInterior ? "Interior" : "Exterior")})" 
            : "<color=yellow>Tidak Ada / Default</color>";

        GUIStyle richTextInfoBox = new GUIStyle(infoBoxStyle);
        richTextInfoBox.richText = true;

        EditorGUILayout.LabelField("Detail Titik Pendaratan:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            $"<b>Point:</b> {destinationName}\n" +
            $"<b>Info :</b> {roomInfoName}",
            richTextInfoBox
        );

        EditorGUILayout.Space(8);

        // --- Teleport Action Button ---
        GUI.backgroundColor = new Color(0.9f, 0.7f, 0.1f);
        if (GUILayout.Button("🚀 TELEPORT SEKARANG!", GUILayout.Height(35)))
        {
            manager.DebugTeleportTo(selectedDoor.destinationPoint, selectedDoor.destinationInfo);
        }
        GUI.backgroundColor = defaultBg;

        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(4);
        
        Rect bottomSep = EditorGUILayout.GetControlRect(false, 2);
        EditorGUI.DrawRect(bottomSep, new Color(0.7f, 0.2f, 0.9f, 0.8f));
    }

    private void ScanDoors()
    {
        availableDoors = FindObjectsByType<WarpDoor>(FindObjectsSortMode.None);
        
        // Sortir alfabetikal berdasar nama GameObject untuk kemudahan dropdown
        System.Array.Sort(availableDoors, (a, b) => a.gameObject.name.CompareTo(b.gameObject.name));

        dropDownOptions = new string[availableDoors.Length];
        for (int i = 0; i < availableDoors.Length; i++)
        {
            WarpDoor door = availableDoors[i];
            
            string roomName = door.destinationInfo != null ? door.destinationInfo.roomName : "Lokasi Tak Dikenal";
            dropDownOptions[i] = $"{door.gameObject.name} ➔ {roomName}";
        }
    }
}
