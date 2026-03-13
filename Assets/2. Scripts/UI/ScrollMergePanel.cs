using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class ScrollMergePanel : BaseUI
{
    [SerializeField] private GameObject mergeNotification;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject skillItemPrefab;

    private List<SkillItem> _pool = new List<SkillItem>();

    protected override void OnOpen()
    {
        RefreshUI();
    }

    protected override void OnClose()
    {
        
    }

    private void RefreshUI()
    {
        foreach (var item in _pool)
        {
            item.gameObject.SetActive(false);
        }

        var targetSkills = SkillManager.Instance.Inventory
            .Where(x => x.Key.isScroll)
            .ToList();

        for (int i = 0; i < targetSkills.Count; i++)
        {
            SkillItem item = GetOrCreateItem(i);
            SkillKey key = targetSkills[i].Key;
            SkillData data = DataManager.Instance.GetData<SkillData>(key.sid);

            item.SetSlotData(SkillItem.SlotType.Scroll, key, -1,SkillItem.DisplayMode.Info);
            //item.set(targetSkills[i].Key, SkillItem.DisplayMode.Info);

            item.gameObject. SetActive(true);

            // 여기에 아이템의 텍스트나 아이콘을 설정하는 컴포넌트 호출 로직 추가
            // item.GetComponent<SkillItemUI>().SetData(targetSkills[i].Key, targetSkills[i].Value);
        }
    }

    private SkillItem GetOrCreateItem(int index)
    {
        // 풀에 모자라면 새로 생성
        if (index >= _pool.Count)
        {
            GameObject newObj = Instantiate(skillItemPrefab, contentParent);
            SkillItem item = newObj.GetComponent<SkillItem>();
            _pool.Add(item);
            return item;
        }
        return _pool[index];
    }

    public void Click_Merge()
    {
        mergeNotification.SetActive(true);
    }

   
}
