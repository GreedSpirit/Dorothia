using System.Collections.Generic;

[System.Serializable]
public class InventorySaveData
{
    public List<EquipmentSaveData> EquipmentInventory;

    public void Init()
    {
        EquipmentInventory ??= new List<EquipmentSaveData>();
    }
}