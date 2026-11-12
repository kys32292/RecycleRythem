using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the dirty biome image until that stage is cleared, then swaps in the clean image.
/// </summary>
public class HotspotClearSprite : MonoBehaviour
{
    [SerializeField] private string stageSceneName = "Stage1";
    [SerializeField] private Image background;
    [SerializeField] private Sprite dirtySprite;
    [SerializeField] private Sprite cleanSprite;

    private void OnEnable()
    {
        Apply();
    }

    public void Apply()
    {
        if (background == null)
            background = GetComponent<Image>();

        if (background == null)
            return;

        var cleared = StageClearStore.IsCleared(stageSceneName);
        var sprite = cleared && cleanSprite != null ? cleanSprite : dirtySprite;
        if (sprite == null)
            return;

        background.sprite = sprite;
        background.color = Color.white;
        background.preserveAspect = true;
    }
}
