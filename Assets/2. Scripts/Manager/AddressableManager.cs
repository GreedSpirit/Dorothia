using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class AddressableManager : MonoBehaviour
{
    public static AddressableManager Instance;

    // 핸들 캐시
    private Dictionary<string, AsyncOperationHandle> _assetCache = new();
    // 참조 카운트
    private Dictionary<string, int> _refCounts = new();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void LoadAsset<T>(string address, Action<T> onComplete = null) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(address)) return;
        address = address.Trim(); // 공백 방지

        // 이미 캐시에 존재한다면 (로드 중이거나 완료됨)
        if (_assetCache.TryGetValue(address, out AsyncOperationHandle handle))
        {
            _refCounts[address]++;

            // Completed 이벤트는 이미 완료된 상태여도 다음 프레임에 호출되거나 
            // 즉시 실행되므로 통합 관리 가능
            handle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                    onComplete?.Invoke(op.Result as T);
            };
            return;
        }

        // 신규 로드 시작
        var newHandle = Addressables.LoadAssetAsync<T>(address);
        _assetCache[address] = newHandle;
        _refCounts[address] = 1;

        newHandle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                onComplete?.Invoke(op.Result as T);
            }
            else
            {
                Debug.LogError($"[Addressable] 로드 실패: {address}");
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
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            _assetCache.Remove(address);
            _refCounts.Remove(address);
        }
    }

    /// <summary>
    /// 강제 해제 (씬 전환 등 특수 상황용)
    /// </summary>
    public void ClearCache()
    {
        foreach (var handle in _assetCache.Values)
        {
            if (handle.IsValid()) Addressables.Release(handle);
        }
        _assetCache.Clear();
        _refCounts.Clear();
    }
}