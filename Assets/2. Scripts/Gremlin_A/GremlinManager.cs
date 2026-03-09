using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GremlinManager : MonoBehaviour
{
    public static GremlinManager Instance;

    public Transform PlayerTransform { get {  return transform; } }

    [SerializeField] private GremlinInventory _inventory;
    [SerializeField] private Transform playerTransform; // 플레이어의 위치

    [SerializeField] private GameObject spawnEffectPrefab; // 소환 시 재생될 이펙트, 파티클
    [SerializeField] private GameObject despawnEffectPrefab; // 해제/교체 시 재생될 이펙트, 파티클

    private GremlinInstance currentGremlin;
    public Gremlin gremlinInstance { get; private set; }

    private void Awake()
    {

    }

    //그렘린 생성
    public async Task CreateGremlin()
    {
        var handle = Addressables.LoadAssetAsync<GremlinSOData>("");
        await handle.Task;

        GremlinSOData so = handle.Result;

        var gremlin = new Gremlin();
        gremlin.Init(Guid.NewGuid().ToString(), so, Rarity.Normal);

        _inventory.AddGremlin(gremlin);
    }

    //그렘린 교체

    //Addressable

    //플레이어 트랜스폼 전달
}
