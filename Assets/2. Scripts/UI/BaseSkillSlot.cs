using System.Collections;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseSkillSlot : MonoBehaviour
{
    [Header("슬롯 설정")]
    [SerializeField] protected int slotIndex; // 에디터에서 0, 1, 2 등으로 설정
    [SerializeField] protected Skill_Type slotType = Skill_Type.Active; // 액티브용인지 궁극기용인지 구분

    [SerializeField] private GameObject skillObj;
    [SerializeField] private Image icon;
    [SerializeField] private Image backIcon;
    [SerializeField] private Image grade;

    [SerializeField] protected SkillListPanel listPanel;

    protected PlayerCtrl player;
    protected BaseSkill _skill;
    private Button button;

    public bool IsEquip { get; private set; }
    private string loadedIconAddr;

    private void Awake()
    {
        player = FindAnyObjectByType<PlayerCtrl>();
        if (!IsEquip) skillObj.SetActive(false);

        button = GetComponent<Button>();

        button.onClick.AddListener(Click_Slot);
    }

    private void OnEnable()
    {
        // Execution Order를 설정했다면 여기서 Instance는 절대 null이 아닙니다.
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.OnEquipSkillChanged -= RefreshUI;
            SkillManager.Instance.OnEquipSkillChanged += RefreshUI;
            RefreshUI(slotType, slotIndex);
        }
    }

    private void OnDisable()
    {
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.OnEquipSkillChanged -= RefreshUI;
        }
    }

    protected virtual void Update()
    {
        if (!IsEquip || _skill == null) return;
        if (_skill.Data.Skill_Type == Skill_Type.Passive) return;

        CoolTimeUI();
    }

    /// <summary>
    /// SkillManager의 상태를 확인하여 슬롯의 이미지를 동기화
    /// </summary>
    private void RefreshUI(Skill_Type type, int targetIdx)
    {
        if (slotIndex != targetIdx || slotType != type) return;

        Debug.LogWarning($"{slotIndex},{gameObject.name}");

        var sm = SkillManager.Instance;
        if (sm == null) return;

        BaseSkill targetSkill = null;

        // 1. 타입에 따라 매니저의 배열에서 내 인덱스에 맞는 스킬 가져오기
        if (slotType == Skill_Type.Active)
        {
            if (slotIndex >= 0 && slotIndex < sm.ActiveSlots.Length)
                targetSkill = sm.ActiveSlots[slotIndex];
        }
        else if (slotType == Skill_Type.Ultimate)
        {
            targetSkill = sm.UltimateSlot;
        }
        else
        {
            targetSkill = sm.PassiveSlots[slotIndex];
        }

        // 2. 스킬 상태에 따른 장착/해제 처리
        if (targetSkill != null)
        {
            Equip(targetSkill);
        }
        else
        {
            // 매니저 데이터가 비어있다면 UI상에서도 해제
            if (IsEquip) UnEquip();
        }
    }

    public void Equip(BaseSkill skill)
    {
        if (_skill == skill) return;

        // 기존 리소스 해제
        if (IsEquip) UnEquip();

        _skill = skill;
        IsEquip = true;
        skillObj.SetActive(true);

        CoolTimeUI();
        LoadIconAddressables();
    }

    public void UnEquip()
    {
        if (!IsEquip) return;

        // 리소스 해제
        if (!string.IsNullOrEmpty(loadedIconAddr))
        {
            AddressableManager.Instance.ReleaseAsset(loadedIconAddr);
            loadedIconAddr = null;
        }

        // 이미지 초기화
        icon.sprite = null;
        backIcon.sprite = null;

        _skill = null;
        IsEquip = false;
        skillObj.SetActive(false);
    }

    private void CoolTimeUI()
    {
        icon.fillAmount = _skill.CooldownRatio;
    }

    private void LoadIconAddressables()
    {
        if (_skill == null) return;

        var am = AddressableManager.Instance;
        loadedIconAddr = _skill.Data.Skill_Icon;

        am.LoadAsset<Sprite>(loadedIconAddr, (s) =>
        {
            // 콜백 시점에 이미 다른 스킬로 바뀌었거나 해제되었는지 확인
            if (s == null || !IsEquip || loadedIconAddr != _skill.Data.Skill_Icon) return;

            icon.sprite = s;
            backIcon.sprite = s;
        });

        grade.sprite = SkillManager.Instance.GetSpriteByGrade(_skill.Rarity);
    }

    public abstract void Click_Slot();
}
