using UnityEngine;
using UnityEngine.UI;

public class PromotionIconView : MonoBehaviour
{
    private Image _icon;
    private string _currentIconKey; // 현재 로드된 키 추적

    private void Awake()
    {
        _icon = GetComponent<Image>();
    }

    private void Start()
    {
        PlayerStats.Instance.OnPromotionChanged += RefreshIcon;
        RefreshIcon(PlayerStats.Instance.CurrentPromotion); // 초기값 반영
    }

    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnPromotionChanged -= RefreshIcon;

        ReleaseIcon();
    }

    private void RefreshIcon(int currentPromotion)
    {
        Debug.Log(currentPromotion);
        Character_RankData data = DataManager.Instance.GetData<Character_RankData>(currentPromotion);
        //Debug.Log(data.Character_Icon);
        string newKey = data.Character_Icon;

        // 같은 키면 재로드 불필요
        if (_currentIconKey == newKey) return;

        // 이전 아이콘 릴리즈
        ReleaseIcon(); 

        _currentIconKey = newKey;

        AddressableManager.Instance.LoadAsset<Sprite>(newKey, (sprite) =>
        {
            // 로드 완료 전에 오브젝트가 파괴된 경우 방어
            if (_icon != null)
                _icon.sprite = sprite;
        });
    }

    private void ReleaseIcon()
    {
        if (string.IsNullOrEmpty(_currentIconKey)) return;

        AddressableManager.Instance.ReleaseAsset(_currentIconKey);
        _currentIconKey = null;
        _icon.sprite = null;
    }
}