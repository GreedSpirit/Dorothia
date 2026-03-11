using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillMergeResultPanel : BaseUI
{
    [Header("결과 텍스트 및 상태")]
    [SerializeField] private TextMeshProUGUI resultStatusText; // "합성 성공!" 또는 "합성 실패..."
    [SerializeField] private Color successColor = Color.yellow;
    [SerializeField] private Color failColor = Color.gray;

    [Header("획득 아이템 정보")]
    [SerializeField] private Image resultIcon;
    [SerializeField] private TextMeshProUGUI resultNameText;
    //[SerializeField] private GameObject effectVisual; // 성공 시 연출용 이펙트 (선택 사항)

    private string _currentIconKey;

    protected override void OnOpen()
    {
        // 열릴 때 간단한 애니메이션이나 사운드를 추가할 수 있습니다.
    }

    protected override void OnClose()
    {
        AddressableManager.Instance.ReleaseAsset(_currentIconKey);
    }

    /// <summary>
    /// 합성이 완료된 후 호출되어 UI를 세팅합니다.
    /// </summary>
    /// <param name="isSuccess">합성 성공 여부</param>
    /// <param name="rewardKey">보상으로 받은 스킬 키 (실패 시에도 하위 템 등을 보여준다면 사용)</param>
    public void Setup(bool isSuccess, SkillKey rewardKey)
    {
        if (rewardKey == null) return;

        rewardKey.rarity++;

        // 1. 성공/실패 상태 UI 설정
        resultStatusText.text = isSuccess ? "합성 성공!" : "합성 실패";
        resultStatusText.color = isSuccess ? successColor : failColor;

        //if (effectVisual != null)
        //    effectVisual.SetActive(isSuccess);

        // 2. 데이터 로드 및 UI 반영
        var data = DataManager.Instance.GetData<SkillData>(rewardKey.sid);
        if (data != null)
        {
            resultNameText.text = data.Skill_Name;

            _currentIconKey = data.Skill_Icon;
            AddressableManager.Instance.LoadAsset<Sprite>(_currentIconKey, (sprite) =>
            {
                if (resultIcon != null) resultIcon.sprite = sprite;
            });
        }
    }

    // '확인' 버튼 등에 연결
    public void Click_Confirm()
    {
        Close();
    }
}