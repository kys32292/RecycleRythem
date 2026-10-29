using System;
using UnityEngine;

/// <summary>
/// 노트 판정과 오염도 수치를 분리합니다. ChartRuntimeManager는 타이밍을,
/// 이 컴포넌트는 수치와 곡 종료 결과를 담당합니다. ChartManager에 자동으로 추가됩니다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(ChartRuntimeManager))]
public class PollutionManager : MonoBehaviour
{
    public enum RoundResult
    {
        NotFinished,
        Failed,
        Clear,
        GreatSuccess
    }

    // 이름은 오염도지만, 요청한 규칙대로 성공하면 올라가고 높은 수치로 클리어합니다.
    // 중간에 0이 되어도 종료하지 않습니다. 최종 판정 시점은 음악이 끝났을 때입니다.
    public const int MaxValue = 100;
    public const int StartingValue = 50;
    public const int HitGain = 5;
    public const int MissLoss = 20;

    [SerializeField, Range(0, MaxValue)] private int currentValue = StartingValue;
    public int CurrentValue => currentValue;
    public RoundResult Result { get; private set; } = RoundResult.NotFinished;

    // UI는 ValueChanged를 구독해 게이지를 바꾸고, 결과 화면은 RoundFinished를 구독하면 됩니다.
    public event Action<int> ValueChanged;
    public event Action<RoundResult> RoundFinished;

    private ChartRuntimeManager chart;

    private void OnEnable()
    {
        chart = GetComponent<ChartRuntimeManager>();
        chart.PlaybackStarted += ResetValue;
        chart.NoteHit += HandleHit;
        chart.NoteMiss += HandleMiss;
        chart.PlaybackCompleted += FinishRound;
    }

    private void OnDisable()
    {
        if (chart == null)
        {
            return;
        }

        chart.PlaybackStarted -= ResetValue;
        chart.NoteHit -= HandleHit;
        chart.NoteMiss -= HandleMiss;
        chart.PlaybackCompleted -= FinishRound;
    }

    private void ResetValue()
    {
        Result = RoundResult.NotFinished;
        currentValue = StartingValue;
        ValueChanged?.Invoke(currentValue);
    }

    private void HandleHit()
    {
        // Great/Good만 성공입니다. 두 판정 모두 한 노트당 +5입니다.
        ChangeValue(HitGain);
    }

    private void HandleMiss()
    {
        // Fair 또는 시간 초과 Miss가 실패 이벤트를 보내며, 둘 다 -20입니다.
        // 노트가 없는 곳에서 누른 입력은 추가 감점하지 않습니다.
        ChangeValue(-MissLoss);
    }

    private void ChangeValue(int amount)
    {
        if (Result != RoundResult.NotFinished)
        {
            return;
        }

        // 성공/실패가 반복돼도 수치가 0 미만 또는 100 초과가 되지 않도록 제한합니다.
        currentValue = Mathf.Clamp(currentValue + amount, 0, MaxValue);
        ValueChanged?.Invoke(currentValue);
    }

    private void FinishRound()
    {
        if (Result != RoundResult.NotFinished)
        {
            return;
        }

        // 종료 규칙을 확정했습니다: 0~50은 실패, 51~80은 클리어, 81~100은 대성공입니다.
        // 80도 일반 클리어입니다. 높은 기준부터 검사해 대성공이 일반 클리어에 포함돼 가려지지 않게 합니다.
        // 대성공은 나중에 별도 연출/보상에 쓸 수 있도록 결과만 구분합니다.
        // 대성공 구분이 필요 없는 화면에서는 Clear와 GreatSuccess를 모두 클리어로 취급하면 됩니다.
        Result = currentValue > 80
            ? RoundResult.GreatSuccess
            : currentValue > 50 ? RoundResult.Clear : RoundResult.Failed;

        RoundFinished?.Invoke(Result);
        Debug.Log($"[Pollution] Song ended: {currentValue}/{MaxValue} -> {Result}");
    }
}
