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
        if (settingsPanel != null) settingsPanel.SetActive(true);
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
