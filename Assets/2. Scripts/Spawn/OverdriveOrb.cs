using System.Collections;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;

public class OverdriveOrb : MonoBehaviour
{
    private MonsterSpawnManager _owner;

    //플레이어 관련 로직
    private OverDriveMode overDriveMode;
    private float duration = 1f;
    private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private const float ORB_VALUE = 10f;

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

        if (overDriveMode != null)
        {
            overDriveMode.Gauge += ORB_VALUE;
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
