using System;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class SkillItem : MonoBehaviour
{
    public enum SlotType { Skill, Scroll, InfoDetail, MergeResult }
    public enum DisplayMode { None, Info, Selection }
    public DisplayMode _currentMode = DisplayMode.None;

    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image outLine_Grade;
    [SerializeField] private GameObject outLine_Base;
    [SerializeField] private GameObject outLine_Scroll;
    [SerializeField] private GameObject newNoti;

    [Header("Dynamic Info (Text & Slider)")]
    [SerializeField] private GameObject countObj;
    [SerializeField] private TextMeshProUGUI countText;     // 보유 개수 텍스트
    [SerializeField] private TextMeshProUGUI levelText;     // 레벨 텍스트
    [SerializeField] private GameObject resultCount;        // 결과 개수 부모
    [SerializeField] private TextMeshProUGUI resultCountText; // 결과 개수 텍스트
    [SerializeField] private TextMeshProUGUI equipText; // 장착 여부 텍스트
    [SerializeField] private Slider countSlider;

    private AsyncOperationHandle<Sprite> iconHandle;
    private SkillPanel _skillPanel;
    private Button button;
    private SkillKey _key;
    private Action<SkillKey, SkillItem> _onSelected;
    private int _targetIdx = -1;

    public Image OutLine_Grade { get => outLine_Grade; set => outLine_Grade = value; }

    private void Awake()
    {
        _skillPanel = GetComponentInParent<SkillPanel>();

        button = GetComponent<Button>();
        button.onClick.AddListener(Click_Item);

    }

    private void OnEnable()
    {
        SkillManager.Instance.OnInventoryChanged += UpdateValueUI;
        SkillManager.Instance.OnEquipSkillChanged += UpdateEquipUI;
    }

    private void OnDisable()
    {
        SkillManager.Instance.OnInventoryChanged -= UpdateValueUI;
        SkillManager.Instance.OnEquipSkillChanged -= UpdateEquipUI;
    }

    public void Click_Item()
    {
        if (_currentMode == DisplayMode.None) return;

        if (_currentMode == DisplayMode.Info)
        {
            _skillPanel.Click_SkillInfo(_key, _targetIdx);
        }
        else
        {
            _onSelected?.Invoke(_key, this);
        }
    }

    /// <summary>
    /// 슬롯의 데이터를 설정하고 비주얼을 갱신합니다.
    /// </summary>
    public void SetSlotData(SlotType type, SkillKey key, int targetIdx = -1, DisplayMode display = DisplayMode.None, Action<SkillKey, SkillItem> onSelected = null)
    {
        _key = key;

        _currentMode = display;
        _onSelected = onSelected;
        _targetIdx = targetIdx;

        // 1. 데이터 가져오기 (DataManager 연동)
        var skillData = DataManager.Instance.GetData<SkillData>(key.sid);
        string iconAddress = skillData.Skill_Icon;

        // 2. 상태별 UI 활성화 로직 실행
        UpdateSlotVisual(type);

        // 3. 텍스트 및 슬라이더 업데이트 (추가된 부분)
        UpdateValueUI(key);

        // 4. 어드레서블 아이콘 로드
        LoadIconAddressable(iconAddress);

        // 5. 등급에 따른 스프라이트 적용
        ApplyGradeSprite(key.rarity);
    }

    private void UpdateSlotVisual(SlotType type)
    {
        countObj.SetActive(false);
        OutLine_Grade.gameObject.SetActive(false);
        outLine_Base.SetActive(false);
        outLine_Scroll.SetActive(false);
        levelText.gameObject.SetActive(false);
        newNoti.SetActive(false);
        resultCount.SetActive(false);
        equipText.gameObject.SetActive(false);
        if (countSlider != null) countSlider.gameObject.SetActive(false);

        switch (type)
        {
            case SlotType.Skill:
                countObj.SetActive(true);
                OutLine_Grade.gameObject.SetActive(true);
                levelText.gameObject.SetActive(true);
                if (countSlider != null) countSlider.gameObject.SetActive(true);
                if (_currentMode == DisplayMode.Info) equipText.gameObject.SetActive(true);
                break;
            case SlotType.Scroll:
                countObj.SetActive(true);
                outLine_Scroll.SetActive(true);
                OutLine_Grade.gameObject.SetActive(false);
                if (countSlider != null) countSlider.gameObject.SetActive(true);
                break;
            case SlotType.InfoDetail:
                countObj.SetActive(true);
                OutLine_Grade.gameObject.SetActive(true);
                break;
            case SlotType.MergeResult:
                OutLine_Grade.gameObject.SetActive(true);
                newNoti.SetActive(true);
                resultCount.SetActive(true);
                break;
        }
    }

    private void UpdateEquipUI(Skill_Type type, int idx)
    {
        // 1. 현재 이 스킬의 전역 장착 상태 확인 (인벤토리 아이콘 UI 갱신용)
        bool isEquipped = SkillManager.Instance.IsEquipped(_key);

        // 2. 장착 중 표시(Text/Icon) 활성화/비활성화
        if (equipText != null)
        {
            equipText.gameObject.SetActive(isEquipped);
        }

        // 3. 슬롯 인덱스 유효성 검사 및 타겟 스킬 가져오기
        BaseSkill targetSkill = null;

        switch (type)
        {
            case Skill_Type.Active:
                // 액티브 슬롯 범위 체크 (0~2)
                if (idx >= 0 && idx < SkillManager.ACTIVE_SLOT_MAX)
                    targetSkill = SkillManager.Instance.ActiveSlots[idx];
                break;

            case Skill_Type.Passive:
                // 패시브 슬롯 범위 체크 (0~4)
                if (idx >= 0 && idx < SkillManager.PASSIVE_SLOT_MAX)
                    targetSkill = SkillManager.Instance.PassiveSlots[idx];
                break;

            default: // Ultimate 등
                targetSkill = SkillManager.Instance.UltimateSlot;
                break;
        }

        // 4. 슬롯이 비어있거나 타겟 스킬이 없는 경우 얼리 리턴
        if (targetSkill == null || targetSkill.Data == null)
        {
            return;
        }

        // 5. 현재 UI의 스킬 ID와 실제 슬롯에 장착된 스킬 ID 비교 (강조 연출 등)
        if (_key.sid == targetSkill.Data.Job_Skill_Id)
        {
            // 연출 로직 (예: 장착된 슬롯 테두리 하이라이트 등)
        }
    }

    private void UpdateValueUI(SkillKey key)
    {
        if (_key != key) return;

        int count = SkillManager.Instance.GetItemCount(key);
        if (!key.isScroll)
        {
            if (SkillManager.Instance.UnlockedSkills.TryGetValue(key, out BaseSkill bs))
            {
                if (levelText != null)
                    levelText.text = $"{bs.Level}";
            }
        }

        if (equipText != null)
        {
            // SkillManager에서 장착 여부를 가져옴
            bool isEquipped = SkillManager.Instance.IsEquipped(_key);

            // 장착 중일 때만 텍스트를 활성화하거나 내용을 변경
            equipText.gameObject.SetActive(isEquipped);
            //if (isEquipped)
            //{
            //    equipText.text = "EQUIPPED"; // 또는 "장착 중"
            //}
        }

        if (resultCountText != null) resultCountText.text = $"x{count}";
        Color textColor = count >= 3 ? Color.green : Color.red;
        if (countText != null)
        {
            countText.color = textColor;
            countText.text = $"{count}/{3}";
        }

        if (countSlider != null)
        {
            countSlider.value = count / 3;
        }
    }

    private void LoadIconAddressable(string address)
    {
        if (iconHandle.IsValid()) Addressables.Release(iconHandle);
        iconHandle = Addressables.LoadAssetAsync<Sprite>(address);
        iconHandle.Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                iconImage.sprite = handle.Result;
        };
    }

    private void ApplyGradeSprite(Rarity grade)
    {
        Sprite targetSprite = SkillManager.Instance.GetSpriteByGrade(grade);
        if (targetSprite != null) OutLine_Grade.sprite = targetSprite;
    }

    private void OnDestroy()
    {
        if (iconHandle.IsValid()) Addressables.Release(iconHandle);
    }
}