using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [SerializeField] GameObject _infoPanel; // UI 패널
    [SerializeField] Image _infoIcon;
    [SerializeField] TextMeshProUGUI _infoDescription;
    [SerializeField] Button _confirmButton;
    [SerializeField] Button _cancelButton;
    [SerializeField] EquipmentUI equipmentUI;

    private SlotType _slotType;
    private EquipSlot _targetSlot;
    private Equipment _selectedEquipment;
    private void Awake()
    {
        if(Instance != null &&  Instance != this)
            Destroy(gameObject);

        Instance = this;
        DontDestroyOnLoad(Instance);
    }

    public void SetTargetSlot(EquipSlot slot)
    {
        _targetSlot = slot;
    }

    public EquipSlot GetTargetSlot()
    {
        return _targetSlot;
    }

    public void OnClickItem(Equipment equip)
    {
        _selectedEquipment = equip;
        _infoPanel.SetActive(true);
        _infoIcon.sprite = equip.icon;
        _infoDescription.text = $"이름: {equip.name}";
        _infoDescription.color = RarityColor.GetColor(equip.equipmentRarity);
        _confirmButton.onClick.RemoveAllListeners();
        _confirmButton.onClick.AddListener(() =>
        {
            AddToFuseSlot(_selectedEquipment);
        });
        _cancelButton.onClick.RemoveAllListeners();
        _cancelButton.onClick.AddListener(() =>
        {
            RemoveFromFuseSlot();
        });
    }

    private void AddToFuseSlot(Equipment equip)
    {
        if (_targetSlot != null)
        {
            _targetSlot.equipped = equip;
            _targetSlot.iconImage.sprite = equip.icon;
            _targetSlot.iconImage.enabled = true;
            _targetSlot.iconImage.color = RarityColor.GetColor(equip.equipmentRarity);
        }
    }

    private void RemoveFromFuseSlot()
    {
        if(_targetSlot != null)
        {
            _targetSlot.equipped = null;
            _targetSlot.iconImage.sprite = null;
        }
    }
}
