using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    // 이펙트 이름별로 풀을 관리
    private Dictionary<string, ObjectPool<GameObject>> _effectPools = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void PlayEffect(EffectData data, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!_effectPools.ContainsKey(data.effectName)) CreatePool(data);

        GameObject obj = _effectPools[data.effectName].Get();

        if (parent != null)
        {
            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }
        else
        {
            obj.transform.SetPositionAndRotation(position, rotation);
        }

        StartCoroutine(ReleaseRoutine(data, obj));
    }

    private void CreatePool(EffectData data)
    {
        _effectPools[data.effectName] = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(data.prefab),
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            defaultCapacity: 10,
            maxSize: 20
        );
    }

    private IEnumerator ReleaseRoutine(EffectData data, GameObject obj)
    {
        yield return new WaitForSeconds(data.duration);
        _effectPools[data.effectName].Release(obj);
    }
}