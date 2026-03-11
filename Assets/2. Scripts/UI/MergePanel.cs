using UnityEngine;

public class MergePanel : BaseUI
{
    [SerializeField] private ScrollMergePanel scrollPanel;
    [SerializeField] private SkillMergePanel skillPanel;

    protected override void OnClose()
    {
    }

    protected override void OnOpen()
    {
        scrollPanel.gameObject.SetActive(false);
        skillPanel.gameObject.SetActive(false);
    }

    public void Click_ScrollMergePanel()
    {
        UIManager.Instance.OpenPanel(scrollPanel);
    }
    public void Click_SkillMergePanel()
    {
        UIManager.Instance.OpenPanel(skillPanel);
    }
}
