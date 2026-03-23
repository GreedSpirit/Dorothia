using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GremlinInventory : MonoBehaviour
{
    public List<Gremlin> _gremlinInventory { get; private set; }

    private void Awake()
    {
        _gremlinInventory = new List<Gremlin>();
    }

    public void AddGremlin(Gremlin item)
    {
        _gremlinInventory.Add(item);
    }
    public void SortInventory()
    {
        _gremlinInventory = _gremlinInventory.OrderBy(p => p._gremlinData.PetID).ThenByDescending(p => p._rarity).ToList();
        foreach(var pet in  _gremlinInventory)
        {
            Debug.Log(pet._gremlinData.PetID);
        }
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
}
