using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace QuestSystem
{
    [CustomPropertyDrawer(typeof(QuestStateStep))]
    public class QuestStateStepDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Inisialisasi referensi database dari QuestManager yang ada di hierarki Scene
            QuestMasterDatabase database = null;
            QuestManager managerInScene = Object.FindAnyObjectByType<QuestManager>();
            
            if (managerInScene != null)
            {
                database = managerInScene.questDatabase;
            }

            float lineH = EditorGUIUtility.singleLineHeight;
            Rect questSelectRect = new Rect(position.x, position.y, position.width, lineH);
            Rect statusRect = new Rect(position.x, position.y + lineH + 2, position.width / 2 - 5, lineH);
            Rect stateRect = new Rect(position.x + position.width / 2 + 5, position.y + lineH + 2, position.width / 2 - 5, lineH);

            SerializedProperty indexProp = property.FindPropertyRelative("questIndex");
            
            if (database != null && database.allQuests.Count > 0)
            {
                string[] options = new string[database.allQuests.Count];
                for (int i = 0; i < database.allQuests.Count; i++)
                {
                    Quest q = database.allQuests[i];
                    options[i] = $"[{i}] " + (q != null ? q.judulQuest : "(Null Quest)");
                }

                indexProp.intValue = EditorGUI.Popup(questSelectRect, "Target Quest", indexProp.intValue, options);
            }
            else
            {
                GUI.color = Color.yellow;
                EditorGUI.LabelField(questSelectRect, "Waiting for QuestManager in Scene...", EditorStyles.helpBox);
                GUI.color = Color.white;
                
                EditorGUI.PropertyField(questSelectRect, indexProp, new GUIContent("Index"));
            }

            EditorGUI.PropertyField(statusRect, property.FindPropertyRelative("triggerStatus"), GUIContent.none);
            
            float labelWidth = 50;
            Rect stateLabelRect = new Rect(stateRect.x, stateRect.y, labelWidth, lineH);
            Rect toggleRect = new Rect(stateRect.x + labelWidth, stateRect.y, stateRect.width - labelWidth, lineH);
            
            EditorGUI.LabelField(stateLabelRect, "Active:", EditorStyles.miniLabel);
            EditorGUI.PropertyField(toggleRect, property.FindPropertyRelative("targetState"), GUIContent.none);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 8;
        }
    }
}
