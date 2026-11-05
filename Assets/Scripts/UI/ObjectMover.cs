using System.Collections;
using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    [Header("이동 설정")]
    public Vector3 moveOffset = new Vector3(0f, 3f, 0f);
    public float moveDuration = 1.0f;
    public float stayDuration = 2.0f;
    public float startDelay = 1.0f;

    [Header("옵션")]
    public bool smoothMovement = true;

    [Tooltip("체크하면 회전 관련 로직을 끄고, 오직 위치 이동(위아래 등)만 수행합니다.")]
    public bool onlyMoveVertical = true; // <-- 새로 추가된 인스펙터 옵션

    [Tooltip("체크하면 이동 방향을 바라보고, 도착 시 반대 방향으로 회전합니다.")]
    public bool lookAtMovement = true;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        StartCoroutine(MoveRoutine());
    }

    IEnumerator MoveRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            Vector3 targetPosition = originalPosition + moveOffset;
            Vector3 moveDirection = (targetPosition - originalPosition).normalized;

            // 1. 나갈 때 이동
            yield return StartCoroutine(MoveRoutine(targetPosition, moveDirection, moveDuration));

            // 도착 후 회전 옵션이 켜져있고, 수직 이동 모드가 아닐 때만 회전
            if (!onlyMoveVertical && lookAtMovement && moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(-moveDirection);
            }

            // 2. 도착한 곳에서 대기
            yield return new WaitForSeconds(stayDuration);

            // 3. 돌아올 때 이동
            Vector3 returnDir = (!onlyMoveVertical && lookAtMovement) ? -moveDirection : moveDirection;
            yield return StartCoroutine(MoveRoutine(originalPosition, returnDir, moveDuration));

            // 원래 자리에 도착 후 회전 옵션이 켜져있고, 수직 이동 모드가 아닐 때만 원위치 복귀
            if (!onlyMoveVertical && lookAtMovement)
            {
                transform.rotation = originalRotation;
            }

            // 4. 원래 자리에서 대기
            yield return new WaitForSeconds(stayDuration);
        }
    }

    IEnumerator MoveRoutine(Vector3 targetPos, Vector3 lookDir, float duration)
    {
        float time = 0f;
        Vector3 startPos = transform.position;

        // 수직 이동 모드가 아닐 때만 바라보기 회전 수행
        if (!onlyMoveVertical && lookAtMovement && lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // smoothMovement 여부에 따른 보간 적용
            float currentT = smoothMovement ? Mathf.SmoothStep(0f, 1f, t) : t;

            transform.position = Vector3.Lerp(startPos, targetPos, currentT);

            yield return null;
        }

        transform.position = targetPos;
    }
}