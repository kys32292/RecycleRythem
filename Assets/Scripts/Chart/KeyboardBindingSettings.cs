using System;
using UnityEngine;
using UnityEngine.InputSystem;

// 씬을 이동하거나 게임을 재실행해도 같은 키 설정을 사용합니다.
public static class KeyboardBindingSettings
{
    private const string AttackPreference = "Input.AttackKey";

    // 저장값이 없으면 Inspector의 기본 키를 사용하고 Esc나 잘못된 저장값은 제외합니다.
    public static Key GetAttackKey(Key fallback = Key.Space)
    {
        Key saved = (Key)PlayerPrefs.GetInt(AttackPreference, (int)fallback);
        // 잘못된 저장값도 기본 입력인 스페이스바로 복구합니다.
        return IsBindable(saved) ? saved : Key.Space;
    }

    // 취소 전용인 Esc는 UI뿐 아니라 저장 단계에서도 차단합니다.
    public static bool IsBindable(Key key)
    {
        return key != Key.None && key != Key.Escape && Enum.IsDefined(typeof(Key), key);
    }

    public static bool SetAttackKey(Key key)
    {
        if (!IsBindable(key)) return false;
        PlayerPrefs.SetInt(AttackPreference, (int)key);
        PlayerPrefs.Save();
        return true;
    }

    // 숫자 키의 enum 이름(Digit1)과 실제 컨트롤 이름(1)의 차이도 처리합니다.
    public static string GetPath(Key key)
    {
        if (Keyboard.current != null) return "<Keyboard>/" + Keyboard.current[key].name;
        string name = key.ToString().ToLowerInvariant();
        if (name.StartsWith("digit")) name = name.Substring(5);
        return "<Keyboard>/" + name;
    }
}
