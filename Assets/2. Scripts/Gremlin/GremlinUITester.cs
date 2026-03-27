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
        ExchangeManager.Instance.onShardValueChanged += MergeShard;
    }

    public void MergeShard()
    {
        int id = ExchangeManager.Instance.GetCurrentShardID();
        GenerateGremlin(id);
    }

    public async void GenerateGremlin(int id)
    {
        {
            string name = DataManager.Instance.GetData<G_StoneData>(id).G_Stone_Name;
            Gremlin target = new Gremlin();
            switch (name)
            {
                case "플린트":
                    var so = Addressables.LoadAssetAsync<GremlinSOData>("SO_Flint");
                    await so.Task;
                    GremlinSOData data = so.Result;
                    target.Init(Guid.NewGuid().ToString(), data, Rarity.Normal);
                    break;
                case "피니언":
                    so = Addressables.LoadAssetAsync<GremlinSOData>("SO_Pinion");
                    await so.Task;
                    data = so.Result;
                    target.Init(Guid.NewGuid().ToString(), data, Rarity.Normal);
                    break;
                case "코어":
                    so = Addressables.LoadAssetAsync<GremlinSOData>("SO_Core");
                    await so.Task;
                    data = so.Result;
                    target.Init(Guid.NewGuid().ToString(), data, Rarity.Normal);
                    break;
                case "징크":
                    so = Addressables.LoadAssetAsync<GremlinSOData>("SO_Zinc");
                    await so.Task;
                    data = so.Result;
                    target.Init(Guid.NewGuid().ToString(), data, Rarity.Normal);
                    break;
            }
            _gremlinList.AddGremlin(target);
            
        }
    }

    //더미데이터 생성
    private async void GenerateDummyData()
    {
        //그렘린아이템데이터의 집합체
        _dummyGremlins = _gremlinList._gremlinInventory;

        var so = Addressables.LoadAssetAsync<GremlinSOData>("SO_Zinc");
        await so.Task;
        GremlinSOData data = so.Result;

        Gremlin TestGremlin1 = new Gremlin();
        TestGremlin1.Init(Guid.NewGuid().ToString(), data, Rarity.Normal);
        _gremlinList.AddGremlin(TestGremlin1);


        so = Addressables.LoadAssetAsync<GremlinSOData>("SO_Core");
        await so.Task;
        data = so.Result;

        Gremlin TestGremlin2 = new Gremlin();
        TestGremlin2.Init(Guid.NewGuid().ToString(), data, Rarity.Normal);
        _gremlinList.AddGremlin(TestGremlin2);

        so = Addressables.LoadAssetAsync<GremlinSOData>("SO_Flint");
        await so.Task;
        data = so.Result;

        Gremlin TestGremlin3 = new Gremlin();
        TestGremlin3.Init(Guid.NewGuid().ToString(), data, Rarity.Normal);
        _gremlinList.AddGremlin(TestGremlin3);

        so = Addressables.LoadAssetAsync<GremlinSOData>("SO_Pinion");
        await so.Task;
        data = so.Result;

        Gremlin TestGremlin4 = new Gremlin();
        TestGremlin4.Init(Guid.NewGuid().ToString(), data, Rarity.Normal);
        _gremlinList.AddGremlin(TestGremlin4);
    }
}