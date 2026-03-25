using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;

public class GremlinManager : MonoBehaviour
{
    public static GremlinManager Instance;

    public Transform PlayerTransform { get {  return playerTransform; } }

    [SerializeField] private GremlinInventory _inventory;     // 그렘린 보관 전용 인벤토리
    [SerializeField] private Transform playerTransform;       // 플레이어의 위치
    [SerializeField] private Transform GremlinSpawnPoint;     // 그렘린을 소환할 위치

    [SerializeField] private GameObject spawnEffectPrefab;    // 소환 시 재생될 이펙트, 파티클
    [SerializeField] private GameObject despawnEffectPrefab;  // 해제/교체 시 재생될 이펙트, 파티클
    [SerializeField] private GameObject _gremlinPrefab;       // 소환할 그렘린 프리팹

    public GremlinInstance currentGremlin { get; private set; }  // 현재 소환한 그렘린
    public Gremlin gremlinInstance { get; private set; }         // 그 그렘린의 정보값
    private Rarity rarity;                                       // 현재 소환된 그렘린의 등급

    private void Awake()
    {
        //싱글톤 패턴 - 이미 있을 경우 파괴
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        //인스턴스화
        Instance = this;
    }

    /// <summary>
    /// 그렘린 생성
    /// </summary>
    /// <param name="address">생성할 그렘린의 주소</param>
    public async void CreateGremlin(string address)
    {
        //어드레서블 활용, 그렘린 스크립터블오브젝트 데이터를 받아옴
        var handle = Addressables.LoadAssetAsync<GremlinSOData>(address);
        await handle.Task;

        //그 결과물을 받아옴
        GremlinSOData so = handle.Result;

        //새로운 그렘린 생성
        var gremlin = new Gremlin();

        //새로운 GUID 부여, 등급은 Normal, 받아온 스크립터블 오브젝트를 사용해 초기화
        gremlin.Init(Guid.NewGuid().ToString(), so, Rarity.Normal);

        //인벤토리에 초기화 진행된 그렘린 추가
        _inventory.AddGremlin(gremlin);
    }

    /// <summary>
    /// 그렘린 교체
    /// </summary>
    /// <param name="toChange">바꾸려는 그렘린</param>
    /// <returns></returns>
    public IEnumerator ChangeGremlin(Gremlin toChange)
    {
        //소멸 파티클의 생성 위치는 펫의 위치입니다.
        Vector3 spawnPos = _gremlinPrefab != null? _gremlinPrefab.transform.position : GremlinSpawnPoint.position;

        //그렘린의 프리팹이 존재하지 않는다면, 스폰 포인트를 가져옵니다.
        if(_gremlinPrefab == null)
        {
            spawnPos = GremlinSpawnPoint.position;
        }

        //기존에 존재하던 그렘린 프리팹이 있으며, 그 그렘린 정보의 GUID가 교체하려는 그렘린의 GUID와 다른 경우
        else if(currentGremlin == null ||_gremlinPrefab != null && gremlinInstance != null && toChange.InstanceGUID != gremlinInstance.InstanceGUID)
        {
            //소멸 파티클을 생성합니다.
            GameObject removeParticle = Instantiate(despawnEffectPrefab, spawnPos, Quaternion.identity);

            //1초를 기다립니다. (파티클 유지 시간을 1초로 설정해 두었기 때문. 이후 변경하게 될 수도 있음.)
            yield return new WaitForSeconds(1.0f);

            //해당 프리팹을 파괴합니다.
            Destroy(_gremlinPrefab);

            //0.5초를 기다립니다. (파티클을 없애기 위한 시간. 기운을 전부 내뿜어 사라진 뒤, 그 기운이 약간 흩어진 후 사라지는 연출.)
            yield return new WaitForSeconds(0.5f);

            if(currentGremlin == null)
            {
                //소멸 파티클을 제거합니다.
                Destroy(removeParticle);
            }

            else
            {
                //소환된 것이 있다는 뜻이므로 그 그렘린부터 해제합니다.
                Addressables.ReleaseInstance(_gremlinPrefab);
                Destroy(removeParticle);
            }
        }

        //그렘린이 존재하고, 바꾸려는 그렘린의 GUID가 소환한 것과 일치하며 등급이 다른 경우
        if(gremlinInstance != null && toChange.InstanceGUID == gremlinInstance.InstanceGUID && toChange._rarity !=  Instance.rarity)
        {
            //등급 변화시의 모델 변화
            currentGremlin.ChangeModeling(toChange._rarity);
            yield break;
        }
        //그냥 완전히 동일한 경우 스킵
        else if(toChange == gremlinInstance)
        {
            yield break;
        }

        //스폰포인트를 다시 받아옵니다.
        spawnPos = GremlinSpawnPoint.position;

        //생성 파티클을 생성합니다.
        GameObject spawnParticle = Instantiate(spawnEffectPrefab, spawnPos, Quaternion.identity);

        //1초를 기다립니다. (소멸과 동일하게 시간을 1초로 설정해 두었기 때문. 이후 변경하게 될 수 있음.)
        yield return new WaitForSeconds(1.0f);

        var changeGremlin = toChange._gremlinData.Prefab.InstantiateAsync(
            GremlinSpawnPoint.position,
            Quaternion.identity);
        yield return changeGremlin;


        GameObject Change = changeGremlin.Result;

        //그렘린 프리팹은, 바꾸려는 대상의 SO 내부에 존재하는 프리팹을,
        //그렘린 전용 위치에 회전 없이, 그렘린 전용 위치 하위에 생성합니다.
        _gremlinPrefab = Change;

        //그렘린의 행동 전반은 해당 오브젝트에 들어있으므로, 생성된 그렘린 프리팹으로부터 받아옵니다.
        currentGremlin = _gremlinPrefab.GetComponent<GremlinInstance>();
        //그렘린의 정보는 현재 바꾸려는 그렘린의 정보로 덮어씌웁니다.
        gremlinInstance = toChange;

        rarity = toChange._rarity;
        //행동의 경우, Instance 내에서 해당 그렘린의 정보를 바탕으로
        //정해진 행동의 자식 클래스를 받아오는 메서드를 시행하여 받아옵니다.
        currentGremlin.Init(gremlinInstance);
        currentGremlin.ChangeModeling(toChange._rarity);

        //소환 파티클을 제거합니다.
        Destroy(spawnParticle);
        yield return null;
    }
    //Addressable

    //플레이어 트랜스폼 전달
}
