using UnityEngine;

/// <summary>
/// Turns on this world-canvas image after the linked stage has been cleared.
/// </summary>
public class WorldStageClearImage : MonoBehaviour
{
    [SerializeField] private string stageSceneName = "Stage1";
    [SerializeField] private GameObject clearImage;

    private void OnEnable()
    {
        if (clearImage == null)
            return;

        clearImage.SetActive(StageClearStore.IsCleared(stageSceneName));
    }
}
