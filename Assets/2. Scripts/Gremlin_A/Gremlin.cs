using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Gremlin
{
    public string InstanceGUID { get; private set; }
    public GremlinSOData _gremlinData;
    public int _currentLevel = 0;          // 현재레벨
    public Rarity _rarity;
    public bool _isEquipped = false;
    public int _enchantCount = 0;

    //그렘린을 갈아끼거나 할 때 부를 초기화 함수
    public void Init(string GUID, GremlinSOData data, Rarity rarity)
    {
        InstanceGUID = GUID;
        _currentLevel = _currentLevel == 0? 0: _currentLevel;
        _gremlinData = data;
        _rarity = rarity;
        _enchantCount = _enchantCount == 0? 0: _enchantCount;
    }

    public void Init(string GUID, GremlinSOData data, Rarity rarity, int level, int enchantCount)
    {
        InstanceGUID = GUID;
        _currentLevel = level;
        _gremlinData = data;
        _rarity = rarity;
        _enchantCount = enchantCount;
    }

    public GremlinSaveData ToSaveData()
    {
        GremlinSaveData data = new GremlinSaveData();
        data.guid = InstanceGUID;
        int id = _gremlinData.PetID;
        Debug.Log(id);
        data.petID = _gremlinData.PetID;
        data.rarity = (int)_rarity;
        data.level = _currentLevel;
        data.enchantCount = _enchantCount;
        data.isEquipped = _isEquipped;
        return data;
    }

    public async Task<Gremlin> FromSaveData(GremlinSaveData data)
    {
        Gremlin gremlin = new Gremlin();
        GremlinSOData SO = await GetSOData(data.petID);
        gremlin.Init(data.guid, SO, (Rarity)data.rarity, data.level, data.enchantCount);
        gremlin._isEquipped = data.isEquipped;
        return gremlin;
    }

    public async Task<GremlinSOData> GetSOData(int id)
    {
        string key = id switch
        {
            140001 => "SO_Flint",
            140002 => "SO_Pinion",
            143001 => "SO_Core",
            143002 => "SO_Zinc",
            _ => null
        };

        var handle = Addressables.LoadAssetAsync<GremlinSOData>(key);
        await handle.Task;

        return handle.Result;
    }
}
