using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Main";
    [SerializeField] private GameObject settingsPanel;

    public void OnClickStart()
    {
        GameFeedback.Current?.PlayClick();
        FourCutIntro.ComingFromTitle = true;
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnClickSettings()
    {
        if (settingsPanel == null) return;
        GameFeedback.Current?.PlayClick();
        // 기존 씬과 프리팹에서도 별도 Inspector 연결 없이 키 설정 항목을 제공합니다.
        if (settingsPanel.GetComponent<KeyBindingSettingsPanel>() == null)
            settingsPanel.AddComponent<KeyBindingSettingsPanel>();
        settingsPanel.SetActive(true);
    }

    public void CloseSettingsPanel()
    {
        GameFeedback.Current?.PlayClick();
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void SetMasterVolume(float value)
    {
        AudioListener.volume = value;
    }

    public void OnClickQuit()
    {
        GameFeedback.Current?.PlayClick();
        DOVirtual.DelayedCall(0.1f, Quit, true).SetLink(gameObject);
    }

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
