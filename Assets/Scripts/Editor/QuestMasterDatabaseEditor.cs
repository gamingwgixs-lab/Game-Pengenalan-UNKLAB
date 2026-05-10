using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(QuestMasterDatabase))]
public class QuestMasterDatabaseEditor : Editor
{
    private QuestMasterDatabase database;
    private bool showFullDetails = true;
    private int amountToAdd = 1;

    private void OnEnable()
    {
        database = (QuestMasterDatabase)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Color defaultColor = GUI.backgroundColor;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("QUEST TIMELINE MONITORING", EditorStyles.boldLabel);
        database.catatanDeveloper = EditorGUILayout.TextArea(database.catatanDeveloper, GUILayout.Height(50));
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // BAGIAN SETTINGS START INDEX
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("PROTOTYPE SETTINGS", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        SerializedProperty startIdxProp = serializedObject.FindProperty("startQuestIndex");
        EditorGUILayout.PropertyField(startIdxProp, new GUIContent("Start from Index", "Game akan dimulai dari misi ini. Misi sebelumnya dianggap SELESAI."));
        if (startIdxProp.intValue < 0) startIdxProp.intValue = 0;
        if (startIdxProp.intValue >= database.allQuests.Count && database.allQuests.Count > 0) 
            startIdxProp.intValue = database.allQuests.Count - 1;
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        showFullDetails = EditorGUILayout.ToggleLeft("Show Quest Details", showFullDetails, GUILayout.Width(150));
        EditorGUILayout.LabelField($"Total Quests: {database.allQuests.Count}", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        DrawQuestList();

        EditorGUILayout.Space(10);

        // BAGIAN BATCH ADD
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("BATCH OPERATIONS", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        amountToAdd = EditorGUILayout.IntField("Amount to Add", amountToAdd);
        amountToAdd = Mathf.Clamp(amountToAdd, 1, 20); // Maksimal 20 sesuai permintaan
        
        if (GUILayout.Button($"Add {amountToAdd} Slots"))
        {
            SerializedProperty listProp = serializedObject.FindProperty("allQuests");
            for (int i = 0; i < amountToAdd; i++)
            {
                listProp.arraySize++;
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Force Validate All Chain Links", GUILayout.Height(30)))
        {
            ValidateQuestChain();
        }

        EditorGUILayout.Space(20);
        GUI.backgroundColor = new Color(1f, 0.3f, 0.3f); // Warna Merah Peringatan
        if (GUILayout.Button("DANGER: Clear All Quests Database", GUILayout.Height(35)))
        {
            ExecuteClearDatabaseWithDoubleConfirmation();
        }
        GUI.backgroundColor = defaultColor;

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed) EditorUtility.SetDirty(database);
    }

    private void ExecuteClearDatabaseWithDoubleConfirmation()
    {
        bool firstCheck = EditorUtility.DisplayDialog(
            "Clear Quest Database?", 
            "Apakah Anda yakin ingin menghapus seluruh daftar quest di database ini?", 
            "Ya, Lanjutkan", 
            "Batal"
        );

        if (firstCheck)
        {
            bool finalCheck = EditorUtility.DisplayDialog(
                "FINAL CONFIRMATION", 
                "Tindakan ini tidak bisa dibatalkan (Undo). Seluruh referensi quest akan dikosongkan. Anda benar-benar yakin?", 
                "SAYA YAKIN, HAPUS SEMUA", 
                "TIDAK, BATALKAN!"
            );

            if (finalCheck)
            {
                database.allQuests.Clear();
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                Debug.Log("<color=red>[Quest Database] Seluruh daftar quest telah dikosongkan.</color>");
            }
        }
    }

    private void DrawQuestList()
    {
        SerializedProperty listProp = serializedObject.FindProperty("allQuests");

        for (int i = 0; i < listProp.arraySize; i++)
        {
            SerializedProperty element = listProp.GetArrayElementAtIndex(i);
            Quest q = element.objectReferenceValue as Quest;

            Color defaultColor = GUI.backgroundColor;
            bool isBroken = false;
            
            if (i < listProp.arraySize - 1)
            {
                Quest nextInList = listProp.GetArrayElementAtIndex(i + 1).objectReferenceValue as Quest;
                if (q != null && q.nextQuest != nextInList)
                {
                    isBroken = true;
                    GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                }
            }

            // Highlight Start Index
            if (i == serializedObject.FindProperty("startQuestIndex").intValue)
            {
                GUI.backgroundColor = new Color(0.4f, 1f, 0.4f); // Hijau untuk Starting Point
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"[{i}]", GUILayout.Width(30));
            EditorGUILayout.PropertyField(element, GUIContent.none);
            
            // TOMBOL NAVIGASI & EDIT
            EditorGUI.BeginDisabledGroup(i == 0);
            if (GUILayout.Button("▲", EditorStyles.miniButtonLeft, GUILayout.Width(25)))
            {
                listProp.MoveArrayElement(i, i - 1);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(i == listProp.arraySize - 1);
            if (GUILayout.Button("▼", EditorStyles.miniButtonMid, GUILayout.Width(25)))
            {
                listProp.MoveArrayElement(i, i + 1);
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("➕", EditorStyles.miniButtonMid, GUILayout.Width(25)))
            {
                listProp.InsertArrayElementAtIndex(i);
                // Kosongkan agar tidak menduplikasi isi sebelumnya
                listProp.GetArrayElementAtIndex(i).objectReferenceValue = null;
            }

            if (GUILayout.Button("❌", EditorStyles.miniButtonRight, GUILayout.Width(25)))
            {
                // Hapus, jika referensi sebelumnya ada nilainya, Unity butuh dihapus 2 kali (Nulling, lalu Delete array)
                if (listProp.GetArrayElementAtIndex(i).objectReferenceValue != null)
                {
                    listProp.GetArrayElementAtIndex(i).objectReferenceValue = null;
                }
                listProp.DeleteArrayElementAtIndex(i);
                break; // Hentikan loop frame ini karena ukuran list berubah
            }

            if (isBroken)
            {
                EditorGUILayout.LabelField("⚠️ CHAIN BROKEN!", EditorStyles.boldLabel, GUILayout.Width(110));
            }
            EditorGUILayout.EndHorizontal();

            if (showFullDetails && q != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"Title: {q.judulQuest}", EditorStyles.miniBoldLabel);
                
                string givers = (q.npcGivers != null && q.npcGivers.Count > 0) ? string.Join(", ", q.npcGivers) : "None";
                string target = !string.IsNullOrEmpty(q.poiTarget) ? $"POI: {q.poiTarget}" : 
                                (q.npcTargets != null && q.npcTargets.Count > 0 ? $"NPC: {string.Join(", ", q.npcTargets)}" : "Unknown");
                
                EditorGUILayout.LabelField($"Giver: {givers} | Target: {target}", EditorStyles.miniLabel);
                
                if (q.nextQuest != null)
                    EditorGUILayout.LabelField($"Next Logic: {q.nextQuest.name}", EditorStyles.miniLabel);
                else
                    EditorGUILayout.LabelField("Next Logic: [END OF CHAIN]", EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            GUI.backgroundColor = defaultColor;
        }
    }

    private void ValidateQuestChain()
    {
        int issues = 0;
        for (int i = 0; i < database.allQuests.Count - 1; i++)
        {
            Quest current = database.allQuests[i];
            Quest next = database.allQuests[i + 1];

            if (current != null && current.nextQuest != next)
            {
                Debug.LogWarning($"[Quest Chain] Issue at Index {i}: '{current.name}' should link to '{next.name}'");
                issues++;
            }
        }

        if (issues == 0) Debug.Log("<color=green>[Quest Chain] All perfectly linked!</color>");
        else Debug.LogError($"[Quest Chain] Found {issues} broken links!");
    }
}
