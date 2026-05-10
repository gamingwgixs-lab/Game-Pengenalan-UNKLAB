using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(QuestManager))]
public class QuestManagerEditor : Editor
{
    private int jumpIndex = 0;

    // Cache GUIStyles
    private GUIStyle headerStyle;
    private GUIStyle subHeaderStyle;
    private GUIStyle infoBoxStyle;
    private GUIStyle statusLabelStyle;
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
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(8, 8, 6, 6)
        };

        statusLabelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            fontSize = 10
        };

        stylesInitialized = true;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (!Application.isPlaying) return;

        QuestManager manager = (QuestManager)target;
        if (manager.questDatabase == null || manager.allQuests.Count == 0) return;

        InitStyles();

        EditorGUILayout.Space(20);

        // ============================================
        // PANEL UTAMA
        // ============================================
        Color defaultBg = GUI.backgroundColor;
        Color defaultContent = GUI.contentColor;

        // Garis pemisah
        Rect separatorRect = EditorGUILayout.GetControlRect(false, 2);
        EditorGUI.DrawRect(separatorRect, new Color(0.15f, 0.7f, 0.9f, 0.8f));

        EditorGUILayout.Space(4);

        // Header
        EditorGUILayout.LabelField("QUEST NAVIGATOR", headerStyle);
        EditorGUILayout.LabelField("Runtime Debug Tool", subHeaderStyle);

        EditorGUILayout.Space(2);

        // Garis pemisah tipis
        Rect thinSep = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(thinSep, new Color(0.3f, 0.3f, 0.3f));

        EditorGUILayout.Space(8);

        // ============================================
        // INFO QUEST AKTIF
        // ============================================
        int currentIndex = manager.DebugGetActiveQuestIndex();
        int totalQuests = manager.allQuests.Count;
        string currentName = manager.activeQuest != null
            ? manager.activeQuest.judulQuest
            : "Tidak ada quest aktif";

        // Progress bar visual
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField("QUEST AKTIF", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.Space(2);

        // Box info quest
        GUI.backgroundColor = new Color(0.18f, 0.18f, 0.22f);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = defaultBg;

        if (currentIndex >= 0)
        {
            EditorGUILayout.LabelField($"[{currentIndex}] {currentName}", infoBoxStyle);

            // Progress bar
            EditorGUILayout.Space(4);
            float progress = (float)(currentIndex + 1) / totalQuests;
            Rect progressRect = EditorGUILayout.GetControlRect(false, 16);

            // Background
            EditorGUI.DrawRect(progressRect, new Color(0.15f, 0.15f, 0.15f));

            // Fill
            Rect fillRect = new Rect(progressRect.x, progressRect.y, progressRect.width * progress, progressRect.height);
            Color progressColor = Color.Lerp(new Color(0.2f, 0.7f, 1f), new Color(0.3f, 1f, 0.5f), progress);
            EditorGUI.DrawRect(fillRect, progressColor);

            // Text overlay
            EditorGUI.LabelField(progressRect, $"{currentIndex + 1} / {totalQuests}", statusLabelStyle);
        }
        else
        {
            EditorGUILayout.LabelField("- Belum ada quest aktif -", infoBoxStyle);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(8);

        // ============================================
        // NAVIGASI: PREVIOUS / NEXT
        // ============================================
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("NAVIGASI", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.Space(4);

        EditorGUILayout.BeginHorizontal();

        // Previous Button
        EditorGUI.BeginDisabledGroup(currentIndex <= 0);
        GUI.backgroundColor = new Color(0.6f, 0.85f, 1f);
        if (GUILayout.Button("\u25C0  Previous", GUILayout.Height(28)))
        {
            manager.DebugJumpToQuest(currentIndex - 1);
        }
        GUI.backgroundColor = defaultBg;
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(4);

        // Next Button
        EditorGUI.BeginDisabledGroup(currentIndex >= totalQuests - 1);
        GUI.backgroundColor = new Color(0.6f, 1f, 0.7f);
        if (GUILayout.Button("Next  \u25B6", GUILayout.Height(28)))
        {
            manager.DebugJumpToQuest(currentIndex + 1);
        }
        GUI.backgroundColor = defaultBg;
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(8);

        // ============================================
        // JUMP TO QUEST
        // ============================================
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("JUMP TO QUEST", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.Space(4);

        // Bangun dropdown list
        string[] questOptions = new string[totalQuests];
        for (int i = 0; i < totalQuests; i++)
        {
            Quest q = manager.allQuests[i];
            string status = "";
            if (i < currentIndex) status = "\u2713 ";
            else if (i == currentIndex) status = "\u25B6 ";

            questOptions[i] = $"{status}[{i}] " + (q != null ? q.judulQuest : "(Null)");
        }

        jumpIndex = Mathf.Clamp(jumpIndex, 0, totalQuests - 1);
        jumpIndex = EditorGUILayout.Popup("Target Quest", jumpIndex, questOptions);

        EditorGUILayout.Space(4);

        // Jump button
        EditorGUI.BeginDisabledGroup(jumpIndex == currentIndex);
        GUI.backgroundColor = new Color(1f, 0.85f, 0.3f);
        if (GUILayout.Button("JUMP!", GUILayout.Height(26)))
        {
            manager.DebugJumpToQuest(jumpIndex);
        }
        GUI.backgroundColor = defaultBg;
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(4);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(4);

        // Garis penutup
        Rect bottomSep = EditorGUILayout.GetControlRect(false, 2);
        EditorGUI.DrawRect(bottomSep, new Color(0.15f, 0.7f, 0.9f, 0.8f));

        EditorGUILayout.Space(5);

        // Force repaint agar progress bar selalu update
        if (Application.isPlaying) Repaint();
    }
}