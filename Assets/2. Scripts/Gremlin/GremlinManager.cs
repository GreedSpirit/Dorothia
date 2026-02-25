using UnityEngine;

public class GremlinManager : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    [SerializeField] private GameObject spawnEffectPrefab; // 소환 시 재생될 이펙트, 파티클
    [SerializeField] private GameObject despawnEffectPrefab; // 해제/교체 시 재생될 이펙트, 파티클

    private GremlinBase currentGremlinInstance;

    /// <summary>
    /// UI(그렘린 장비창)에서 그렘린을 장착하거나 교체할 때 호출할 함수
    /// 생성할 그렘린 프리팹,
    /// 그렘린 고유 ID,
    /// 그렘린 이름,
    /// 등급,
    /// 레벨,
    /// CSV에서 로드한 스탯(공속, 공격력 또는 버프수치)
    /// </summary>
    
    public void EquipGremlin(GameObject gremlinPrefab, string id, string gremlinName, Rarity tier, int level, float csvBaseValue)
    {
        DespawnCurrentGremlin();

        SpawnGremlin(gremlinPrefab, id, name, tier, level, csvBaseValue);
    }

    public void DespawnCurrentGremlin()
    {
        if(currentGremlinInstance != null)
        {
            if(despawnEffectPrefab != null)
            {
                Instantiate(despawnEffectPrefab, currentGremlinInstance.transform.position, Quaternion.identity);
            }

            Destroy(currentGremlinInstance.gameObject);
            currentGremlinInstance = null;

            Debug.Log("[GremlinManager] 기존 그렘린 소멸 함수 작동");
        }
    }

    private void SpawnGremlin(GameObject prefab, string id, string name, Rarity tier, int level, float csvBaseValue)
    {
        Vector3 spawnPosition = playerTransform.position;

        GameObject newGremlinObj = Instantiate(prefab, spawnPosition, Quaternion.identity);
        currentGremlinInstance = newGremlinObj.GetComponent<GremlinBase>();

        if(currentGremlinInstance != null)
        {
            currentGremlinInstance.Init(id, name, tier, level, csvBaseValue, playerTransform);

            if (spawnEffectPrefab != null)
            {
                Instantiate(spawnEffectPrefab, newGremlinObj.transform.position, Quaternion.identity);
            }
            Debug.Log($"[GremlinManager] {name} (Tier : {tier}, Lv: {level}) 소환 성공");
        }
        else
        {
            Debug.LogError("그렘린 프리팹에 GremlinBase 컴포넌트가 있는지 확인해 볼 것!");
            Destroy(newGremlinObj);
        }
    }


}
