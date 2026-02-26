using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GremlinUIPanel : MonoBehaviour
{
    [Header("Top Panel (Info)")]
    [SerializeField] private Image _imgPortrait;
    [SerializeField] private TextMeshProUGUI _txtName;
    [SerializeField] private TextMeshProUGUI _txtLevel;
    [SerializeField] private TextMeshProUGUI _txtStat;

    [Header("Middle Panel (Scroll View)")]
    [SerializeField] private Transform _scrollContent;
    [SerializeField] private GameObject _gremlinItemPrefab;

    [Header("Bottom Panel (Buttons)")]
    [SerializeField] private Button _btnEquip;
    [SerializeField] private Button _btnGoEnhance;
    [SerializeField] private Button _btnGoMerge;

    private GremlinItemData _selectedGremlinData;
    private GremlinUIItem _selectedUIItem;
    private GremlinUIItem _equippedUIItem;
    private List<GremlinUIItem> _createdItems = new List<GremlinUIItem>();

    private void Awake()
    {
        _btnEquip.onClick.AddListener(OnClickEquip);
        _btnGoEnhance.onClick.AddListener(OnClickGoEnhance);
        _btnGoMerge.onClick.AddListener(OnClickGoMerge);
    }

    //TODO 패널이 열릴 때 호출할 함수 (보유한 그렘린 리스트를 넘겨받아야 함)
    public void OpenPanel(List<GremlinItemData> ownedGremlins)
    {
        gameObject.SetActive(true);
        ClearScrollContent();

        foreach (var data in ownedGremlins)
        {
            GameObject itemObj = Instantiate(_gremlinItemPrefab, _scrollContent);
            GremlinUIItem uiItem = itemObj.GetComponent<GremlinUIItem>();
            
            if (uiItem != null)
            {
                uiItem.Init(data, this);
                _createdItems.Add(uiItem);

                if (data.isEquipped)
                {
                    _equippedUIItem = uiItem;
                }
            }
        }

        // TODO 첫 번째 아이템을 기본으로 선택 처리, 추후 기획팀과 대화해볼 내용
        if (_createdItems.Count > 0)
        {
            _createdItems[0]._btnItem.onClick.Invoke();
        }
    }

    // 개별 슬롯을 터치했을 때 호출됨
    public void OnGremlinSelected(GremlinUIItem item, GremlinItemData data)
    {
        if (_selectedUIItem != null)
        {
            _selectedUIItem.UpdateSelectState(false);
        }

        _selectedUIItem = item;
        _selectedGremlinData = data;
        _selectedUIItem.UpdateSelectState(true);

        UpdateTopPanel(data);
    }

    private void UpdateTopPanel(GremlinItemData data)
    {
        if (_imgPortrait != null) _imgPortrait.sprite = data.iconSprite;
        if (_txtName != null) _txtName.text = data.gremlinName;
        if (_txtLevel != null) _txtLevel.text = $"Lv. {data.currentLevel}";
        
        // TODO 공격력인지 버프 수치인지 표시 포맷은 추후 수정 가능
        if (_txtStat != null) _txtStat.text = $"능력치: {data.currentStat}"; 
    }

    private void OnClickEquip()
    {
        if (_selectedUIItem == null || _selectedGremlinData == null) return;

        // 이미 장착된 녀석이라면 무시
        if (_selectedUIItem == _equippedUIItem) return;

        if (_equippedUIItem != null)
        {
            _equippedUIItem.UpdateEquipState(false);
        }

        _equippedUIItem = _selectedUIItem;
        _equippedUIItem.UpdateEquipState(true);

        // TODO: 여기서 GremlinManager.EquipGremlin() 호출
        Debug.Log($"[{_selectedGremlinData.gremlinName}] 장착 완료!");
    }

    private void OnClickGoEnhance()
    {
        if (_selectedGremlinData == null) return;
        Debug.Log("강화 패널 열기 요청 - 선택된 그렘린: " + _selectedGremlinData.gremlinName);
    }

    private void OnClickGoMerge()
    {
        if (_selectedGremlinData == null) return;
        Debug.Log("합성 패널 열기 요청 - 선택된 그렘린: " + _selectedGremlinData.gremlinName);
    }

    private void ClearScrollContent()
    {
        foreach (var item in _createdItems)
        {
            if (item != null) Destroy(item.gameObject);
        }
        _createdItems.Clear();
        _selectedUIItem = null;
        _equippedUIItem = null;
    }
}