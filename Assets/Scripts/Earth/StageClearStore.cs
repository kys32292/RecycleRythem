using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Remembers cleared stages only for the current play.
/// Starting the game again clears them.
/// </summary>
public static class StageClearStore
{
    private const string Prefix = "stage-clear:";
    private static readonly string[] SceneNames = { "Stage1", "Stage2", "Stage3", "Stage4", "Stage5" };
    private static readonly HashSet<string> Cleared = new HashSet<string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetForNewSession()
    {
        Cleared.Clear();
        for (int i = 0; i < SceneNames.Length; i++)
            PlayerPrefs.DeleteKey(Prefix + SceneNames[i]);
        PlayerPrefs.Save();
    }

    public static void MarkCleared(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        Cleared.Add(sceneName);
    }

    public static bool IsCleared(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return false;

        return Cleared.Contains(sceneName);
    }
}
