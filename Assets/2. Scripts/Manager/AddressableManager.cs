using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class AddressableManager : MonoBehaviour
{
    public static AddressableManager Instance;

    // 캐시 저장소: 주소를 키로 사용
    private Dictionary<string, AsyncOperationHandle> _assetCache = new();
    // 참조 카운트: 몇 명이나 이 에셋을 쓰고 있는지 기록
    private Dictionary<string, int> _refCounts = new();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void LoadAsset<T>(string address, Action<T> onComplete) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(address)) return;

        // 1. 이미 로드된 에셋이 있는지 확인
        if (_assetCache.TryGetValue(address, out AsyncOperationHandle existingHandle))
        {
            _refCounts[address]++;
            // 이미 완료된 핸들이라면 즉시 콜백 호출
            if (existingHandle.IsDone)
            {
                onComplete?.Invoke(existingHandle.Result as T);
            }
            else
            {
                // 로드 중이라면 완료 시점에 호출되도록 추가
                existingHandle.Completed += (op) => onComplete?.Invoke(op.Result as T);
            }
            return;
        }

        // 2. 처음 로드하는 경우
        var handle = Addressables.LoadAssetAsync<T>(address);
        _assetCache[address] = handle;
        _refCounts[address] = 1;

        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
                onComplete?.Invoke(op.Result);
            else
            {
                Debug.LogWarning($"로드 실패: {address}");
                _assetCache.Remove(address);
                _refCounts.Remove(address);
            }
        };
    }

    public void ReleaseAsset(string address)
    {
        if (!_assetCache.TryGetValue(address, out AsyncOperationHandle handle)) return;

        _refCounts[address]--;

        // 아무도 안 쓰면 실제로 메모리에서 해제
        if (_refCounts[address] <= 0)
        {
            if (handle.IsValid()) Addressables.Release(handle);
            _assetCache.Remove(address);
            _refCounts.Remove(address);
            Debug.Log($"메모리 완전 해제: {address}");
        }
    }
}