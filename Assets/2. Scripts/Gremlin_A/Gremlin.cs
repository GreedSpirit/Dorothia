using UnityEngine;

public class Gremlin
{
    public string InstanceGUID { get; private set; }
    public GremlinSOData _gremlinData;
    public int _currentLevel = 0;          // 현재레벨
    public Rarity _rarity;
    public bool _isEquipped = false;

    //그렘린을 갈아끼거나 할 때 부를 초기화 함수
    public void Init(string GUID, GremlinSOData data, Rarity rarity)
    {
        InstanceGUID = GUID;
        _currentLevel = _currentLevel == 0? 0: _currentLevel;
        _gremlinData = data;
        _rarity = rarity;
    }
}
