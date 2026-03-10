using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets; // 추가
using UnityEngine.ResourceManagement.AsyncOperations;
using Unity.VisualScripting; // 추가

public class SkillItem : MonoBehaviour
{
    private SkillKey key;

    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI skillCountText;

    private SkillPanel skillPanel;
    private Button button;

    // 어드레서블 핸들 캐싱 (메모리 해제용)
    private AsyncOperationHandle<Sprite> _iconHandle;

    private void Awake()
    {
        icon = GetComponent<Image>();
        skillPanel = GetComponentInParent<SkillPanel>();
        button = GetComponent<Button>();
        button.onClick.AddListener(Click_SkillInfo);
    }

    public void Setup(SkillKey key)
    {
        this.key = key;
        var data = DataManager.Instance.GetData<SkillData>(key.sid);

        // 초기 수량 설정 (SkillManager에서 현재 수량 가져오기)
        int currentCount = SkillManager.Instance.GetItemCount(key);
        UpdateUI(key, currentCount);

        LoadIcon(data.Skill_Icon);

        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.OnInventoryChanged -= UpdateUI;
            SkillManager.Instance.OnInventoryChanged += UpdateUI;
        }
    }

    private void LoadIcon(string iconAddress)
    {
        AddressableManager.Instance.LoadAsset<Sprite>(iconAddress, (sprite) =>
        {
            icon.sprite = sprite;
        });
    }

    private void UpdateUI(SkillKey key, int count)
    {
        if (this.key != key) return;
        skillCountText.text = $"{count} / 3";
    }

    private void Click_SkillInfo()
    {
        skillPanel.Click_SkillInfo(key);
    }

    private void ReleaseIcon()
    {
        var data = DataManager.Instance.GetData<SkillData>(key.sid);
        AddressableManager.Instance.ReleaseAsset(data.Skill_Icon);
    }

    private void OnDestroy()
    {
        // 이벤트 해제 (메모리 누수 방지)
        if (SkillManager.Instance != null)
            SkillManager.Instance.OnInventoryChanged -= UpdateUI;

        // 아이콘 리소스 해제
        ReleaseIcon();
    }
}