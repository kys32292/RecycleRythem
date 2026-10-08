using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// World-canvas label on the earth. Clicking it opens the screen-space detail image.
/// </summary>
public class EarthHotspot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private EarthDetailPanel panel;
    [Tooltip("클릭했을 때 화면 가운데에 나올 직사각형 이미지.")]
    [SerializeField] private Sprite detailImage;
    [Tooltip("이미지가 날아오는 방향. ESC를 누르면 반대쪽으로 나갑니다.")]
    [SerializeField] private SlideDirection enterFrom = SlideDirection.Left;

    [Tooltip("직사각형 이미지를 클릭했을 때 열 씬 이름.")]
    [SerializeField] private string sceneName = "Stage1";

    public void OnPointerClick(PointerEventData eventData)
    {
        if (panel == null)
            return;

        panel.Show(detailImage, enterFrom, sceneName);
    }
}
