using UnityEngine;
using System.Collections.Generic;

public class EquipmentSet
{
    public int equip_Set_Id;
    
    public List<EquipmentSetEffect> effects;
    public Dictionary<int, List<EquipmentSetEffect>> set_Dictionary;
}
