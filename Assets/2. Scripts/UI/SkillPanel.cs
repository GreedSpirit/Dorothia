using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SkillPanel : BaseUI
{
    [SerializeField] private SkillInfoPopup skillInfo;
    [SerializeField] private SkillListPanel skillListPanel;

    [Header("Slots")]
    [SerializeField] private List<Image> activeSlotImages = new List<Image>();
    [SerializeField] private List<Image> passiveSlotImages = new List<Image>();
    [SerializeField] private Image ultimateSlotImage;

    // 현재 각 슬롯에 로드된 아이콘의 주소를 추적하기 위한 리스트/변수
    private List<string> _loadedActiveAddress = new List<string>();
    private List<string> _loadedPassiveAddress = new List<string>();
    private string _loadedUltimateAddress;

    protected override void OnOpen()
    {
        SkillManager.Instance.OnEquipSkillChanged += UpdateUIEquipedSkills;
        UpdateUIEquipedSkills(); // 열릴 때 초기화
    }

    protected override void OnClose()
    {
        SkillManager.Instance.OnEquipSkillChanged -= UpdateUIEquipedSkills;
        ReleaseAllIcons();
    }

    private void UpdateUIEquipedSkills()
    {
        // 1. 기존 리소스 해제 (필요 시)
        ReleaseAllIcons();

        // 2. 타입별로 필터링된 리스트 준비
        var equipped = SkillManager.Instance.SkillSlots;
        var activeSkills = equipped.Where(x => x.Data.Skill_Type == Skill_Type.Active).ToList();
        var passiveSkills = equipped.Where(x => x.Data.Skill_Type == Skill_Type.Passive).ToList();
        var ultimateSkill = equipped.FirstOrDefault(x => x.Data.Skill_Type == Skill_Type.Ultimate);

        // 3. 각 그룹 업데이트
        UpdateSlotGroup(activeSlotImages, activeSkills, _loadedActiveAddress);
        UpdateSlotGroup(passiveSlotImages, passiveSkills, _loadedPassiveAddress);

        // 4. 궁극기 개별 처리
        UpdateUltimateSlot(ultimateSkill);
    }

    private void UpdateSlotGroup(List<Image> slots, List<BaseSkill> targetSkills, List<string> addressTracker)
    {
        addressTracker.Clear(); // 새로운 로드를 위해 트래커 초기화

        for (int i = 0; i < slots.Count; i++)
        {
            // 클로저 이슈 방지를 위해 인덱스 캡처
            int index = i;

            if (i < targetSkills.Count && targetSkills[i]?.Data != null)
            {
                string addr = targetSkills[i].Data.Skill_Icon;
                addressTracker.Add(addr);

                AddressableManager.Instance.LoadAsset<Sprite>(addr, sprite =>
                {
                    if (slots[index] != null) slots[index].sprite = sprite;
                });
            }
            else
            {
                slots[i].sprite = null; // 스킬이 없는 빈 슬롯 처리
            }
        }
    }

    private void UpdateUltimateSlot(BaseSkill ultimateSkill)
    {
        if (ultimateSkill != null)
        {
            _loadedUltimateAddress = ultimateSkill.Data.Skill_Icon;
            AddressableManager.Instance.LoadAsset<Sprite>(_loadedUltimateAddress, sprite =>
            {
                if (ultimateSlotImage != null) ultimateSlotImage.sprite = sprite;
            });
        }
        else
        {
            _loadedUltimateAddress = string.Empty;
            ultimateSlotImage.sprite = null;
        }
    }

    private void ReleaseAllIcons()
    {
        // 액티브 해제
        foreach (var addr in _loadedActiveAddress)
            AddressableManager.Instance.ReleaseAsset(addr);
        _loadedActiveAddress.Clear();

        // 패시브 해제
        foreach (var addr in _loadedPassiveAddress)
            AddressableManager.Instance.ReleaseAsset(addr);
        _loadedPassiveAddress.Clear();

        // 궁극기 해제
        AddressableManager.Instance.ReleaseAsset(_loadedUltimateAddress);
        _loadedUltimateAddress = null;
    }

    public void Click_SkillInfo(SkillKey key)
    {
        if (key == null) return;
        skillInfo.Setup(key);
        UIManager.Instance.OpenPanel(skillInfo);
    }

    public void Click_SkillList(int type)
    {
        skillListPanel.SetOpenType((Skill_Type)type);
        UIManager.Instance.OpenPanel(skillListPanel);
    }
}