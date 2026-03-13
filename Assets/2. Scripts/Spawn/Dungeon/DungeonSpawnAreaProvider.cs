using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum DungeonSpecialPointType
{
    None = 0,
    PlayerSpawn = 1,
    BossSpawn = 2,
    Center = 3,
}

/// <summary>
/// 던전 전용 스폰 위치 제공
/// 고정위치 / 특수위치/ 랜덤위치를 모두 지원
/// 던전 룰이 "어디에 스폰할지"를 선택할 수 있게 함
/// </summary>
public class DungeonSpawnAreaProvider : MonoBehaviour
{
    [Header("Ordered Spawn Points")]
    [SerializeField] private List<Transform> _orderedSpawnPoints = new();

    [Header("Random Spawn Points")]
    [SerializeField] private List<Transform> _randomSpawnPoints = new();

    [Header("Special Points")]
    [SerializeField] private Transform _playerSpawnPoint;
    [SerializeField] private Transform _bossSpawnPoint;
    [SerializeField] private Transform _centerPoint;

    public int OrderedPointCount => 
        _orderedSpawnPoints != null ? _orderedSpawnPoints.Count : 0;
    public int RandomPointCount => 
        _randomSpawnPoints != null ? _randomSpawnPoints.Count : 0;

    /// <summary>
    /// 순서 고정 스폰 포인트
    /// </summary>
    /// <param name="index"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool TryGetOrderedPoint(int index, out Vector3 pos)
    {
        pos = Vector3.zero;

        if (_orderedSpawnPoints == null || _orderedSpawnPoints.Count == 0)
            return false;

        if (index < 0)
            index = 0;

        index %= _orderedSpawnPoints.Count;

        Transform transform = _orderedSpawnPoints[index];
        if (transform == null)
            return false;

        pos = transform.position;
        return true;
    }

    /// <summary>
    /// 랜덤포인트 중 하나 반환
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool TryGetRandomPoint(out Vector3 pos)
    {
        pos = Vector3.zero;

        if (_randomSpawnPoints == null || _randomSpawnPoints.Count == 0)
            return false;

        int index = Random.Range(0, _randomSpawnPoints.Count);
        Transform transform = _randomSpawnPoints[index];

        if (transform == null)
            return false;

        pos = transform.position;
        return true;
    }

    /// <summary>
    /// 특수 포인트 반환
    /// </summary>
    /// <param name="type"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool TryGetSpecialPoint(DungeonSpecialPointType type, out Vector3 pos)
    {
        pos = Vector3.zero;

        Transform target = null;

        switch (type)
        {
            case DungeonSpecialPointType.PlayerSpawn:
                target = _playerSpawnPoint;
                break;
            case DungeonSpecialPointType.BossSpawn:
                target = _bossSpawnPoint;
                break;
            case DungeonSpecialPointType.Center:
                target = _centerPoint;
                break;
        }

        if (target == null)
            return false;

        pos = target.position;
        return true;
    }

    #region NavMesh 보정
    public bool TryGetOrderedPointOnNavMesh(int index, out Vector3 pos)
    {
        if (TryGetOrderedPoint(index, out Vector3 rawPos))
        {
            if (NavMesh.SamplePosition(rawPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                pos = hit.position;
                return true;
            }

            pos = rawPos;
            return true;
        }

        pos = Vector3.zero;
        return false;
    }

    public bool TryGetRandomPointOnNavMesh(out Vector3 pos)
    {
        if (TryGetRandomPoint(out Vector3 rawPos))
        {
            if (NavMesh.SamplePosition(rawPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                pos = hit.position;
                return true;
            }

            pos = rawPos;
            return true;
        }

        pos = Vector3.zero;
        return false;
    }

    public bool TryGetSpecialPointOnNavMesh(DungeonSpecialPointType type, out Vector3 pos)
    {
        if (TryGetSpecialPoint(type, out Vector3 rawPos))
        {
            if (NavMesh.SamplePosition(rawPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                pos = hit.position;
                return true;
            }

            pos = rawPos;
            return true;
        }

        pos = Vector3.zero;
        return false;
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        //Ordered points
        if (_orderedSpawnPoints != null)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < _orderedSpawnPoints.Count; i++)
            {
                if (_orderedSpawnPoints[i] == null)
                    continue;

                Gizmos.DrawWireSphere(_orderedSpawnPoints[i].position, 0.35f);
            }
        }

        //Random points
        if (_randomSpawnPoints != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < _randomSpawnPoints.Count; i++)
            {
                if (_randomSpawnPoints[i] == null)
                    continue;

                Gizmos.DrawWireSphere(_randomSpawnPoints[i].position, 0.25f);
            }
        }

        //Special points
        Gizmos.color = Color.yellow;
        if (_playerSpawnPoint != null)
            Gizmos.DrawWireCube(_playerSpawnPoint.position, Vector3.one * 0.5f);

        Gizmos.color = Color.magenta;
        if (_bossSpawnPoint != null)
            Gizmos.DrawWireCube(_bossSpawnPoint.position, Vector3.one * 0.7f);

        Gizmos.color = Color.green;
        if (_centerPoint != null)
            Gizmos.DrawWireSphere(_centerPoint.position, 0.45f);
    }
#endif
}