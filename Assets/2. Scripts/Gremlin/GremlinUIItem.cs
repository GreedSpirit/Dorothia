using UnityEngine;
using UnityEngine.UI;

public class GremlinItemData //TODO 아직 테이블이 없어서 만든 임시 데이터 클래스(추후 테이블에 맞게 변경)
{
    public int id;             // 아이디값
    public string gremlinName; // 그렘린의 이름
    public int currentLevel;   // 현재 그렘린의 레벨
    public float currentStat;  // 공격력, 공속, 쿨타임 등
    public Rarity tier;        // 그렘린의 등급
    public Sprite iconSprite;  // 그렘린의 스프라이트
    public bool isEquipped;    // 그렘린 장착 여부
}

public class GremlinUIItem : MonoBehaviour
{
    [SerializeField] private Image _imgIcon;           // 아이콘
    [SerializeField] private Image _imgSelectBorder;   // 선택했는지 보여주기 위한 보더
    [SerializeField] private Image _imgEquipBorder;    // 장착했는지 보여주기 위한 보더
    [SerializeField] public Button _btnItem;           // 아이템 버튼

    private GremlinItemData _itemData;
    private GremlinUIPanel _parentPanel;

    public void Init(GremlinItemData itemData, GremlinUIPanel parentPanel)
    {
        //그렘린 아이템데이터, 부모 패널 연결
        _itemData = itemData;
        _parentPanel = parentPanel;

        //이미지 아이콘이 존재하며 아이템데이터에 아이콘스프라이트가 존재하는 경우
        if(_imgIcon != null && itemData.iconSprite != null)
        {
            //이미지 아이콘의 스프라이트를 아이템데이터에서 가져옴.
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
        //이미지 선택 보드가 존재할 때
        if (_imgSelectBorder != null)
        {
            //인자값으로 받아온 isSelected값에 따라 해당 보드 활성화 여부 변경
            _imgSelectBorder.gameObject.SetActive(isSelected);
        }
    }

    public void UpdateEquipState(bool isEquipped)
    {
        //이미지 장착 보드가 존재할 떄
        if (_imgEquipBorder != null)
        {
            //인자값으로 받아온 isEquipped값에 따라 해당 보드 활성화 여부 변경
            _imgEquipBorder.gameObject.SetActive(isEquipped);
        }
        //isEquipped값에 따라 아이템데이터에 장착 여부 변경
        _itemData.isEquipped = isEquipped;
    }

    private void OnClickItem()
    {
        // 부모 패널에 이 아이템이 선택되었음을 알림
        _parentPanel.OnGremlinSelected(this, _itemData);
    }
}
