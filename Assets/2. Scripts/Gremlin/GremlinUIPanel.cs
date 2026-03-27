using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System;

public class GremlinUIPanel : BaseUI
{
    [Header("정보 출력용 상단 패널")]
    [SerializeField] private Image _imgPortrait;       // 이미지
    [SerializeField] private TextMeshProUGUI _txtName; // 그렘린 이름
    [SerializeField] private TextMeshProUGUI _txtLevel;// 그렘린 레벨
    [SerializeField] private TextMeshProUGUI _txtStat; // 그렘린 스텟
    [SerializeField] private TextMeshProUGUI _txtRarityStat; // 그렘린 스텟
    [SerializeField] private TextMeshProUGUI _txtUpgradeStat; // 그렘린 스텟

    [Header("스크롤뷰(중간 패널, 아이템 표기용)")]
    [SerializeField] private Transform _scrollContent; // 스크롤 컨턴츠
    [SerializeField] private GameObject _gremlinItemPrefab; // 그렘린 프리팹

    [Header("버튼")]
    [SerializeField] private Button _btnEquip;          // 장착버튼
    [SerializeField] private Button _btnGoEnhance;      // 강화버튼
    [SerializeField] private Button _btnGoMerge;        // 합성버튼

    [SerializeField] private GremlinInventory _gremlinList;
    [SerializeField] private NoticeUIPanel _noticeUIPanel;
    [SerializeField] private GremlinUpgradePanel _mergePanel;
    [SerializeField] private GremlinEnchantPanel _enchantPanel;

    private Gremlin _selectedGremlinData;               // 선택한 그렘린의 데이터
    private GremlinUIItem _selectedUIItem;              // 선택된 오브젝트 보여줄 UI
    private GremlinUIItem _equippedUIItem;              // 장착 오브젝트 보여줄 UI
    private List<GremlinUIItem> _createdItems = new List<GremlinUIItem>(); // 생성된 아이템 리스트
    private bool isAccepted = false;

    public Action onChangedInventory;

    private void Awake()
    {
        _btnEquip.onClick.AddListener(OnClickEquip);
        _btnGoEnhance.onClick.AddListener(() => {
            _enchantPanel.Init(_selectedGremlinData);
            });
        _btnGoMerge.onClick.AddListener(() =>
        {
            StartCoroutine(_mergePanel.GetFuseTarget(_selectedGremlinData));
        });
        onChangedInventory += Refresh;
    }

    private void OnDestroy()
    {
        onChangedInventory -= Refresh;
    }

    private void Start()
    {
        Close();
    }

    public void Refresh()
    {
        OpenPanel(_gremlinList._gremlinInventory);
    }

    //TODO 패널이 열릴 때 호출할 함수 (보유한 그렘린 리스트를 넘겨받아야 함)
    public void OpenPanel(List<Gremlin> ownedGremlins)
    {
        ClearScrollContent();                      // 스크롤 초기화


        //보유 그렘린들 속 데이터들 대상으로
        foreach (var data in ownedGremlins)
        {
            //게임 오브젝트 생성
            GameObject itemObj = Instantiate(_gremlinItemPrefab, _scrollContent);
            //생성된 오브젝트의 GremlinUIItem 클래스 저장
            GremlinUIItem uiItem = itemObj.GetComponent<GremlinUIItem>();
            
            //그 클래스가 존재하면
            if (uiItem != null)
            {
                //초기화 함수 실행 후 생성된 아이템 리스트에 담기
                uiItem.Init(data, this);
                _createdItems.Add(uiItem);

                //데이터상에서 장착 중이라고 되어있으면
                if (data._isEquipped)
                {
                    //장착 UI에 넣기
                    _equippedUIItem = uiItem;
                }
            }
        }

        // TODO 첫 번째 아이템을 기본으로 선택 처리, 추후 기획팀과 대화해볼 내용
        if (_createdItems.Count > 0)
        {
            //첫 번째 아이템을 기본 선택 상태로 처리.
            _createdItems[0]._btnItem.onClick.Invoke();
        }
    }

    // 개별 슬롯을 터치했을 때 호출됨
    public void OnGremlinSelected(GremlinUIItem item, Gremlin data)
    {
        //이미 선택한 UI아이템이 존재하는 경우
        if (_selectedUIItem != null)
        {
            //선택 상태 false처리.
            _selectedUIItem.UpdateSelectState(false);
        }

        //선택 UI Item 교체, 선택한 그렘린 데이터 추가
        _selectedUIItem = item;
        _selectedGremlinData = data;
        //교체한 item의 선택상태 true 처리
        _selectedUIItem.UpdateSelectState(true);

        //최상단 패널 업데이트
        UpdateTopPanelAsync(data);
    }

    private void UpdateTopPanelAsync(Gremlin data)
    {
        //이미지 초상화 존재할 경우, 그 초상화 스프라이트는 데이터에서 지정한 스프라이트로.
        if (_imgPortrait != null)
        {
            var icon = data._gremlinData.sprite;
            _imgPortrait.sprite = icon;
        };
        //이름이 존재하는 경우, 데이터에서 그렘린 이름을 찾아 지정.
        if (_txtName != null)
        {
            _txtName.color = RarityColor.GetColor(data._rarity);
            _txtName.text = DataManager.Instance.GetData<GremlinData>(data._gremlinData.PetID).Gremlin_Name;
        }
        //레벨이 존재하는 경우, 레벨 텍스트는 아래 형식.
        if (_txtLevel != null) _txtLevel.text = $"강화 : {data._currentLevel}";
        if (_txtStat != null && data._gremlinData.Type == Gremlin_Type.공격형)
        {
            List<Gremlin_StatusData> listData = DataManager.Instance.GetList<Gremlin_StatusData>(data._gremlinData.PetID);
            string text = "";
            foreach(var statusData in listData)
            {
                text += $"공격력 : {statusData.Gremlin_Atk} 공격속도 : {(statusData.Gremlin_Dps * DataManager.Instance.GetData<Gremlin_AtkerData>((int)data._rarity).Gremlin_Tier_Dps).ToString("F2")}/초";
            }
            _txtStat.text = text;
        }
        else if(_txtStat != null && data._gremlinData.Type == Gremlin_Type.지원형)
        {
            List<Gremlin_StatusData> listData = DataManager.Instance.GetList<Gremlin_StatusData>(data._gremlinData.PetID);
            string text = "";
            foreach (var statusData in listData)
            {
                if(statusData.Effect_Type == Effect_Type.Active)
                {
                    text += $"체력 회복 : {DataManager.Instance.GetData<Gremlin_BufferData>((int)data._rarity).Gremlin_Tier_Cooltime}초마다 {statusData.Buff_Value * 100}% ";
                }
                else if(statusData.Effect_Type == Effect_Type.Passive)
                {
                    text += $"스텟 버프 : {statusData.Gremlin_Buff} + {(statusData.Buff_Value * 100).ToString("F1")}%";
                }
            }
            _txtStat.text = text;
        }
        if (_txtRarityStat != null) _txtRarityStat.text = $"등급 보너스 : {DataManager.Instance.GetData<Gremlin_TierData>((int)data._rarity).Gremlin_Tier_Multiplier * 100}%";
        if (_txtUpgradeStat != null) _txtUpgradeStat.text = data._gremlinData.Type == Gremlin_Type.공격형?
                $"강화 보너스 : 공격력 + {DataManager.Instance.GetData<Gremlin_AtkerData>((int)data._rarity).Gremlin_Level_Bonus * data._currentLevel}":
                $"강화 보너스 : {DataManager.Instance.GetData<Gremlin_BufferData>((int)data._rarity).Gremlin_Level_Bonus * data._currentLevel * 100}%";
    }

    private void OnClickEquip()
    {
        //선택한 아이템이나 선택한 오브젝트의 그렘린 데이터가 null이면 반환
        if (_selectedUIItem == null || _selectedGremlinData == null) return;

        // 이미 장착된 녀석이라면 무시
        if (_selectedUIItem == _equippedUIItem)
        {
            _noticeUIPanel.ChangeNoticeTitle("오류");
            _noticeUIPanel.ChangeNoticeDescription("이미 장착된 그렘린입니다.");
            _noticeUIPanel.ChangeNoticePanelLogic(true);
            _noticeUIPanel.Open();
            return;
        }

        if(isAccepted == false && _equippedUIItem != null && _selectedUIItem.GetGremlin()._rarity < _equippedUIItem.GetGremlin()._rarity)
        {
            _noticeUIPanel.ChangeNoticeTitle("오류");
            _noticeUIPanel.ChangeNoticeDescription("장착 중인 그렘린보다 약합니다.\n정말 교체하시겠습니까?");
            _noticeUIPanel.ChangeNoticePanelLogic(false);
            _noticeUIPanel.Open();

            _noticeUIPanel.Acceptbtn.onClick.AddListener(AcceptModify);
            _noticeUIPanel.Acceptbtn.onClick.AddListener(OnClickEquip);
            _noticeUIPanel.Acceptbtn.onClick.AddListener(_noticeUIPanel.Close);
            return;
        }

        //장비한 UIItem이 존재하는 경우
        if (_equippedUIItem != null)
        {
            //장착한 아이템의 장착상태 false
            _equippedUIItem.UpdateEquipState(false);
        }

        //장착 아이템은 현재 선택한 아이템
        _equippedUIItem = _selectedUIItem;
        //장비 상태 업데이트 true
        _equippedUIItem.UpdateEquipState(true);

        if(isAccepted == true)
        {
            AcceptModify();
        }
        _noticeUIPanel.Acceptbtn.onClick.RemoveAllListeners();
    }

    private void AcceptModify()
    {
        isAccepted = !isAccepted;
    }

    private void ClearScrollContent()
    {
        //생성된 아이템들 기준
        foreach (var item in _createdItems)
        {
            //내용물이 있으면 파괴
            if (item != null) Destroy(item.gameObject);
        }
        //생성된 아이템들 리스트 초기화
        _createdItems.Clear();
        //선택된 아이템 null, 장착된 아이템 null
        _selectedUIItem = null;
        _equippedUIItem = null;
    }

    protected override void OnOpen()
    {
        _gremlinList.SortInventory();
        OpenPanel(_gremlinList._gremlinInventory);
    }

    protected override void OnClose()
    {
        if(_selectedUIItem != null)
        {
            GremlinManager.Instance.StartCoroutine(GremlinManager.Instance.ChangeGremlin(_equippedUIItem?.GetGremlin()));
        }
    }
}