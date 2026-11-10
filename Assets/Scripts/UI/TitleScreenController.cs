using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Main";
    [SerializeField] private GameObject settingsPanel;

    public void OnClickStart()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnClickSettings()
    {
        if (settingsPanel == null) return;
        // 기존 씬과 프리팹에서도 별도 Inspector 연결 없이 키 설정 항목을 제공합니다.
        if (settingsPanel.GetComponent<KeyBindingSettingsPanel>() == null)
            settingsPanel.AddComponent<KeyBindingSettingsPanel>();
        settingsPanel.SetActive(true);
    }

    public void CloseSettingsPanel()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void SetMasterVolume(float value)
    {
        AudioListener.volume = value;
    }

    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
