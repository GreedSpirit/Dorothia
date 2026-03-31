using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData
{
    public int level;
    public string currentExpStr;        // BigInteger → string
    public int promotion;
    public List<StatUpgradeEntry> statUpgrades = new();
    public float overdriveGauge;
    public bool isAutoMode;
}

[Serializable]
public class StatUpgradeEntry
{
    public int statType;    // (int)Status
    public int upgradeLevel;
}