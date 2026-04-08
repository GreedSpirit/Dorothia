using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class AddressableManager : MonoBehaviour
{
    public static AddressableManager Instance;

    // 비제네릭 → 제네릭 핸들로 변경해 unsafe 캐스팅 제거
    private Dictionary<string, AsyncOperationHandle> _assetCache = new();
    private Dictionary<string, int> _refCounts = new();

    // 로드 완료 전 대기 콜백을 주소별로 모아서 관리
    private Dictionary<string, List<Action<object>>> _pendingCallbacks = new();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void LoadAsset<T>(string address, Action<T> onComplete = null) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(address)) return;
        address = address.Trim();

        // 캐시 히트
        if (_assetCache.TryGetValue(address, out AsyncOperationHandle handle))
        {
            _refCounts[address]++;

            // 이미 완료된 경우 → 즉시 콜백 호출
            if (handle.IsDone)
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    onComplete?.Invoke(handle.Result as T);
                else
                    Debug.LogError($"[Addressable] 캐시된 핸들 실패 상태: {address}");
                return;
            }

            // 아직 로드 중인 경우 → 대기 목록에 추가
            if (!_pendingCallbacks.ContainsKey(address))
                _pendingCallbacks[address] = new List<Action<object>>();

            _pendingCallbacks[address].Add(result => onComplete?.Invoke(result as T));
            return;
        }

        // 신규 로드
        var newHandle = Addressables.LoadAssetAsync<T>(address);
        _assetCache[address] = newHandle;
        _refCounts[address] = 1;
        _pendingCallbacks[address] = new List<Action<object>>();

        // 최초 요청 콜백도 대기 목록에 추가해 통합 처리
        if (onComplete != null)
            _pendingCallbacks[address].Add(result => onComplete?.Invoke(result as T));

        newHandle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                // 대기 중인 모든 콜백 일괄 호출
                if (_pendingCallbacks.TryGetValue(address, out var callbacks))
                {
                    foreach (var cb in callbacks)
                        cb?.Invoke(op.Result);

                    _pendingCallbacks.Remove(address);
                }
            }
            else
            {
                Debug.LogError($"[Addressable] 로드 실패: {address}\n원인: {op.OperationException}");

                // null로 콜백 호출해서 EffectManager가 _loadingAssets 정리할 수 있게
                if (_pendingCallbacks.TryGetValue(address, out var callbacks))
                {
                    foreach (var cb in callbacks)
                        cb?.Invoke(null);
                    _pendingCallbacks.Remove(address);
                }

                _assetCache.Remove(address);
                _refCounts.Remove(address);
                if (newHandle.IsValid()) Addressables.Release(newHandle);
            }
        };
    }

    public void ReleaseAsset(string address)
    {
        if (string.IsNullOrEmpty(address)) return;
        address = address.Trim();

        if (!_assetCache.TryGetValue(address, out AsyncOperationHandle handle)) return;

        _refCounts[address]--;

        if (_refCounts[address] <= 0)
        {
            if (handle.IsValid()) Addressables.Release(handle);
            _assetCache.Remove(address);
            _refCounts.Remove(address);
            _pendingCallbacks.Remove(address); // 혹시 남은 대기 콜백도 정리
        }
    }

    public void ClearCache()
    {
        foreach (var handle in _assetCache.Values)
            if (handle.IsValid()) Addressables.Release(handle);

        _assetCache.Clear();
        _refCounts.Clear();
        _pendingCallbacks.Clear();
    }
}