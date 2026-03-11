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

    public void LoadAsset<T>(string address, Action<T> onComplete) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(address)) return;

        // 1. 이미 캐시에 존재하고 유효한지 확인
        if (_assetCache.TryGetValue(address, out AsyncOperationHandle handle))
        {
            _refCounts[address]++;

            if (handle.IsDone)
            {
                // 이미 로드 완료됨
                onComplete?.Invoke(handle.Result as T);
            }
            else
            {
                // 로드 중: 완료 시점에 호출되도록 등록
                handle.Completed += (op) =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                        onComplete?.Invoke(op.Result as T);
                };
            }
            return;
        }

        // 2. 신규 로드 시작
        var newHandle = Addressables.LoadAssetAsync<T>(address);
        _assetCache[address] = newHandle;
        _refCounts[address] = 1;

        newHandle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                onComplete?.Invoke(op.Result);
            }
            else
            {
                Debug.LogError($"[Addressable] 로드 실패: {address}");
                // 실패 시 데이터 청소
                _assetCache.Remove(address);
                _refCounts.Remove(address);
                if (newHandle.IsValid()) Addressables.Release(newHandle);
            }
        };
    }

    public void ReleaseAsset(string address)
    {
        if (string.IsNullOrEmpty(address)) return;
        if (!_assetCache.TryGetValue(address, out AsyncOperationHandle handle)) return;

        _refCounts[address]--;

        if (_refCounts[address] <= 0)
        {
            // 실제 메모리 해제
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            _assetCache.Remove(address);
            _refCounts.Remove(address);
            Debug.Log($"[Addressable] 메모리 완전 해제: {address}");
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