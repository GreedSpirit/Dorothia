using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GremlinInventory : MonoBehaviour, ISaveable<List<GremlinSaveData>>
{
    public static GremlinInventory Instance;
    public List<Gremlin> _gremlinInventory { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _gremlinInventory = new List<Gremlin>();
    }

    /// <summary>
    /// 그렘린 추가
    /// </summary>
    /// <param name="item"></param>
    public void AddGremlin(Gremlin item)
    {
        //받아온 그렘린을 리스트에 추가
        _gremlinInventory.Add(item);
    }

    /// <summary>
    /// 인벤토리 정렬
    /// </summary>
    public void SortInventory()
    {
        //PetID 순서에 맞춰 정렬한 후, 등급에 따라 한번 더 정렬
        _gremlinInventory = _gremlinInventory.OrderBy(p => p._gremlinData.PetID).ThenByDescending(p => p._rarity).ToList();
    }

    /// <summary>
    /// 조건에 맞는 3개의 그렘린만을 인벤토리 칸 순서 기준으로 가져옵니다. 조건 : 등급
    /// </summary>
    /// <param name="rarity">가져올 그렘린들의 조건으로 사용할 등급</param>
    /// <returns>해당 등급의 그렘린 3마리</returns>
    public List<Gremlin> GetSpecificItem(int id, Rarity rarity)
    {
        List<Gremlin> item = _gremlinInventory.Where(p => p._gremlinData.PetID == id && p._rarity == rarity).Take(3).ToList();
        return item;
    }

    public List<GremlinSaveData> GetSaveData()
    {
        return _gremlinInventory.Select(p => p.ToSaveData()).ToList();
    }

    public async void LoadFromSaveData(List<GremlinSaveData> data)
    {
        _gremlinInventory.Clear();
        foreach (var item in data)
        {
           Gremlin gremlin = new Gremlin();
            var g = await gremlin.FromSaveData(item);
            gremlin = g;
            if(gremlin._isEquipped == true)
            {
                GremlinManager.Instance.ChangeGremlin(gremlin);
            }
            AddGremlin(gremlin);
        }
    }
}
