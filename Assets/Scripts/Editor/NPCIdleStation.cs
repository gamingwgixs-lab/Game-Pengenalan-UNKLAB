using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class NPCIdleStation : EditorWindow
{
    private TipeVisualNPC defaultRole = TipeVisualNPC.Pemandu;
    private string baseOutputPath = "Assets/Animations/Character/NPC sampingan";
    private GameObject npcBaseTemplate;
    private int frameRate = 12;

    [MenuItem("Window/NPC Factory/Idle Station")]
    public static void ShowWindow() { GetWindow<NPCIdleStation>("NPC Idle Station"); }

    private void OnEnable()
    {
        baseOutputPath = EditorPrefs.GetString("NPC_PATH_STABLE_V9", "Assets/Animations/Character/NPC sampingan");
        npcBaseTemplate = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Animations/Character/NPC sampingan/BaseArthur.prefab");
    }

    private void OnGUI()
    {
        GUILayout.Label("NPC SOUL FACTORY (MATERIAL HERITAGE EDITION)", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        npcBaseTemplate = (GameObject)EditorGUILayout.ObjectField("Master Template", npcBaseTemplate, typeof(GameObject), false);
        defaultRole = (TipeVisualNPC)EditorGUILayout.EnumPopup("Default Role", defaultRole);
        baseOutputPath = EditorGUILayout.TextField("Output Path", baseOutputPath);
        frameRate = EditorGUILayout.IntSlider("Framerate (FPS)", frameRate, 1, 60);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(20);

        if (Selection.objects.Length > 0 && Selection.objects[0] is Texture2D)
        {
            if (GUILayout.Button($"GENERATE {Selection.objects.Length} PERFECT NPC SOULS", GUILayout.Height(45))) ProcessBatch(Selection.objects);
        }
    }

    private void ProcessBatch(Object[] targets)
    {
        int count = 0;
        foreach (Object obj in targets)
        {
            if (obj is Texture2D tex)
            {
                if (ProcessNPC(tex, tex.name.Replace(" ", "_").Trim())) count++;
            }
        }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "NPC Lahir dengan Visual & Material yang Sempurna!", "Mantap");
    }

    private bool ProcessNPC(Texture2D tex, string npcName)
    {
        if (npcBaseTemplate == null) return false;

        string assetPath = AssetDatabase.GetAssetPath(tex);
        string npcFolderPath = Path.Combine(baseOutputPath, npcName).Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(npcFolderPath)) { Directory.CreateDirectory(npcFolderPath); AssetDatabase.ImportAsset(npcFolderPath); }

        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        List<Sprite> sprites = new List<Sprite>();
        foreach (var a in allAssets) if (a is Sprite s) sprites.Add(s);
        if (sprites.Count < 2) return false;

        AnimationClip clip = CreateClip(npcName, npcFolderPath, sprites.GetRange(0, Mathf.Min(13, sprites.Count)));
        var ctrl = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath($"{npcFolderPath}/{npcName}_Controller.controller");
        ctrl.layers[0].stateMachine.AddState("Idle").motion = clip;

        // CLONE & UNPACK
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(npcBaseTemplate);
        instance.name = npcName;
        PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        // --- NUCLEAR HERITAGE LOGIC ---
        Transform modelTransform = instance.transform.Find("GO/Character/Model");
        if (modelTransform != null)
        {
            SpriteRenderer oldSR = modelTransform.GetComponent<SpriteRenderer>();
            Animator oldAnim = modelTransform.GetComponent<Animator>();
            
            Material templateMaterial = null;
            if (oldSR) templateMaterial = oldSR.sharedMaterial; // CATAT MATERIAL ARTHUR

            // Hapus SR dan Anim lama
            if (oldSR) DestroyImmediate(oldSR);
            if (oldAnim) DestroyImmediate(oldAnim);

            // Pasang Baru
            SpriteRenderer newSR = modelTransform.gameObject.AddComponent<SpriteRenderer>();
            newSR.sprite = sprites[0];
            if (templateMaterial != null) newSR.sharedMaterial = templateMaterial; // PASANG MATERIAL ARTHUR KE NPC BARU
            
            Animator newAnim = modelTransform.gameObject.AddComponent<Animator>();
            newAnim.runtimeAnimatorController = ctrl;
            newAnim.avatar = null;
        }

        // Inject Dialogue
        NpcDialogue diag = instance.GetComponent<NpcDialogue>();
        if (diag) { diag.npcName = npcName; diag.npcPortrait = sprites[0]; diag.tipeVisual = defaultRole; }

        // Smart Wiring
        Transform bubble = instance.transform.Find("GO/Character/NPC_Canvas/UI_Bubble");
        if (bubble)
        {
            if (!instance.GetComponent<WorldCanvasCameraAssigner>()) instance.AddComponent<WorldCanvasCameraAssigner>();
            NpcInteractionButton bs = bubble.GetComponent<NpcInteractionButton>() ?? bubble.gameObject.AddComponent<NpcInteractionButton>();
            Button btn = bubble.GetComponent<Button>();
            if (btn) {
                while (btn.onClick.GetPersistentEventCount() > 0) UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn.onClick, 0);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, bs.TriggerDialogue);
            }
        }

        PrefabUtility.SaveAsPrefabAsset(instance, $"{npcFolderPath}/{npcName}.prefab");
        DestroyImmediate(instance);
        return true;
    }

    private AnimationClip CreateClip(string n, string p, List<Sprite> s)
    {
        AnimationClip c = new AnimationClip { frameRate = frameRate };
        EditorCurveBinding b = new EditorCurveBinding { type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite" };
        ObjectReferenceKeyframe[] k = new ObjectReferenceKeyframe[s.Count + 1];
        for (int i = 0; i < s.Count; i++) k[i] = new ObjectReferenceKeyframe { time = i * (1.0f / frameRate), value = s[i] };
        k[s.Count] = new ObjectReferenceKeyframe { time = s.Count * (1.0f / frameRate), value = s[s.Count - 1] };
        AnimationUtility.SetObjectReferenceCurve(c, b, k);
        var set = AnimationUtility.GetAnimationClipSettings(c); set.loopTime = true; AnimationUtility.SetAnimationClipSettings(c, set);
        AssetDatabase.CreateAsset(c, $"{p}/{n}_Idle.anim");
        return c;
    }
}
