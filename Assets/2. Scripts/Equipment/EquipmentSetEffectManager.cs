using System.Collections.Generic;
using UnityEngine;

public class EquipmentSetEffectManager : MonoBehaviour
{
    public static EquipmentSetEffectManager Instance;      // 전역적으로 접근 가능한 Instance

    public Dictionary<int, List<EquipmentSet>> effects;    // 모든 세트효과를 담아둘 Dictionary

    public bool isActived = false;

    private void Awake()
    {
        //이미 인스턴스가 존재하며 그것이 자신이 아닌 경우 삭제.
        if (Instance != null && Instance != this)
            Destroy(gameObject);

        //인스턴스에 자신을 넣고, Dictionary를 재정의. 게임 전반에서 유지되어야 하므로 파괴 방지.
        Instance = this;
        effects = new Dictionary<int, List<EquipmentSet>>();
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 세트 데이터를 불러옵니다.
    /// </summary>
    /// <param name="setId">세트의 ID값</param>
    /// <param name="equip_SetDataList">그 ID값에 따른 세트 데이터들의 리스트</param>
    public void GetSetData(int setId, List<Equip_SetData> equip_SetDataList)
    {
        //리스트 내에 있는 모든 효과들을 기준으로
        foreach (var effect in equip_SetDataList)
        {
            //그 효과의 Key로서 인자값 int값이 없을 경우
            if (!effects.ContainsKey(setId))
            {
                //해당 int값을 key값으로 갖는 새로운 리스트를 생성하고
                effects[effect.Equip_Set_Id] = new List<EquipmentSet>();
            }
            //세트데이터를 세트효과에 적용하고
            EquipmentSet dictValue = new EquipmentSet(effect);
            //그 세트효과를 Dictionary에 추가.
            effects[effect.Equip_Set_Id].Add(dictValue);
        }
    }

    /// <summary>
    /// 모든 세트효과를 받아옵니다.
    /// </summary>
    public void AddAllSets()
    {
        var dataDic = DataManager.Instance.GetListDict<Equip_SetData>();
        foreach (var key in dataDic.Keys)
        {
            GetSetData(key, DataManager.Instance.GetList<Equip_SetData>(key));
        }
    }

    /// <summary>
    /// 장비가 어떤 세트효과를 가지게 되는지 찾아 해당 id값을 반환합니다.
    /// </summary>
    /// <param name="equip_name">세트효과를 찾아줄 장비의 이름입니다.</param>
    /// <returns>적합한 세트 효과 id값</returns>
    public int ApplySetEffects(string equip_name)
    {
        int set_id = 0;
        foreach(var effect in effects.Values)
        {
            foreach(var e in effect)
            { 
                if(equip_name.Contains(e.equip_Set_Need_Name))
                {
                    set_id = e.equip_Set_Id;
                }
            }
        }
        return set_id;
    }
}
