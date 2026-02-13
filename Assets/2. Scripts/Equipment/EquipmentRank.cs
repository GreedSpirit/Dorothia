using UnityEngine;

public class EquipmentRank
{
    public int equip_rank_id;
    public Equip_Rank equip_rank;
    public float equip_value;
    public float equip_success_prob;
    public float equip_rank_failure;

    public EquipmentRank(Equip_RankData equip_Rankdata)
    {
        equip_rank_id = equip_Rankdata.Equip_Rank_Id;
        equip_rank = equip_Rankdata.Equip_Rank;
        equip_value = equip_Rankdata.Equip_Value;
        equip_success_prob = equip_Rankdata.Equip_Success_Prob;
        equip_rank_failure = equip_Rankdata.Equip_Rank_Failure;
    }
}
