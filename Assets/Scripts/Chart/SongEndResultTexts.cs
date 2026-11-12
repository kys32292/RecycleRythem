using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// After the end screen is fully black, reveals three success lines or three failure lines.
/// 51 and above uses the success lines. 50 and below uses the failure lines.
/// </summary>
public class SongEndResultTexts : MonoBehaviour
{
    [SerializeField] private PollutionManager pollution;
    [SerializeField] private Image black;
    [SerializeField] private TMP_Text[] successTexts;
    [SerializeField] private TMP_Text[] failureTexts;

    [Header("카메라")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("성공일 때 카메라가 이동할 위치와 방향. 씬에서 이 빈 오브젝트를 옮기면 됩니다.")]
    [SerializeField] private Transform successCameraPoint;
    [Tooltip("실패일 때 카메라가 이동할 위치와 방향. 씬에서 이 빈 오브젝트를 옮기면 됩니다.")]
    [SerializeField] private Transform failureCameraPoint;
    [Tooltip("화면이 완전히 어두워진 뒤 카메라가 목표 위치까지 이동하는 시간(초). 셋째 문구가 나오기 전에 도착합니다.")]
    [Min(0.01f)]
    [SerializeField] private float cameraMoveDuration = 1.2f;

    [Header("나타는 간격")]
    [Tooltip("화면이 완전히 어두워진 뒤 첫 문구까지 기다리는 시간(초).")]
    [Min(0f)]
    [SerializeField] private float firstDelay = 2f;

    [Tooltip("첫 문구가 나온 뒤 둘째 문구까지 기다리는 시간(초).")]
    [Min(0f)]
    [SerializeField] private float secondDelay = 2f;

    [Tooltip("둘째 문구가 나온 뒤 셋째 문구까지 기다리는 시간(초).")]
    [Min(0f)]
    [SerializeField] private float thirdDelay = 4f;

    [Min(0.01f)]
    [SerializeField] private float appearDuration = 0.4f;

    private Coroutine _routine;
    private Transform _cameraParent;
    private Transform _cameraPoint;
    private Vector3 _cameraLocalPosition;
    private Quaternion _cameraLocalRotation;
    private bool _cameraMoved;

    private void Awake()
    {
        if (pollution == null)
            pollution = FindFirstObjectByType<PollutionManager>();

        HideAll();
    }

    public void HideAll()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        Hide(successTexts);
        Hide(failureTexts);
        RestoreCamera();
    }

    public void Play()
    {
        HideAll();
        bool success = pollution != null && pollution.CurrentValue > 50;
        MoveCamera(success ? successCameraPoint : failureCameraPoint);
        _routine = StartCoroutine(PlayRoutine(success));
    }

    private IEnumerator PlayRoutine(bool success)
    {
        TMP_Text[] lines = success ? successTexts : failureTexts;
        float[] waits = { firstDelay, secondDelay, thirdDelay };
        int lastIndex = Mathf.Min(lines.Length, waits.Length) - 1;

        for (int i = 0; i < lines.Length && i < waits.Length; i++)
        {
            yield return new WaitForSecondsRealtime(waits[i]);
            Show(lines[i]);
            if (i == lastIndex)
                ClearBlack();
        }
    }

    private static void Hide(TMP_Text[] texts)
    {
        if (texts == null)
            return;

        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null)
                continue;

            text.DOKill();
            SetAlpha(text, 0f);
            text.gameObject.SetActive(false);
        }
    }

    private void Show(TMP_Text text)
    {
        if (text == null)
            return;

        text.gameObject.SetActive(true);
        SetAlpha(text, 0f);
        DOTween.ToAlpha(() => text.color, value => text.color = value, 1f, appearDuration)
            .SetUpdate(true)
            .SetLink(text.gameObject)
            .SetTarget(text);
    }

    private void ClearBlack()
    {
        if (black != null)
        {
            black.DOKill();
            black.raycastTarget = false;
            Color color = black.color;
            color.a = 0f;
            black.color = color;
        }

        PlaceCameraNow(_cameraPoint);
    }

    private void MoveCamera(Transform point)
    {
        _cameraPoint = point;
        if (cameraTransform == null || point == null)
            return;

        if (!_cameraMoved)
        {
            _cameraParent = cameraTransform.parent;
            _cameraLocalPosition = cameraTransform.localPosition;
            _cameraLocalRotation = cameraTransform.localRotation;
            _cameraMoved = true;
        }

        cameraTransform.DOKill();
        cameraTransform.SetParent(null, true);
        cameraTransform.DOMove(point.position, cameraMoveDuration).SetEase(Ease.InOutSine).SetUpdate(true);
        cameraTransform.DORotate(point.eulerAngles, cameraMoveDuration, RotateMode.Fast).SetEase(Ease.InOutSine).SetUpdate(true);
    }

    private void PlaceCameraNow(Transform point)
    {
        if (cameraTransform == null || point == null)
            return;

        cameraTransform.DOKill();
        cameraTransform.SetParent(null, true);
        cameraTransform.SetPositionAndRotation(point.position, point.rotation);
    }

    private void RestoreCamera()
    {
        if (!_cameraMoved || cameraTransform == null)
            return;

        cameraTransform.DOKill();
        cameraTransform.SetParent(_cameraParent, false);
        cameraTransform.localPosition = _cameraLocalPosition;
        cameraTransform.localRotation = _cameraLocalRotation;
        _cameraMoved = false;
    }

    private static void SetAlpha(TMP_Text text, float alpha)
    {
        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }
}
