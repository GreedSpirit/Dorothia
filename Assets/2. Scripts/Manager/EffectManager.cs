using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    private Dictionary<string, ObjectPool<GameObject>> _effectPools = new();

    // 로드 중인 주소 추적 (중복 로드 방지)
    private HashSet<string> _loadingAssets = new();

    // 로드 완료 전 대기 중인 스폰 요청
    private Dictionary<string, List<SpawnRequest>> _pendingSpawns = new();

    // WaitForSeconds 캐싱 (GC 방지)
    private Dictionary<float, WaitForSeconds> _waitCache = new();

    private readonly struct SpawnRequest
    {
        public readonly float Duration;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Transform Parent;

        public SpawnRequest(float d, Vector3 p, Quaternion r, Transform t)
        {
            Duration = d; Position = p; Rotation = r; Parent = t;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── PlayEffect ───────────────────────────────────────────────────

    public void PlayEffect(string effectName, float duration,
                           Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (string.IsNullOrEmpty(effectName)) return;

        // 풀이 이미 있으면 즉시 스폰
        if (_effectPools.TryGetValue(effectName, out var pool))
        {
            Spawn(pool, effectName, duration, position, rotation, parent);
            return;
        }

        // 대기 요청 등록
        if (!_pendingSpawns.ContainsKey(effectName))
            _pendingSpawns[effectName] = new List<SpawnRequest>();

        _pendingSpawns[effectName].Add(new SpawnRequest(duration, position, rotation, parent));

        // 이미 로드 중이면 대기만
        if (_loadingAssets.Contains(effectName)) return;

        _loadingAssets.Add(effectName);
        AddressableManager.Instance.LoadAsset<GameObject>(effectName, prefab =>
        {
            CreatePool(effectName, prefab);
            _loadingAssets.Remove(effectName);

            // 대기 중인 요청 일괄 처리
            if (_pendingSpawns.TryGetValue(effectName, out var requests))
            {
                var createdPool = _effectPools[effectName];
                foreach (var req in requests)
                    Spawn(createdPool, effectName, req.Duration, req.Position, req.Rotation, req.Parent);

                _pendingSpawns.Remove(effectName);
            }
        });
    }

    // ── 풀 생성 ──────────────────────────────────────────────────────

    private void CreatePool(string effectName, GameObject prefab)
    {
        _effectPools[effectName] = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(prefab),
            actionOnGet: OnGetFromPool,
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: obj => Destroy(obj),
            defaultCapacity: 5,
            maxSize: 15
        );
    }

    // ── 풀에서 꺼낼 때 초기화 ────────────────────────────────────────

    private void OnGetFromPool(GameObject obj)
    {
        obj.SetActive(true);

        foreach (var resettable in obj.GetComponentsInChildren<IResettableEffect>(true))
            resettable.ResetEffect();

        foreach (var ps in obj.GetComponentsInChildren<ParticleSystem>(true))
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

    private float GetEffectDuration(GameObject fx)
    {
        float maxDuration = 0f;

        foreach (var resettable in fx.GetComponentsInChildren<IResettableEffect>(true))
        {
            float d = resettable.GetEffectDuration();
            if (d > maxDuration) maxDuration = d;
        }

        if (maxDuration > 0f) return maxDuration;

        maxDuration = 2f;
        foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            float lifetime = main.duration + main.startLifetime.constantMax;
            if (lifetime > maxDuration) maxDuration = lifetime;
        }

        return maxDuration;
    }

    // ── 반환 코루틴 (WaitForSeconds 캐싱) ───────────────────────────

    private IEnumerator ReleaseRoutine(string name, GameObject obj, float duration)
    {
        // 소수점 첫째 자리로 반올림해 캐시 키 통일
        float key = Mathf.Round(duration * 10f) / 10f;

        if (!_waitCache.TryGetValue(key, out var wait))
        {
            wait = new WaitForSeconds(key);
            _waitCache[key] = wait;
        }

        yield return wait;

        if (_effectPools.TryGetValue(name, out var pool))
            pool.Release(obj);
    }

    // ── 정리 ─────────────────────────────────────────────────────────

    public void ClearAllPools()
    {
        foreach (var pool in _effectPools.Values) pool.Clear();
        _effectPools.Clear();
        _pendingSpawns.Clear();
        _loadingAssets.Clear();
        _waitCache.Clear();
    }
}