using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Loads the earth main scene when this result line is clicked.
/// </summary>
[RequireComponent(typeof(Button))]
public class ReturnToMainButton : MonoBehaviour
{
    [SerializeField] private string sceneName = "Main테스트";

    private void Awake()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(GoToMain);
    }

    public void GoToMain()
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        SceneManager.LoadScene(sceneName);
    }
}
