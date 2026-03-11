using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SkillItem : MonoBehaviour
{
    // 모드 정의
    public enum DisplayMode { Info, Selection }
    private DisplayMode _currentMode;

    private SkillKey _key;
    private SkillData _cachedData;

    [SerializeField] private Image icon;
    [SerializeField] private GameObject newNoti; // TextMeshProUGUI 대신 GameObject로 껐다 켜는 게 효율적입니다.
    [SerializeField] private TextMeshProUGUI skillCountText;
    [SerializeField] private GameObject selectionVisual; // 선택 시 보여줄 테두리

    private SkillPanel _skillPanel; // 정보창 모드용
    private Action<SkillKey, SkillItem> _onSelected; // 선택 모드용 콜백
    private Button _button;

    private void Awake()
    {
        _skillPanel = GetComponentInParent<SkillPanel>();
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClickItem);
    }

    private void OnEnable()
    {
        SkillManager.Instance.OnInventoryChanged += UpdateUI;
    }

    // 풀링 시스템에서 호출할 초기화 함수
    public void Setup(SkillKey key, DisplayMode mode, Action<SkillKey, SkillItem> onSelected = null)
    {
        if (_key != null && _key.Equals(key))
        {
            _currentMode = mode;
            _onSelected = onSelected;

            UpdateUI(key, SkillManager.Instance.GetItemCount(key));
            return;
        }

        this._key = key;
        this._currentMode = mode;
        this._onSelected = onSelected;
        this._cachedData = DataManager.Instance.GetData<SkillData>(key.sid);

        // 1. UI 초기화
        //selectionVisual.SetActive(false);
        int currentCount = SkillManager.Instance.GetItemCount(key);
        UpdateUI(key, currentCount);

        // 2. 신규 알림(New) 처리
        // SkillManager에 IsNewSkill(key) 같은 로직이 있다고 가정합니다.
        if (newNoti != null)
            newNoti.SetActive(SkillManager.Instance.IsNewSkill(key));

        // 3. 아이콘 로드
        LoadIcon(_cachedData.Skill_Icon);
    }

    private void UpdateUI(SkillKey key, int count)
    {
        if (this._key != key) return;

        // 3개 이상일 때 강조하거나 일반 텍스트 표시
        skillCountText.text = $"{count} / 3";

        // 만약 선택 모드인데 개수가 줄어들어 조건 미달이 되면 여기서 처리 가능
    }

    private void OnClickItem()
    {
        if (_currentMode == DisplayMode.Info)
        {
            _skillPanel.Click_SkillInfo(_key);

            // 정보창을 확인했으므로 New 표시 끄기
            if (newNoti != null && newNoti.activeSelf)
            {
                newNoti.SetActive(false);
                SkillManager.Instance.MarkAsConfirmed(_key); // 매니저에도 확인 기록
            }
        }
        else
        {
            _onSelected?.Invoke(_key, this);
        }
    }

    public void SetSelectState(bool isSelected)
    {
        if (selectionVisual != null)
            selectionVisual.SetActive(isSelected);
    }

    #region Addressables & Cleanup
    private void LoadIcon(string iconAddress)
    {
        AddressableManager.Instance.LoadAsset<Sprite>(iconAddress, (sprite) =>
        {
            if (icon != null) icon.sprite = sprite;
        });
    }

    private void ReleaseIcon()
    {
        if (_cachedData != null)
            AddressableManager.Instance.ReleaseAsset(_cachedData.Skill_Icon);
    }

    private void OnDisable()
    {
        // 풀링으로 돌아갈 때 이벤트 해제
        if (SkillManager.Instance != null)
            SkillManager.Instance.OnInventoryChanged -= UpdateUI;
    }

    private void OnDestroy()
    {
        ReleaseIcon();
    }
    #endregion
}