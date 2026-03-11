using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MergeNotificationPopup : BaseUI
{
    [Header("아이콘 관련")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI scrollCount;

    [Header("슬라이더 설정")]
    [SerializeField] private Slider mergeSlider;
    [SerializeField] private TextMeshProUGUI mergeMaxCount;

    public SkillKey Key { get; set; }
    private SkillData data;

    // 현재 슬라이더로 선택된 합성 수량
    private int currentMergeCount;

    protected override void OnClose()
    {
        // 이벤트 리스너 제거 (메모리 누수 방지)
        mergeSlider.onValueChanged.RemoveListener(UpdateMergeCountText);
        AddressableManager.Instance.ReleaseAsset(data.Skill_Icon);
    }

    protected override void OnOpen()
    {
        if (Key == null)
        {
            Debug.LogWarning($"키가 할당되지 않았습니다");
            return;
        }

        data = DataManager.Instance.GetData<SkillData>(Key.sid);
        AddressableManager.Instance.LoadAsset<Sprite>(data.Skill_Icon, s => icon.sprite = s);
        skillName.text = data.Skill_Name;

        int total = SkillManager.Instance.GetItemCount(Key);
        int vaildMax = total / 3;

        scrollCount.text = $"{total}/3";

        // 중복 리스너 방지
        mergeSlider.onValueChanged.RemoveAllListeners();
        // 최소 1개 (보유량이 없으면 0)
        mergeSlider.minValue = vaildMax > 0 ? 1 : 0;
        mergeSlider.maxValue = vaildMax;
        // 정수 단위로만 움직이게 설정
        mergeSlider.wholeNumbers = true;

        mergeSlider.value = vaildMax;
        currentMergeCount = vaildMax;

        mergeSlider.onValueChanged.AddListener(UpdateMergeCountText);

        // 초기 텍스트 업데이트
        UpdateMergeCountText(mergeSlider.value);
    }

    // 슬라이더 값 변경 시 호출되는 함수
    private void UpdateMergeCountText(float value)
    {
        currentMergeCount = (int)value;
        mergeMaxCount.text = $"{currentMergeCount}";
    }

    public void Click_Merge()
    {
        SkillManager.Instance.CraftSkill(Key, currentMergeCount);

        UIManager.Instance.CloseTopPanel();
    }
}