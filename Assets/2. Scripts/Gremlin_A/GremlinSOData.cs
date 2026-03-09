using UnityEngine;

[CreateAssetMenu(fileName = "GremlinSOData", menuName = "Scriptable Objects/GremlinSOData")]
public class GremlinSOData : ScriptableObject
{
    public int PetID;
    public GameObject Prefab;
    public Gremlin_Type Type;
    public Rarity rarity;
}
