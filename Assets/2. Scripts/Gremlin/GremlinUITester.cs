using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GremlinUITester : MonoBehaviour
{
    public Sprite dummyIconNormal;
    public Sprite dummyIconRare;
    public Sprite dummyIconLegendary;

    [SerializeField] private GremlinUIPanel _gremlinUIPanel;
    [SerializeField] GremlinInventory _gremlinList;

    private List<Gremlin> _dummyGremlins;

    private void Start()
    {
        //더미데이터 생성
        GenerateDummyData();
        
        //그렘린 UI 패널이 존재할 경우
        if (_gremlinUIPanel != null)
        {

        }
        else
        {
            Debug.LogError("[GremlinUITester] _gremlinUIPanel 필요");
        }
    }

    //더미데이터 생성
    private async void GenerateDummyData()
    {
        //그렘린아이템데이터의 집합체
        _dummyGremlins = _gremlinList._gremlinInventory;

        // 장착 중인 그렘린 (빨간 테두리 테스트용)
        Gremlin TestGremlin1 = new Gremlin();

        var so = Addressables.LoadAssetAsync<GremlinSOData>("SO_Flint");
        await so.Task;
        GremlinSOData data = so.Result;
        TestGremlin1.Init(Guid.NewGuid().ToString(), data, Rarity.Normal);
        TestGremlin1._isEquipped = true;
        _gremlinList.AddGremlin(TestGremlin1);

        Gremlin TestGremlin2 = new Gremlin();

        so = Addressables.LoadAssetAsync<GremlinSOData>("SO_Core");
        await so.Task;
        data = so.Result;
        TestGremlin2.Init(Guid.NewGuid().ToString(), data, Rarity.Normal);
        _gremlinList.AddGremlin(TestGremlin2);

        Gremlin TestGremlin3 = new Gremlin();

        so = Addressables.LoadAssetAsync<GremlinSOData>("SO_Jink");
        await so.Task;
        data = so.Result;
        TestGremlin3.Init(Guid.NewGuid().ToString(), data, Rarity.Uncommon);
        _gremlinList.AddGremlin(TestGremlin3);
        //
        //_gremlinList.AddGremlin(new GremlinItemData
        //{
        //    id = 2,
        //    gremlinName = "황금 렌치 포드",
        //    currentLevel = 25,
        //    currentStat = 350.0f,
        //    tier = Rarity.Rare,
        //    iconSprite = dummyIconRare,
        //    isEquipped = false
        //});
        //
        //for (int i = 3; i <= 14; i++)
        //{
        //    _gremlinList.AddGremlin(new GremlinItemData
        //    {
        //        id = i,
        //        gremlinName = $"실험용 그렘린 MK-{i}",
        //        currentLevel = Random.Range(1, 50),
        //        currentStat = Random.Range(10f, 1000f),
        //        tier = (Rarity)Random.Range(1, 4), // Enum 인덱스 랜덤
        //        iconSprite = dummyIconLegendary, // 테스트용 통일
        //        isEquipped = false
        //    });
        //}
        //
        Debug.Log($"[GremlinUITester] 총 {_gremlinList._gremlinInventory.Count}개의 더미 데이터가 생성되었습니다.");
    }
}