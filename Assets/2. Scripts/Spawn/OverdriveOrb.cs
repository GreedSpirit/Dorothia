using System.Collections;
using Unity.Hierarchy;
using UnityEngine;

public class OverdriveOrb : MonoBehaviour
{
    private MonsterSpawnManager _owner;

    //플레이어 관련 로직
    private OverDriveMode overDriveMode;
    private float duration = 1f;
    private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private void OnDisable()
    {
        StopAllCoroutines();
    }


    public void Setup(IMonsterTarget player)
    {
        if (overDriveMode == null)
        {
            if (player.Transform.TryGetComponent<OverDriveMode>(out OverDriveMode odm))
            {
                overDriveMode = odm;
            }
        }

        StopAllCoroutines();
        StartCoroutine(MoveOrbToPlayer());
    }

    public void SetOwner(MonsterSpawnManager owner)
    {
        _owner = owner;
    }

    public void Collect()
    {
        StopAllCoroutines();

        //todo : 1. 스테이지매니저를 싱글톤으로만든다
        //todo : 2. 셋업에서 스테이지매니저를 할당해준다
        //todo : 3. Find로 가져온다(비추천);;
        if (overDriveMode != null)
        {
            overDriveMode.Gauge += 30f;
        }

        if (_owner != null)
            _owner.ReleaseOrb(this);
    }

    IEnumerator MoveOrbToPlayer()
    {
        if (overDriveMode == null) yield break;

        float timer = 0;
        Vector3 startPos = transform.position;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            float curveValue = moveCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPos, overDriveMode.transform.position, curveValue);

            yield return null;
        }

        Collect();
    }
}
