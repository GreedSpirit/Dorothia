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

    // duration == 0 → 이펙트 자체 수명으로 자동 반환
    public void PlayEffect(string effectName, float duration,
                           Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (string.IsNullOrEmpty(effectName)) return;

        if (_effectPools.TryGetValue(effectName, out var pool))
            Spawn(pool, effectName, duration, position, rotation, parent);
        else
            LoadAndCreatePool(effectName, duration, position, rotation, parent);
    }

    // ── 풀 생성 ──────────────────────────────────────────────────────

    private void LoadAndCreatePool(string effectName, float duration,
                                   Vector3 pos, Quaternion rot, Transform parent)
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
                actionOnGet: OnGetFromPool,
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                defaultCapacity: 5,
                maxSize: 15
            );

            Spawn(_effectPools[effectName], effectName, duration, pos, rot, parent);
        };
    }

    // ── 풀에서 꺼낼 때 공통 초기화 ──────────────────────────────────

    /// <summary>
    /// IResettableEffect와 ParticleSystem을 모두 지원하는 통합 OnGet 핸들러.
    /// </summary>
    private void OnGetFromPool(GameObject obj)
    {
        obj.SetActive(true);

        // IResettableEffect 구현체 초기화 (MultipleObjectsMake 등)
        foreach (var resettable in obj.GetComponentsInChildren<IResettableEffect>())
            resettable.ResetEffect();

        // ParticleSystem 초기화 (잔상 제거 후 재생)
        foreach (var ps in obj.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Clear();
            ps.Play();
        }
    }

    // ── Spawn ────────────────────────────────────────────────────────

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

        float releaseTime = duration > 0f ? duration : GetEffectDuration(obj);
        StartCoroutine(ReleaseRoutine(name, obj, releaseTime));
    }

    // ── 지속 시간 계산 ───────────────────────────────────────────────

    /// <summary>
    /// IResettableEffect → ParticleSystem → 기본값 순으로 지속 시간을 결정한다.
    /// </summary>
    private float GetEffectDuration(GameObject fx)
    {
        // 1순위: IResettableEffect (MultipleObjectsMake 등 커스텀 이펙트)
        var resettable = fx.GetComponentInChildren<IResettableEffect>();
        if (resettable != null)
            return resettable.GetEffectDuration();

        // 2순위: ParticleSystem
        float maxDuration = 2f; // 컴포넌트가 없을 때 기본값
        foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>())
        {
            var main = ps.main;
            float lifetime = main.duration + main.startLifetime.constantMax;
            if (lifetime > maxDuration) maxDuration = lifetime;
        }
        return maxDuration;
    }

    // ── 반환 코루틴 ──────────────────────────────────────────────────

    private IEnumerator ReleaseRoutine(string name, GameObject obj, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (_effectPools.TryGetValue(name, out var pool))
            pool.Release(obj);
    }

    // ── 정리 ─────────────────────────────────────────────────────────

    public void ClearAllPools()
    {
        foreach (var pool in _effectPools.Values) pool.Clear();
        _effectPools.Clear();

        foreach (var handle in _assetHandles.Values) Addressables.Release(handle);
        _assetHandles.Clear();
    }
}