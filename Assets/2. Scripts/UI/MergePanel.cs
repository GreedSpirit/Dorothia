using UnityEngine;

public class MergePanel : BaseUI
{
    [SerializeField] private GameObject scrollPanel;
    [SerializeField] private GameObject skillPanel;

    protected override void OnClose()
    {
    }

    protected override void OnOpen()
    {
        scrollPanel.SetActive(false);
        skillPanel.SetActive(false);
    }

    public void Click_ScrollMergePanel()
    {
        scrollPanel.SetActive(true);
    }
    public void Click_SkillMergePanel()
    {
        scrollPanel.SetActive(true);
    }
}
