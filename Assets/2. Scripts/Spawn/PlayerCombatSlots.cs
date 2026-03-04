using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어 주변에 근접 몬스터가 
/// 접근할 위치 슬롯을 생성 및 관리하는 시스템
/// 
/// 업그레이드 Combat Slot 시스템
/// - 각도 기반 슬롯
/// - 랜덤 오프셋
/// - 자연스러운 포지션 분산
/// </summary>
public class PlayerCombatSlots : MonoBehaviour
{
    [SerializeField] private int _slotCount = 12; // 플레이어 주변 슬롯 수
    [SerializeField] private float radius = 1.5f; // 플레이어 중심에서 슬롯까지 거리
    [SerializeField] private float randomOffset = 0.35f; // 슬롯 퍼짐 정도

    private Transform[] _slots; // 생성된 슬롯 위치 배열

    //현재 어떤 몬스터가 어떤 슬롯을 갖고 있는지 관리
    private Dictionary<IMonster, int> _occupied = new();

    private HashSet<int> _usedSlots = new();

    private void Awake()
    {
        //슬롯 배열 생성
        _slots = new Transform[_slotCount];

        //플레이어 주변에 원형으로 슬롯 생성
        for (int i = 0; i < _slotCount; i++)
        {
            GameObject gameObject = new GameObject("Slot_" + i);
            gameObject.transform.parent = transform;

            //360도를 슬록 개수만큼 분할
            float angle = (360f / _slotCount) * i;

            //원형 방향 벡터 계산
            Vector3 direction = 
                new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));

            //플레이어 기준 위치 설정
            gameObject.transform.localPosition = direction * radius;

            _slots[i] = gameObject.transform;
        }
    }

    /// <summary>
    /// 몬스터가 근접 공격 위치 슬롯을 요청할 때 호출
    /// </summary>
    /// <param name="monster"></param>
    /// <returns></returns>
    public Transform RequestSlot(IMonster monster)
    {
        //이미 슬롯이 있다면 재사용
        if (_occupied.TryGetValue(monster, out int slotIndex))
        {
            return _slots[slotIndex];
        }

        int bestIndex = -1;
        float bestDist = float.MaxValue;

        //빈 슬롯 찾기
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_usedSlots.Contains(i))
                continue;

            float dist =
                (monster.Transform.position - _slots[i].position).sqrMagnitude;

            if (dist < bestDist)
            {
                bestDist = dist;
                bestIndex = i;
            }
        }

        if (bestIndex != -1)
        {
            _occupied.Add(monster, bestIndex);
            _usedSlots.Add(bestIndex);

            ApplyRandomOffset(bestIndex);

            return _slots[bestIndex];
        }

        return null; // 빈슬롯 없으면 널반환
    }

    /// <summary>
    /// 슬롯 위치 랜덤 분산
    /// </summary>
    /// <param name="index"></param>
    private void ApplyRandomOffset(int index)
    {
        Vector3 random =
            new Vector3(
                Random.Range(-randomOffset, randomOffset),
                0,
                Random.Range(-randomOffset, randomOffset));

        _slots[index].localPosition += random;
    }

    public void ReleaseSlot(IMonster monster)
    {
        if (_occupied.TryGetValue(monster, out int index))
        {
            _occupied.Remove(monster);

            _usedSlots.Remove(index);

            //슬롯 위치 원래대로 복구
            ResetSlot(index);
        }
    }

    /// <summary>
    /// 슬롯 위치 초기화
    /// </summary>
    /// <param name="monster"></param>
    private void ResetSlot(int index)
    {
        float angle = (360f / _slotCount) * index;

        Vector3 dir =
            new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                0,
                Mathf.Sin(angle * Mathf.Deg2Rad));

        _slots[index].localPosition = dir * radius;
    }
}
