using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum StageStateType
{
    Current,
    Cleared,
    Locked
}

public class StageButtonItem : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text label;

    [SerializeField] private Image _selectionBorder; // 주황 테두리 이미지

    [SerializeField] private Color _currentColor = Color.white;
    [SerializeField] private Color _clearedColor;
    [SerializeField] private Color _lockedColor;

    public int RealSectionNumber { get; private set; } // 실제 섹션 ID 저장

    private Action<int> _onClick;

    //displayNumber + realSectionNumber 분리
    public void Initialize(int displayNumber, int realSectionNumber,
        StageStateType stateType, Action<int> clickAction)
    {
        RealSectionNumber = realSectionNumber;
        _onClick = clickAction;

        label.text = displayNumber.ToString(); // UI 화면에는 1~50 표시

        switch (stateType)
        {
            case StageStateType.Current:
                _button.image.color = _currentColor;
                _button.interactable = true;
                break;

            case StageStateType.Cleared:
                _button.image.color = _clearedColor;
                _button.interactable = true;
                break;

            case StageStateType.Locked:
                _button.image.color = _lockedColor;
                _button.interactable = false;
                break;
        }

        SetSelected(false);

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => _onClick?.Invoke(RealSectionNumber));
    }

    public void SetSelected(bool selected)
    {
        if (_selectionBorder != null)
            _selectionBorder.gameObject.SetActive(selected);
    }
}