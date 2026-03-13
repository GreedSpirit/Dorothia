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
    [SerializeField] public Image gradeOutlineImage;
    [SerializeField] private GameObject outLine_Base;
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
        gradeOutlineImage.gameObject.SetActive(false);
        outLine_Base.SetActive(false);
        levelText.gameObject.SetActive(false);
        newNoti.SetActive(false);
        resultCount.SetActive(false);
        equipText.gameObject.SetActive(false);
        if (countSlider != null) countSlider.gameObject.SetActive(false);

        switch (type)
        {
            case SlotType.Skill:
                countObj.SetActive(true);
                gradeOutlineImage.gameObject.SetActive(true);
                levelText.gameObject.SetActive(true);
                if (countSlider != null) countSlider.gameObject.SetActive(true);
                if (_currentMode == DisplayMode.Info) equipText.gameObject.SetActive(true);
                break;
            case SlotType.Scroll:
                countObj.SetActive(true);
                outLine_Base.SetActive(true);
                if (countSlider != null) countSlider.gameObject.SetActive(true);
                break;
            case SlotType.InfoDetail:
                countObj.SetActive(true);
                gradeOutlineImage.gameObject.SetActive(true);
                break;
            case SlotType.MergeResult:
                gradeOutlineImage.gameObject.SetActive(true);
                newNoti.SetActive(true);
                resultCount.SetActive(true);
                break;
        }
    }

    private void UpdateEquipUI(Skill_Type type, int idx)
    {
        // 1. 현재 이 스킬(아이템)이 전체 인벤토리/매니저 기준으로 장착 중인지 확인
        // targetSkill이 null이든 아니든, 이 정보는 SkillManager가 알고 있습니다.
        bool isEquipped = SkillManager.Instance.IsEquipped(_key);

        // 2. 텍스트 상태 즉시 반영 (여기서 다른 아이템들도 상태가 동기화됨)
        if (equipText != null)
        {
            equipText.gameObject.SetActive(isEquipped);
        }

        // 3. [선택 사항] 만약 이 스킬이 방금 '해제'된 대상인지 로그를 찍거나 
        // 특정 연출을 하고 싶을 때만 아래 null 체크를 사용합니다.
        BaseSkill targetSkill = type switch
        {
            Skill_Type.Active => SkillManager.Instance.ActiveSlots[idx],
            Skill_Type.Passive => SkillManager.Instance.PassiveSlots[idx],
            _ => SkillManager.Instance.UltimateSlot
        };

        if (targetSkill == null)
        {
            // 슬롯이 비었음을 확인했지만, 위에서 이미 내 장착 상태를 갱신했으므로 
            // 추가 로직이 없다면 여기서 종료해도 무방합니다.
            return;
        }

        // 4. 내가 방금 이 슬롯에 들어온 주인공이라면 추가 작업 (예: 강조 연출)
        if (_key.sid == targetSkill.Data.Job_Skill_Id)
        {
            // Debug.Log($"{targetSkill.Data.Skill_Name}이(가) {idx}번 슬롯에 장착됨!");
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
        if (targetSprite != null) gradeOutlineImage.sprite = targetSprite;
    }

    private void OnDestroy()
    {
        if (iconHandle.IsValid()) Addressables.Release(iconHandle);
    }
}