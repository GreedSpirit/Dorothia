using UnityEngine;

public class EquipmentInstance : MonoBehaviour
{
    public string InstanceGUID { get; private set; }

    public Equipment equipment;

    public int upgrade;

    public bool isEquipped;
    public bool isLocked;
    public bool isFusing;

    public int equippedSlotIndex;

    public float upgradeWeight;
    public float fustWeight;
}
