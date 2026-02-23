using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Equipment
{
    //CSV 구조도 순서대로 작성합니다!
    //equip
    public int equip_id;                                       // 장비의 ID값입니다.
    public string equip_name;                                  // 장비의 이름입니다.
    public Dictionary<Status, float> equip_status;    // 장비의 주요 스텟 전반을 담고 있을 Dictionary입니다.
    public Equip_Type equip_type;                              // 장비의 타입입니다. int값을 받으면 EquipType 열거형으로 자동 치환하여 가독성을 높입니다.
    public int equip_price;                                    // 장비의 판매가입니다.
    public string equip_model;                                 // 장비의 모델입니다. 모델? 장착 시 외형 변화를 위한 것인가...
    public string equip_icon;                                  // 장비의 아이콘, 즉 인벤토리에 출력 시 사용될 스프라이트를 가져오기 위한 경로입니다.
    public Sprite icon;                                        // 장비의 실질적 아이콘입니다. equip_icon을 통해 가져온 스프라이트를 넣을 공간입니다.

    //equip_rank
    //▼ 일반 : 1, 희귀 : 2, 레어 : 3, 전설 : 4, 신화 : 5
    public int equipment_Rarity;                      // 장비의 레어도입니다. 데이터에서 받아온 등급의 값의 가독성을 높이기 위해 열거형을 사용합니다.

    //equip_Upgrade
    public int equip_Upgrade_Value;

    public int equip_set_id;

    public int equip_level;

    public int equip_Upgrade;
    public int equip_Upgrade_Count;

    public int EquippedSlotIndex = -1; // 기본값은 -1, 장착 시 0, "반지 2 슬롯 한정" 1
    public bool isEquipped = false;    // 장착 시에만 true가 되는 장착 여부 확인용 bool형 매개변수
    public bool isFusing = false;


    /// <summary>
    /// 장착 슬롯에 해당 장비를 장착합니다.
    /// </summary>
    /// <param name="slotIndex">장착할 슬롯(반지 슬롯 대비)</param>
    public void SetEquipped(int slotIndex)
    {
        if(isEquipped == true)
        {
            Debug.Log("이미 장착한 장비입니다.");
            return;
        }
        //현재 장비를 장착하는 것이므로 장착 여부를 참으로 설정합니다.
        isEquipped = true;
        
        //현재 장착한 슬롯의 위치를 인자값으로 받아온 인덱스 값으로 설정합니다.
        EquippedSlotIndex = slotIndex;
    }

    /// <summary>
    /// 장비를 장착 해제합니다.
    /// </summary>
    public void UnEquip()
    {
        //현재 장비를 장착 해제한 것이므로 장착 여부를 거짓으로 설정합니다.
        isEquipped = false;

        //현재 장착한 슬롯 위치는 장착하지 않았을 때의 기본값으로 변경합니다.
        EquippedSlotIndex = -1;
    }

    /// <summary>
    /// 합성 칸에 등록했던 장비를 합성 칸에서 빼냅니다.
    /// </summary>
    public void CancelFuseMaterial()
    {
        isFusing = false;
    }

    /// <summary>
    /// 데이터의 장비 id와 추가적인 값들을 기반으로, 장비를 생성합니다.
    /// </summary>
    /// <param name="equipData">id값을 통해 데이터 테이블로부터 빼온 장비 id값</param>
    /// <param name="rarity">해당 장비의 등급</param>
    public Equipment(EquipData equipData, int rarity, int equipLevel)
    {
        equip_id = equipData.Equip_Id;                                            // 장비 id값
        equip_name = equipData.Equip_Name;                                        // 장비 이름
        equip_type = equipData.Equip_Type;                                        // 장착 부위
        equip_icon = equipData.Equip_Icon;                                        // 장비 아이콘
        equip_model = equipData.Equip_Model;                                      // ?
        equip_price = equipData.Equip_Price;                                      // 장비 판매 가격

        equipment_Rarity = rarity;                                                                                       // 장비 등급

        equip_status = new Dictionary<Status, float>();
        Debug.Log(equipData.Equip_Type);
        AddEquipStatusByType(equipData, (Rarity)DataManager.Instance.GetData<Equip_RankData>(equipment_Rarity).Equip_Rank);


        equip_set_id = GetSetEffect(equip_name);

        equip_level = equipLevel;

        equip_Upgrade = 0;
        equip_Upgrade_Count = 0;

    }

    /// <summary>
    /// 장비 데이터로부터, 해당 장비에서 유효한 스텟만 가져오는 메서드입니다.
    /// </summary>
    /// <param name="equipStatus">장비의 스텟</param>
    /// <param name="equipStatusValue">해당 장비 스텟의 값</param>
    public void AddEquipStatus(Status equipStatus,float equipStatusValue)
    {
        //이미 해당 스테이터스가 Dictionary에 존재한다면, 해당 값을 추가합니다.
        if(equip_status.ContainsKey(equipStatus))
        {
            equip_status[equipStatus] += equipStatusValue;
        }
        //Dictionary에 존재하지 않을 경우, 값이 0이 아닌 경우에만 포함시킵니다.
        else if(equipStatusValue != 0)
        {
            equip_status.Add(equipStatus, equipStatusValue);
        }
    }

    /// <summary>
    /// 규칙으로부터 확인한 스테이터스에 따라, 데이터로부터 해당 스테이터스의 정보를 받아옵니다.
    /// </summary>
    /// <param name="status">정보를 확인해야 하는 스테이터스</param>
    /// <param name="data">그 스테이터스를 확인하기 위한 테이블상의 장비데이터</param>
    public void AddStatusFromData(Status status, EquipData data)
    {
        switch (status)
        {
            case Status.HP:
                AddEquipStatus(status, data.Equip_Hp);
                break;

            case Status.ATK:
                AddEquipStatus(status, data.Equip_Atk);
                break;

            case Status.MagicATK:
                AddEquipStatus(status, data.Equip_Atk_M);
                break;

            case Status.AttackSpeed:
                AddEquipStatus(status, data.Equip_Dps);
                break;

            case Status.CriticalChance:
                AddEquipStatus(status, data.Equip_Crt_Prob);
                break;

            case Status.CriticalDamage:
                AddEquipStatus(status, data.Equip_Crt_Dmg);
                break;

            case Status.DEF:
                AddEquipStatus(status, data.Equip_Def);
                break;

            case Status.MagicDEF:
                AddEquipStatus(status, data.Equip_Def_M);
                break;

            case Status.HPRegen:
                AddEquipStatus(status, data.Equip_Hp_Regen);
                break;

            case Status.MoveSpeed:
                AddEquipStatus(status, data.Equip_Agi);
                break;
        }
    }

    /// <summary>
    /// 장착 부위별로 지정된 주요 스테이터스와 보조 스테이터스의 규칙대로 스테이터스를 형성합니다.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="rarity"></param>
    public void AddEquipStatusByType(EquipData data, Rarity rarity)
    {
        Debug.Log(data.Equip_Type);
        //static으로 선언한 규칙에서 만드려는 장비의 Dictionary를 받아옵니다.
        var rule = EquipStatusStaticRule._rules[data.Equip_Type];

        //해당 장비의 타입을 기반으로, 데이터를 확인하여 메인 스테이터스를 생성합니다.
        foreach(var main in rule.MainStatus)
        {
            AddStatusFromData(main, data);
        }

        //장비의 등급에 따라, 스테이터스를 얼마나 만들지 확인합니다.
        int subCount = EquipStatusStaticRule.SubStatusCount[rarity];

        //장비의 장착 부위와 등급에 따라, 보조 스테이터스 리스트를 생성합니다.
        var selectedSubs = new List<Status>();
        
        //규칙으로 정한 보조 스테이터스의 수량까지 도달하거나(배열 칸 이탈 방지) 추가해야 하는 보조 스테이터스 수량이 될 때까지 아래 코드를 실행합니다.
        for(int i = 0; i< rule.SubStatus.Count && selectedSubs.Count < subCount; i++)
        {
            //보조 스테이터스 규칙에 따라, 앞에 있는 것부터 순차적으로 추가합니다.
            selectedSubs.Add(rule.SubStatus[i]);
        }

        //그럼에도 여전히 스텟을 추가해야 하는 경우라면, 위 코드를 반복합니다.
        while(selectedSubs.Count < subCount)
        {
            //왼쪽 조건문이 초기화되었어도, 오른쪽 조건문은 그대로일 테니 필요하면 멈출 것입니다.
            for (int i = 0; i < rule.SubStatus.Count && selectedSubs.Count < subCount; i++)
            {
                //보조 스테이터스 규칙에 따라, 앞에 있는 것부터 순차적으로 추가합니다.
                selectedSubs.Add(rule.SubStatus[i]);
            }
        }

        //채워넣은 보조 스테이터스 리스트의 각 보조 스테이터스마다 데이터를 확인하여 스테이터스를 생성합니다.
        foreach(var sub in selectedSubs)
        {
            AddStatusFromData(sub, data);
        }
    }

    /// <summary>
    /// 장비 등급이 상승하게 되면, 그에 맞도록 보조 스테이터스를 추가합니다.
    /// </summary>
    /// <param name="data">장비의 데이터</param>
    /// <param name="oldRarity">승급하기 이전 장비의 등급</param>
    /// <param name="newRarity">승급하고 난 후의 장비의 등급</param>
    public void AddSubStatusOnUpgrade(EquipData data, Rarity oldRarity, Rarity newRarity)
    {
        //승급하기 전과 승급 이후의 보조 스테이터스 수를 구합니다.
        int oldCount = EquipStatusStaticRule.SubStatusCount[oldRarity];
        int newCount = EquipStatusStaticRule.SubStatusCount[newRarity];

        //승급 후의 보조 스테이터스 수에서 승급 전의 보조 스테이터스 수를 뺌으로서 추가할 보조 스테이터스 수를 구합니다.
        int addCount = newCount - oldCount;

        //그 수가 0이거나 그보다 작다면 추가할 사유가 없으므로 반환합니다.
        if (addCount <= 0)
        {
            Debug.Log("해당 등급에서 추가할 보조 스테이터스가 존재하지 않습니다.");
            return;
        }

        //index가 0에서 시작하므로, 기존 등급의 보조 스테이터스 수가 보조 스테이터스 리스트 길이보다 작은 경우 아래 코드를 실행합니다.
        if (EquipStatusStaticRule._rules[data.Equip_Type].SubStatus.Count > oldCount)
        {
            //현재 등급이 올라감으로서 추가되는 보조 스테이터스의 수는 1이므로 단일로 작성합니다.
            var sub = EquipStatusStaticRule._rules[data.Equip_Type].SubStatus[oldCount];

            AddStatusFromData(sub, data);
        }

        //기존 등급의 보조 스테이터스 수가 스테이터스 리스트 길이와 같거나 그보다 크면 아래 코드를 실행합니다.
        //(현재로서는 보조 스테이터스 리스트에 1개만 담겨져 있는 상황의 대비용 코드)
        else if (EquipStatusStaticRule._rules[data.Equip_Type].SubStatus.Count <= oldCount)
        {
            //예를 들어, 만약 3개째의 옵션이 추가되어야 하는데 규칙상의 보조 스테이터스 리스트 길이는 2라면
            //newCount = 3, oldCount = 2. 2%2 = 0이므로 앞에 있는 보조 스테이터스가 추가되는 형식입니다.
            var sub = EquipStatusStaticRule._rules[data.Equip_Type].SubStatus[oldCount
                % EquipStatusStaticRule._rules[data.Equip_Type].SubStatus.Count];

            AddStatusFromData(sub, data);
        }
    }

    public string GetEquipStatusString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        int i = 0;
        foreach(var stat in equip_status)
        {
            stringBuilder.Append($"{stat.Key} + {stat.Value} ");
            if (i == 1)
                stringBuilder.Append("\n");
            i++;
        }
        return stringBuilder.ToString();
    }

    public float GetStatus(Status equipStatus)
    {
        return equip_status.TryGetValue(equipStatus, out float value) ? value : 0f ;
    }

    private int GetSetEffect(string equipName)
    {
        Dictionary<int, List<Equip_SetData>> allSets = DataManager.Instance.GetListDict<Equip_SetData>();
        int set_id = 0;
        foreach(var Set in allSets.Values)
        {
            foreach(var item in Set)
            {
                if (equipName.Contains(item.Equip_Set_Need_Name))
                    set_id = item.Equip_Set_Id;
            }
        }
        return set_id;
    }
}
