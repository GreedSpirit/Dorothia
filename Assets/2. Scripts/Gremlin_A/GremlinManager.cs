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

    [SerializeField] private GremlinInventory _inventory;
    [SerializeField] private Transform playerTransform; // 플레이어의 위치
    [SerializeField] private Transform GremlinSpawnPoint;

    [SerializeField] private GameObject spawnEffectPrefab; // 소환 시 재생될 이펙트, 파티클
    [SerializeField] private GameObject despawnEffectPrefab; // 해제/교체 시 재생될 이펙트, 파티클
    [SerializeField] private GameObject _gremlinPrefab;

    private GremlinInstance currentGremlin;
    public Gremlin gremlinInstance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    //그렘린 생성
    public async void CreateGremlin(string address)
    {
        var handle = Addressables.LoadAssetAsync<GremlinSOData>(address);
        await handle.Task;

        GremlinSOData so = handle.Result;

        var gremlin = new Gremlin();
        gremlin.Init(Guid.NewGuid().ToString(), so, Rarity.Normal);

        _inventory.AddGremlin(gremlin);
    }

    //그렘린 교체
    public IEnumerator ChangeGremlin(Gremlin toChange)
    {
        //소멸 파티클의 생성 위치는 펫의 위치입니다.
        Vector3 spawnPos = _gremlinPrefab != null? _gremlinPrefab.transform.position : GremlinSpawnPoint.position;

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
                Addressables.ReleaseInstance(_gremlinPrefab);
            }
        }
        if(toChange == gremlinInstance)
        {
            Debug.Log("기존과 같아 생성불가");
            yield break;
        }
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

        //행동의 경우, Instance 내에서 해당 그렘린의 정보를 바탕으로
        //정해진 행동의 자식 클래스를 받아오는 메서드를 시행하여 받아옵니다.
        currentGremlin.Init(gremlinInstance);

        Destroy(spawnParticle);
        yield return null;
    }
    //Addressable

    //플레이어 트랜스폼 전달
}
