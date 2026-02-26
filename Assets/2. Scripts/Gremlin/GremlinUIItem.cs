using UnityEngine;
using UnityEngine.UI;

public class GremlinItemData //TODO 아직 테이블이 없어서 만든 임시 데이터 클래스(추후 테이블에 맞게 변경)
{
    public int id;
    public string gremlinName;
    public int currentLevel;
    public float currentStat; // 공격력, 공속, 쿨타임 등
    public Rarity tier;
    public Sprite iconSprite;
    public bool isEquipped;
}

public class GremlinUIItem : MonoBehaviour
{
    [SerializeField] private Image _imgIcon;
    [SerializeField] private Image _imgSelectBorder;
    [SerializeField] private Image _imgEquipBorder;
    [SerializeField] public Button _btnItem;

    private GremlinItemData _itemData;
    private GremlinUIPanel _parentPanel;

    public void Init(GremlinItemData itemData, GremlinUIPanel parentPanel)
    {
        _itemData = itemData;
        _parentPanel = parentPanel;

        if(_imgIcon != null && itemData.iconSprite != null)
        {
            _imgIcon.sprite = itemData.iconSprite;
        }

        // 초기 테두리 상태 설정
        UpdateSelectState(false);
        UpdateEquipState(itemData.isEquipped);

        // 버튼 이벤트 등록
        _btnItem.onClick.RemoveAllListeners();
        _btnItem.onClick.AddListener(OnClickItem);
    }

    public void UpdateSelectState(bool isSelected)
    {
        if (_imgSelectBorder != null)
        {
            _imgSelectBorder.gameObject.SetActive(isSelected);
        }
    }

    public void UpdateEquipState(bool isEquipped)
    {
        if (_imgEquipBorder != null)
        {
            _imgEquipBorder.gameObject.SetActive(isEquipped);
        }
        _itemData.isEquipped = isEquipped;
    }

    private void OnClickItem()
    {
        // 부모 패널에 이 아이템이 선택되었음을 알림
        _parentPanel.OnGremlinSelected(this, _itemData);
    }
}
