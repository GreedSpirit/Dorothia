using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "GremlinSOData", menuName = "Scriptable Objects/GremlinSOData")]
public class GremlinSOData : ScriptableObject
{
    public int PetID;
    public AssetReferenceGameObject Prefab;
    public string PrefabName;
    public Gremlin_Type Type;
    public Rarity rarity;
    public Sprite sprite;
}
