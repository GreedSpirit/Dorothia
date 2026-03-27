using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class GremlinUIItem : MonoBehaviour
{
    [SerializeField] private Image _imgIcon;           // 아이콘
    [SerializeField] private Image _imgSelectBorder;   // 선택했는지 보여주기 위한 보더
    [SerializeField] private Image _imgEquipBorder;    // 장착했는지 보여주기 위한 보더
    [SerializeField] public Button _btnItem;           // 아이템 버튼

    private Gremlin _itemData;
    private GremlinUIPanel _parentPanel;

    public void Init(Gremlin itemData, GremlinUIPanel parentPanel)
    {
        //그렘린 아이템데이터, 부모 패널 연결
        _itemData = itemData;
        _parentPanel = parentPanel;

        //이미지 아이콘이 존재하며 아이템데이터에 아이콘스프라이트가 존재하는 경우
        if(_imgIcon != null && itemData._gremlinData.sprite != null)
        {
            //이미지 아이콘의 스프라이트를 아이템데이터에서 가져옴.
            var icon = itemData._gremlinData.sprite;
            _imgIcon.sprite = icon;
        }

        // 초기 테두리 상태 설정
        UpdateSelectState(false);
        UpdateEquipState(itemData._isEquipped);

        // 버튼 이벤트 등록
        _btnItem.onClick.RemoveAllListeners();
        _btnItem.onClick.AddListener(OnClickItem);
    }

    public Gremlin GetGremlin()
    {
        Debug.Log(_itemData == null);
        if (_itemData == null) return null;
        return _itemData;
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
        _itemData._isEquipped = isEquipped;
    }

    private void OnClickItem()
    {
        // 부모 패널에 이 아이템이 선택되었음을 알림
        _parentPanel.OnGremlinSelected(this, _itemData);
    }
}
