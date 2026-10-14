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

            // 도착 후 회전 옵션이 켜져있다면 반대 방향으로 즉시 회전
            if (lookAtMovement && moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(-moveDirection);
            }

            // 2. 도착한 곳에서 대기
            yield return new WaitForSeconds(stayDuration);

            // 3. 돌아올 때 이동
            Vector3 returnDir = lookAtMovement ? -moveDirection : moveDirection;
            yield return StartCoroutine(MoveRoutine(originalPosition, returnDir, moveDuration));

            // 원래 자리에 도착 후 회전 옵션이 켜져있다면 원래 회전 값으로 즉시 복귀
            if (lookAtMovement)
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

        if (lookAtMovement && lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, targetPos, smoothT);

            yield return null;
        }

        transform.position = targetPos;
    }
}