using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardPopup : BaseUI
{
    [SerializeField] Image _icon;
    [SerializeField] TextMeshProUGUI _name;
    [SerializeField] TextMeshProUGUI _message;
    [SerializeField] Button _backgroundbtn;

    protected override void OnClose()
    {
        _backgroundbtn.interactable = true;
    }

    protected override void OnOpen()
    {
        _backgroundbtn.interactable = false;
    }


    public void SetInfo(Sprite image, string name, string message)
    {
        _icon.sprite = image;
        _name.text = name;
        _message.text = message;
    }
}
