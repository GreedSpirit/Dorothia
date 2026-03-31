using System;
using System.Collections.Generic;
[Serializable]
public class SaveData
{
    public InventorySaveData equipInv;
    public List<GremlinSaveData> GremlinInv;
    public SkillSaveData skillData;
    public StageSaveData stageData;
    public PlayerSaveData playerData;
}
