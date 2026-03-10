using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoticeUIPanel : BaseUI
{
    [Header("경고문을 통해 알려야 할 정보 출력용 텍스트")]
    [SerializeField] TextMeshProUGUI Title;                  // 제목
    [SerializeField] TextMeshProUGUI Description;            // 내용

    [Header("단순 통보용의 경우")]
    [SerializeField] Button btn;                             // 통보 확인 후 닫는 용도

    [Header("플레이어의 의사를 확인해야 하는 경우")]
    public Button Acceptbtn;                                 // 추가 로직을 집어넣어야 함. 이건 이 창이 꺼내질 각 클래스에서 다룰 것.
    [SerializeField] Button Rejectbtn;                       // 닫기

    private void Awake()
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            Close();
        });
        Rejectbtn.onClick.RemoveAllListeners();
        Rejectbtn.onClick.AddListener(() =>
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

    public void ChangeNoticePanelLogic(bool isNotice)
    {
        Acceptbtn.gameObject.SetActive(!isNotice);
        Rejectbtn.gameObject.SetActive(!isNotice);
        btn.gameObject.SetActive(isNotice);
    }
    protected override void OnClose()
    {
        
    }

    protected override void OnOpen()
    {
        
    }
}
