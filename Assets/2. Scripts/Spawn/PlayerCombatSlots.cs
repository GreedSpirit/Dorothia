using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어 주변에 근접 몬스터가 
/// 접근할 위치 슬롯을 생성 및 관리하는 시스템
/// 
/// 플레이어 주변에 원형으로 N개의 슬롯 생성
/// 몬스터는 슬롯을 요청
/// 비어있는 슬롯만 점유 가능
/// 몬스터가 죽거나 사라지면 슬롯 반환
/// </summary>
public class PlayerCombatSlots : MonoBehaviour
{
    [SerializeField] private int _slotCount = 12; // 플레이어 주변 슬롯 수
    [SerializeField] private float radius = 1.5f; // 플레이어 중심에서 슬롯까지 거리

    private Transform[] _slots; // 생성된 슬롯 위치 배열

    //현재 어떤 몬스터가 어떤 슬롯을 갖고 있는지 관리
    private Dictionary<IMonster, int> _occupied = new();

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
    public Transform RequestSlof(IMonster monster)
    {
        //점유한 슬롯이 있다면 재사용
        if (_occupied.ContainsKey(monster))
            return _slots[_occupied[monster]];

        //빈 슬롯 찾기
        for (int i = 0; i < _slots.Length; i++)
        {
            //아무도 안쓰고 있다면
            if (!_occupied.ContainsValue(i))
            {
                //몬스터에게 슬롯 할당
                _occupied.Add(monster, i);
                return _slots[i];
            }
        }

        //빈 슬롯이 없으면 null 반환
        return null;
    }

    /// <summary>
    /// 몬스터가 사망하거나 전투에서 이탈할 때 호출
    /// </summary>
    /// <param name="monster"></param>
    public void ReleaseSlot(IMonster monster)
    {
        if (_occupied.ContainsKey(monster))
            _occupied.Remove(monster);
    }
}
