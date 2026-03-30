using System;
using System.Collections.Generic;
[Serializable]
public class SaveData
{
    public InventorySaveData equipInv;
    public List<GremlinSaveData> GremlinInv;
    public SaveSkillData skillData;
}
