using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoticeUIPanel : BaseUI
{
    [SerializeField] TextMeshProUGUI Title;
    [SerializeField] TextMeshProUGUI Description;
    [SerializeField] Button btn;

    private void Awake()
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            Close();
        });
    }

    public void ChangeNoticeTitle(string text)
    {
        if (Title == null)
        {
            return;
        }
        Title.text = text;
    }

    public void ChangeNoticeDescription(string text)
    {
        Description.text = text;
    }
    protected override void OnClose()
    {
        
    }

    protected override void OnOpen()
    {
        
    }
}
