using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class GremlinUpgradePanel : BaseUI
{
    [SerializeField] Gremlin targetGremlin;
    [SerializeField] GremlinInventory inventory;
    [SerializeField] GremlinUIPanel panel;
    //현재 그렘린이 들어갈 이미지
    [SerializeField] Image _currentGremlinImage;
    //그 그렘린과 일치하는 각각 등급의 이미지
    [SerializeField] Image[] _gremlinRarityImage;
    //조각 및 각 등급별 소지 개수 표기용 텍스트 및 버튼
    [SerializeField] Button _normalButton;
    [SerializeField] TextMeshProUGUI _gremlinNormalText;
    [SerializeField] Button _uncommonButton;
    [SerializeField] TextMeshProUGUI _gremlinUncommonText;
    [SerializeField] Button _rareButton;
    [SerializeField] TextMeshProUGUI _gremlinRareText;
    [SerializeField] Button _legendaryButton;
    [SerializeField] TextMeshProUGUI _gremlinLegendaryText;
    [SerializeField] TextMeshProUGUI _gremlinMythticText;
    [SerializeField] TextMeshProUGUI _StatChangeText;
    //선택용 사각형 이미지
    [SerializeField] GameObject _raritySquareImage;
    //합성 버튼
    [SerializeField] Button _fuseButton;

    private Dictionary<Rarity, int> gremlinInv = new Dictionary<Rarity, int>();

    private Rarity currentGremlinRarity;

    private bool isMerging = false;
    int combineCallCount = 0;

    //열었을 때
    private void Awake()
    {
        _normalButton.onClick.AddListener(() => { 
            OnSelectRarity(-304, Rarity.Normal);
        });
        _uncommonButton.onClick.AddListener(() => {
            OnSelectRarity(-152, Rarity.Uncommon);
        });
        _rareButton.onClick.AddListener(() => { 
            OnSelectRarity(0, Rarity.Rare);
        });
        _legendaryButton.onClick.AddListener(() => {
            OnSelectRarity(152, Rarity.Legendary);
        });
        _fuseButton.onClick.RemoveAllListeners();
        _fuseButton.onClick.AddListener(() => {
            FuseGremlin();
            });
        Close();
    }

    //메서드 - 합성 대상 체크
    public IEnumerator GetFuseTarget(Gremlin gremlin)
    {
        //대상 그렘린으로서 인자값으로 받은 그렘린을 받아옵니다.
        targetGremlin = gremlin;
        currentGremlinRarity = gremlin._rarity;
        //그렘린 데이터에서 어드레서블의 스프라이트를 가져옵니다.
        var gSprite = Addressables.LoadAssetAsync<Sprite>($"{gremlin._gremlinData.PrefabName}_Icon");
        yield return gSprite.Task;

        //현재 그렘린의 이미지 스프라이트를 변경합니다.
        _currentGremlinImage.sprite = gSprite.Result;

        //각 레어도별 이미지도 해당 스프라이트를 반영합니다.
        foreach(Image image in _gremlinRarityImage)
        {
            image.sprite = gSprite.Result;
        }


        Refresh();
    }
    //메서드 - 합성
    public void FuseGremlin()
    {
        //대상이 될 그렘린이 없으면 반환합니다.
        if (targetGremlin == null) return;

        //인벤토리 내에 해당 그렘린과 동일 종류이며 동일 등급인 그렘린이 존재하지 않거나 그 수가 본인 포함 3마리 이하일 경우 반환합니다.
        if (!gremlinInv.ContainsKey(currentGremlinRarity) || gremlinInv[currentGremlinRarity] < 3) return;

        //대상 그렘린과 동일한 그렘린ID를 가진, 동일 등급의 그렘린 3마리를 받아옵니다.
        var selectedPets = inventory.GetSpecificItem(targetGremlin._gremlinData.PetID,currentGremlinRarity);
        //만에 하나 3마리가 나오지 않으면 반환합니다.
        if (selectedPets == null || selectedPets.Count < 3) return;

        //합성 중인 경우 반환합니다. (프로세스 내에서 중복 실행 방지)
        if (isMerging == true) return;

        //합성 중으로 표기합니다.
        isMerging = true;

        try
        {
            //3개의 그렘린을 담은 리스트에서 장착된 그렘린이 있으면 그 대상을 메인으로, 없을 경우 가장 첫 번째를 메인으로 사용
            //FirstOrDefault : 조건을 만족하는 첫 번째 대상을 사용
            //?? 연산자 : 앞 부분이 null인 경우 뒤 부분을 사용
            var mainPet = selectedPets.FirstOrDefault(p => p._isEquipped) ?? selectedPets[0];

            var removePet = selectedPets.Where(p => p != mainPet).ToList();

            foreach (var pet in removePet)
            {
                inventory._gremlinInventory.Remove(pet);
            }

            mainPet._rarity++;
            mainPet._currentLevel = 0;
        }

        finally
        {
            Refresh();
            isMerging = false;
        }
    }

    public void OnSelectRarity(float x, Rarity rarity)
    {
        _raritySquareImage.gameObject.transform.position = new Vector3(540+x, 960, 0);
        currentGremlinRarity = rarity;
        Refresh();
        if (!gremlinInv.ContainsKey(rarity)) return;
        CheckRarityText(rarity).text = gremlinInv[rarity] >= 3? $"{gremlinInv[rarity]}/3": $"<color=red>{gremlinInv[rarity]}</color>/3";
    }

    private TextMeshProUGUI CheckRarityText(Rarity rarity)
    {
        switch(rarity)
        {
            case Rarity.Normal:
                return _gremlinNormalText;
            case Rarity.Uncommon:
                return _gremlinUncommonText;
            case Rarity.Rare:
                return _gremlinRareText;
            case Rarity.Legendary:
                return _gremlinLegendaryText;
            default:
                return _gremlinNormalText; 
        }
    }

    //메서드 - 리프레시
    public void Refresh()
    {
        RefreshInventory();
        //하드코딩 상태 : ID값 관련으로 기획팀과 이야기 필요
        _gremlinNormalText.text = gremlinInv.ContainsKey(Rarity.Normal)? $"{gremlinInv[Rarity.Normal]}": "0";
        _gremlinUncommonText.text = gremlinInv.ContainsKey(Rarity.Uncommon)? $"{gremlinInv[Rarity.Uncommon]}": "0";
        _gremlinRareText.text = gremlinInv.ContainsKey(Rarity.Rare)? $"{gremlinInv[Rarity.Rare]}": "0";
        _gremlinLegendaryText.text = gremlinInv.ContainsKey(Rarity.Legendary)? $"{gremlinInv[Rarity.Legendary]}": "0";
        _gremlinMythticText.text = gremlinInv.ContainsKey(Rarity.Mythtic)? $"{gremlinInv[Rarity.Mythtic]}": "0";

        if (targetGremlin._rarity == Rarity.Mythtic) return;
        Gremlin_TierData tierData = DataManager.Instance.GetData<Gremlin_TierData>((int)currentGremlinRarity);
        Gremlin_TierData nextTierData = DataManager.Instance.GetData<Gremlin_TierData>((int)currentGremlinRarity + 1);
        if(targetGremlin._gremlinData.Type == Gremlin_Type.지원형)
        {
            Gremlin_BufferData bufferData = DataManager.Instance.GetData<Gremlin_BufferData>((int)currentGremlinRarity);
            Gremlin_BufferData nextBufferData = DataManager.Instance.GetData<Gremlin_BufferData>((int)currentGremlinRarity + 1);
            _StatChangeText.text = 
                $"스텟 배율 : {tierData.Gremlin_Tier_Multiplier * 100}% -> {nextTierData.Gremlin_Tier_Multiplier * 100}%\n쿨타임 : {bufferData.Gremlin_Tier_Cooltime}초 -> {nextBufferData.Gremlin_Tier_Cooltime}초";
        }
        else if(targetGremlin._gremlinData.Type == Gremlin_Type.공격형)
        {
            Gremlin_AtkerData atkerData = DataManager.Instance.GetData<Gremlin_AtkerData>((int)currentGremlinRarity);
            Gremlin_AtkerData nextAtkerData = DataManager.Instance.GetData<Gremlin_AtkerData>((int)currentGremlinRarity + 1);
            _StatChangeText.text =
                $"스텟 배율 : {tierData.Gremlin_Tier_Multiplier * 100}% -> {nextTierData.Gremlin_Tier_Multiplier * 100}%\n공격속도 : {atkerData.Gremlin_Tier_Dps * 100}% -> {nextAtkerData.Gremlin_Tier_Dps * 100}%";
        }
    }

    public void RefreshInventory()
    {
        gremlinInv.Clear();
        foreach(var gremlin in inventory._gremlinInventory)
        {
            if(gremlin._gremlinData.PetID != targetGremlin._gremlinData.PetID)
            {
                continue;
            }

            if(!gremlinInv.ContainsKey(gremlin._rarity))
            {
                gremlinInv[gremlin._rarity] = 0;
            }

            gremlinInv[gremlin._rarity]++;
        }
    }
    //메서드 - 초기화
    protected override void OnClose()
    {
        panel.onChangedInventory?.Invoke();
    }

    protected override void OnOpen()
    {
        
    }
}
