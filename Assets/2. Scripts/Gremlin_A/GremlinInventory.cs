using System.Collections.Generic;
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
}
