using UnityEngine;

/// <summary>
/// Sets animator triggers from rhythm judgements.
/// Idle, hit, and miss are all triggers.
/// </summary>
public class RhythmAnimalAnimator : MonoBehaviour
{
    [SerializeField] private ChartRuntimeManager chart;
    [SerializeField] private Animator animator;

    [Header("Triggers")]
    [SerializeField] private string idleTrigger = "iDLE";
    [SerializeField] private string hitTrigger = "Event";
    [SerializeField] private string missTrigger = "failure";

    private int _idleHash;
    private int _hitHash;
    private int _missHash;
    private bool _waitingToReturnIdle;

    private void Reset()
    {
        animator = GetComponent<Animator>();
        if (chart == null)
            chart = FindFirstObjectByType<ChartRuntimeManager>();
    }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (chart == null)
            chart = FindFirstObjectByType<ChartRuntimeManager>();

        _idleHash = Animator.StringToHash(idleTrigger);
        _hitHash = Animator.StringToHash(hitTrigger);
        _missHash = Animator.StringToHash(missTrigger);
    }

    private void Start()
    {
        PlayIdle();
    }

    private void Update()
    {
        if (animator == null || animator.IsInTransition(0))
            return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (state.shortNameHash == _idleHash)
        {
            _waitingToReturnIdle = false;
            return;
        }

        if (_waitingToReturnIdle || state.normalizedTime < 1f)
            return;

        _waitingToReturnIdle = true;
        PlayIdle();
    }

    private void OnEnable()
    {
        if (chart == null)
            return;

        chart.NoteHit += PlayHit;
        chart.NoteMiss += PlayMiss;
    }

    private void OnDisable()
    {
        if (chart == null)
            return;

        chart.NoteHit -= PlayHit;
        chart.NoteMiss -= PlayMiss;
    }

    private void PlayIdle()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(_hitHash);
        animator.ResetTrigger(_missHash);
        animator.SetTrigger(_idleHash);
    }

    private void PlayHit()
    {
        if (animator == null)
            return;

        _waitingToReturnIdle = false;
        animator.ResetTrigger(_idleHash);
        animator.ResetTrigger(_missHash);
        animator.SetTrigger(_hitHash);
    }

    private void PlayMiss()
    {
        if (animator == null)
            return;

        _waitingToReturnIdle = false;
        animator.ResetTrigger(_idleHash);
        animator.ResetTrigger(_hitHash);
        animator.SetTrigger(_missHash);
    }
}
