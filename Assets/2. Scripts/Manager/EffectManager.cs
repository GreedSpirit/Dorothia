using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    // 풀과 함께 어드레서블 핸들을 관리하여 나중에 메모리 해제 시 사용
    private Dictionary<string, ObjectPool<GameObject>> _effectPools = new();
    private Dictionary<string, AsyncOperationHandle<GameObject>> _assetHandles = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void PlayEffect(string effectName, float duration, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (string.IsNullOrEmpty(effectName)) return;

        // 풀이 이미 있는지 확인
        if (_effectPools.TryGetValue(effectName, out var pool))
        {
            Spawn(pool, effectName, duration, position, rotation, parent);
        }
        else
        {
            // 풀이 없다면 어드레서블에서 에셋을 먼저 로드
            LoadAndCreatePool(effectName, duration, position, rotation, parent);
        }
    }

    private void LoadAndCreatePool(string effectName, float duration, Vector3 pos, Quaternion rot, Transform parent)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(effectName);
        _assetHandles[effectName] = handle;

        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject prefab = op.Result;

                // 풀 생성
                _effectPools[effectName] = new ObjectPool<GameObject>(
                    createFunc: () => Instantiate(prefab),
                    actionOnGet: (obj) => obj.SetActive(true),
                    actionOnRelease: (obj) => obj.SetActive(false),
                    actionOnDestroy: (obj) => Destroy(obj),
                    defaultCapacity: 5,
                    maxSize: 15
                );

                Spawn(_effectPools[effectName], effectName, duration, pos, rot, parent);
            }
            else
            {
                Debug.LogError($"이펙트 로드 실패: {effectName}");
            }
        };
    }

    private void Spawn(ObjectPool<GameObject> pool, string name, float duration, Vector3 pos, Quaternion rot, Transform parent)
    {
        GameObject obj = pool.Get();

        if (parent != null)
        {
            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }
        else
        {
            obj.transform.SetPositionAndRotation(pos, rot);
        }

        StartCoroutine(ReleaseRoutine(name, obj, duration));
    }

    private IEnumerator ReleaseRoutine(string name, GameObject obj, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (_effectPools.TryGetValue(name, out var pool))
        {
            pool.Release(obj);
        }
    }

    // 스테이지 전환 시나 게임 종료 시 메모리 완전 해제
    public void ClearAllPools()
    {
        foreach (var pool in _effectPools.Values)
        {
            pool.Clear(); // 모든 Instantiate된 오브젝트 파괴
        }
        _effectPools.Clear();

        foreach (var handle in _assetHandles.Values)
        {
            Addressables.Release(handle); // 어드레서블 에셋 메모리 해제
        }
        _assetHandles.Clear();
    }
}