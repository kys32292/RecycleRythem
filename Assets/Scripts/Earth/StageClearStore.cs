using UnityEngine;

/// <summary>
/// Remembers which stage scenes were cleared so the earth map can show them later.
/// </summary>
public static class StageClearStore
{
    private const string Prefix = "stage-clear:";

    public static void MarkCleared(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        PlayerPrefs.SetInt(Prefix + sceneName, 1);
        PlayerPrefs.Save();
    }

    public static bool IsCleared(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return false;

        return PlayerPrefs.GetInt(Prefix + sceneName, 0) == 1;
    }
}
