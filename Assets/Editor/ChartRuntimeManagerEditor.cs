using UnityEditor;
using UnityEngine;

// 같은 경로 설정을 일반 모드에서는 단일 항목, 리믹스에서는 배열로 편집합니다.
[CustomEditor(typeof(ChartRuntimeManager))]
[CanEditMultipleObjects]
public class ChartRuntimeManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty remixMode = serializedObject.FindProperty("remixMode");

        // 최상단에서 모드를 선택합니다. 재생 중에는 경로와 모드를 바꾸지 못하게 합니다.
        using (new EditorGUI.DisabledScope(Application.isPlaying))
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(remixMode, new GUIContent("리믹스 모드", "끄면 기존 단일 경로 모드입니다."));
            if (EditorGUI.EndChangeCheck())
            {
                // 다중 선택도 각 오브젝트 자신의 기존 값을 옮깁니다. 다른 대상의 연결을 복사하지 않습니다.
                bool enabled = remixMode.boolValue;
                foreach (Object selected in targets)
                    SwitchMode(new SerializedObject(selected), enabled);
                serializedObject.Update();
                remixMode = serializedObject.FindProperty("remixMode");
            }
        }

        if (remixMode.hasMultipleDifferentValues)
        {
            EditorGUILayout.HelpBox("모드가 다른 오브젝트를 선택했습니다. 경로는 개별 선택하여 편집하세요.", MessageType.Info);
        }
        else
        {
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                if (remixMode.boolValue)
                {
                    EditorGUILayout.HelpBox("기존 스포너와 히트 박스는 Element 0으로 이어집니다. Size를 늘려 경로를 추가하세요.\nElement 번호 = 채보 lane 번호(0부터). 모든 경로는 같은 입력 키를 사용합니다.", MessageType.Info);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("remixLanes"), new GUIContent("리믹스 스포너 / 히트 박스"), true);
                }
                else
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnPoint"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("judgePoint"), new GUIContent("Hit Box (Judge Point)"));
                }
            }
        }

        // 채보/오디오/판정 시간 등 공통 옵션은 기존 직렬화 필드를 그대로 편집합니다.
        DrawPropertiesExcluding(serializedObject, "m_Script", "remixMode", "remixLanes", "spawnPoint", "judgePoint");
        serializedObject.ApplyModifiedProperties();
    }

    // 모드 전환과 참조 이동을 함께 직렬화하여 Undo/Redo 및 프리팹 오버라이드를 지원합니다.
    private static void SwitchMode(SerializedObject settings, bool enabled)
    {
        settings.Update();
        SerializedProperty mode = settings.FindProperty("remixMode");
        if (mode.boolValue == enabled) return;

        SerializedProperty lanes = settings.FindProperty("remixLanes");
        SerializedProperty spawn = settings.FindProperty("spawnPoint");
        SerializedProperty judge = settings.FindProperty("judgePoint");

        if (enabled)
        {
            // 처음 켤 때 배열을 준비하고 단일 경로를 첫 항목에 그대로 옮깁니다.
            // 이미 추가한 두 번째 이후 경로와 레인별 프리팹 설정은 보존합니다.
            if (lanes.arraySize == 0) lanes.arraySize = 1;
            SerializedProperty first = lanes.GetArrayElementAtIndex(0);
            first.FindPropertyRelative("spawnPoint").objectReferenceValue = spawn.objectReferenceValue;
            first.FindPropertyRelative("hitBox").objectReferenceValue = judge.objectReferenceValue;
            lanes.isExpanded = true;
            first.isExpanded = true;
        }
        else if (lanes.arraySize > 0)
        {
            // 다시 끄면 첫 경로의 수정 내용을 단일 항목으로 가져옵니다.
            // 나머지 경로는 삭제하지 않아 다시 켰을 때 재등록할 필요가 없습니다.
            SerializedProperty first = lanes.GetArrayElementAtIndex(0);
            spawn.objectReferenceValue = first.FindPropertyRelative("spawnPoint").objectReferenceValue;
            judge.objectReferenceValue = first.FindPropertyRelative("hitBox").objectReferenceValue;
        }

        mode.boolValue = enabled;
        settings.ApplyModifiedProperties();
    }
}
