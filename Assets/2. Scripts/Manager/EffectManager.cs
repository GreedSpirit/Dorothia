using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    private Dictionary<string, ObjectPool<GameObject>> _effectPools = new();
    private Dictionary<string, AsyncOperationHandle<GameObject>> _assetHandles = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // duration == 0 → 파티클 자체 수명으로 자동 반환
    public void PlayEffect(string effectName, float duration, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (string.IsNullOrEmpty(effectName)) return;

        if (_effectPools.TryGetValue(effectName, out var pool))
        {
            Spawn(pool, effectName, duration, position, rotation, parent);
        }
        else
        {
            LoadAndCreatePool(effectName, duration, position, rotation, parent);
        }
    }

    private void LoadAndCreatePool(string effectName, float duration, Vector3 pos, Quaternion rot, Transform parent)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(effectName);
        _assetHandles[effectName] = handle;

        handle.Completed += (op) =>
        {
            if (op.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"이펙트 로드 실패: {effectName}");
                return;
            }

            GameObject prefab = op.Result;
            _effectPools[effectName] = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(prefab),
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                defaultCapacity: 5,
                maxSize: 15
            );

            //_effectPools[effectName] = new ObjectPool<GameObject>(
            //createFunc: () => Instantiate(prefab),
            //actionOnGet: (obj) =>
            //{
            //    obj.SetActive(true);
            //    foreach (var ps in obj.GetComponentsInChildren<ParticleSystem>())
            //    {
            //        ps.Clear(); // 이전 잔상 제거
            //        ps.Play();  // 다시 재생
            //    }
            //},
            //actionOnRelease: (obj) => obj.SetActive(false),
            //actionOnDestroy: (obj) => Destroy(obj),
            //defaultCapacity: 5,
            //maxSize: 15
            //);

            Spawn(_effectPools[effectName], effectName, duration, pos, rot, parent);
        };
    }

    private void Spawn(ObjectPool<GameObject> pool, string name, float duration,
                       Vector3 pos, Quaternion rot, Transform parent)
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

        float releaseTime = duration > 0f ? duration : GetParticleDuration(obj);

        StartCoroutine(ReleaseRoutine(name, obj, releaseTime));
    }

    // 파티클 시스템의 실제 재생 시간 계산
    private float GetParticleDuration(GameObject fx)
    {
        float maxDuration = 2f; // 파티클 컴포넌트 없을 때 기본값

        foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>())
        {
            var main = ps.main;
            float lifetime = main.duration + main.startLifetime.constantMax;
            if (lifetime > maxDuration) maxDuration = lifetime;
        }

        return maxDuration;
    }

    private IEnumerator ReleaseRoutine(string name, GameObject obj, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (_effectPools.TryGetValue(name, out var pool))
            pool.Release(obj);
    }

    public void ClearAllPools()
    {
        foreach (var pool in _effectPools.Values) pool.Clear();
        _effectPools.Clear();

        foreach (var handle in _assetHandles.Values) Addressables.Release(handle);
        _assetHandles.Clear();
    }
}