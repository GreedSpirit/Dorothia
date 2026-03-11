using System.Collections;
using UnityEngine;

//그렘린의 실제 움직임을 제어하기 위한 클래스입니다.
public class GremlinMovement : MonoBehaviour
{
    [SerializeField] private Transform followTarget;                  // 추적할 대상
    [SerializeField] private Vector3 followOffset = new Vector3(1.5f, 2.0f, -1.0f);         // 추적할 때의 오프셋. 거리 조절용.
    [SerializeField] private float smoothTime = 0.3f;                   // 부드럽게 이동하기 위한 시간

    [SerializeField] private float floatStrength = 0.5f;                // 떠 있는 힘
    [SerializeField] private float floatSpeed = 2.0f;                   // 떠다니는 움직임 속도

    [SerializeField] private float teleportDistance = 10f;              // 너무 멀다고 판단하여 순간이동하기 위한 거리 오프셋

    private Vector3 currentVelocity;                                    // 플레이어의 움직임을 따라가기 위한, 현재의 위치?

    private bool isActing = false;

    private void Update()
    {
        HandleMovement();
    }

    public void Init(Transform target)
    {
        followTarget = target;
    }

    private void HandleMovement()
    {
        //추적할 대상이 없는 경우 반환합니다.
        if (followTarget == null)
        {
            return;
        }

        //특정 동작 시행 중에는 움직이지 않습니다.
        if(isActing == true)
        {
            return;
        }

        //만약, 플레이어와의 위치가 너무 멀어지는 경우, 지정된 위치로 순간 이동합니다.
        if (Vector3.Distance(followTarget.position, transform.position) > teleportDistance)
        {
            transform.position = followTarget.position + followOffset;
        }

        //대상의 위치를 확인, 그 대상의 위치에 오프셋만큼을 추가하여 이동 위치를 잡습니다.
        Vector3 targetPosition = followTarget.TransformPoint(followOffset);

        //smoothDamp를 활용한, 부드러운 추적. 그리고 Slerp를 통한 부드러운 회전.
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, followTarget.rotation, Time.deltaTime * 5f);
    }

    public void ChangeActingState()
    {
        StartCoroutine(ChangeActingState(0.8f));
    }

    public IEnumerator ChangeActingState(float waitSecond)
    {
        isActing = true;
        yield return new WaitForSeconds(waitSecond);

        isActing = false;
    }
}
