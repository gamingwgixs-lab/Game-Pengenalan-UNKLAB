using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Editor untuk QuizUI.
/// Menambahkan tombol preview panel hasil langsung di Inspector tanpa perlu Play.
/// Toggle checkbox per soal untuk melihat kombinasi Passed/Not Passed.
/// Warna otomatis terupdate saat diubah di Inspector.
/// </summary>
[CustomEditor(typeof(QuizUI))]
public class QuizUIEditor : Editor
{
    private bool[] previewData = new bool[10];
    private bool showPreviewSection = false;
    private bool isPreviewActive = false;

    public override void OnInspectorGUI()
    {
        QuizUI quizUI = (QuizUI)target;

        // Deteksi perubahan pada Inspector (termasuk warna)
        EditorGUI.BeginChangeCheck();

        // Gambar Inspector default
        DrawDefaultInspector();

        // Jika ada perubahan dan preview sedang aktif, refresh otomatis
        if (EditorGUI.EndChangeCheck() && isPreviewActive)
        {
            quizUI.PreviewHasil(previewData);
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // ═══════════════════════════════════════════
        //  PREVIEW PANEL HASIL (tanpa Play)
        // ═══════════════════════════════════════════

        showPreviewSection = EditorGUILayout.Foldout(showPreviewSection, "🔍 PREVIEW PANEL HASIL", true, EditorStyles.foldoutHeader);

        if (showPreviewSection)
        {
            EditorGUILayout.HelpBox(
                "Ubah warna di atas, lalu gunakan tombol di bawah untuk preview. " +
                "Warna akan langsung terupdate saat diubah.",
                MessageType.Info
            );

            EditorGUILayout.Space(5);

            // Checkbox per soal
            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < previewData.Length; i++)
            {
                previewData[i] = EditorGUILayout.Toggle($"Soal {i + 1}: Passed?", previewData[i]);
            }
            // Jika checkbox berubah dan preview aktif, refresh
            if (EditorGUI.EndChangeCheck() && isPreviewActive)
            {
                quizUI.PreviewHasil(previewData);
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space(5);

            // Hitung skor preview
            int previewScore = 0;
            foreach (bool b in previewData) if (b) previewScore++;
            EditorGUILayout.LabelField($"Skor Preview: {previewScore}/10", EditorStyles.boldLabel);

            EditorGUILayout.Space(5);

            // Tombol aksi
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("✓ Set Semua Passed", GUILayout.Height(30)))
            {
                for (int i = 0; i < previewData.Length; i++) previewData[i] = true;
                isPreviewActive = true;
                quizUI.PreviewHasil(previewData);
                SceneView.RepaintAll();
            }

            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
            if (GUILayout.Button("✗ Set Semua Not Passed", GUILayout.Height(30)))
            {
                for (int i = 0; i < previewData.Length; i++) previewData[i] = false;
                isPreviewActive = true;
                quizUI.PreviewHasil(previewData);
                SceneView.RepaintAll();
            }

            GUI.backgroundColor = new Color(0.3f, 0.6f, 0.9f);
            if (GUILayout.Button("🎲 Acak Random", GUILayout.Height(30)))
            {
                for (int i = 0; i < previewData.Length; i++) previewData[i] = Random.value > 0.5f;
                isPreviewActive = true;
                quizUI.PreviewHasil(previewData);
                SceneView.RepaintAll();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            // Tombol Sembunyikan
            GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
            if (GUILayout.Button("■ Sembunyikan Preview", GUILayout.Height(25)))
            {
                isPreviewActive = false;
                quizUI.HidePreview();
                SceneView.RepaintAll();
            }
            GUI.backgroundColor = Color.white;
        }
    }
}
