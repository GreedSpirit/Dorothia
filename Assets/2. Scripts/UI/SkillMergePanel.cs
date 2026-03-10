using UnityEngine;

public class SkillMergePanel : MonoBehaviour
{
    [SerializeField] private GameObject mergeNotification;
    public void Click_IsCraftSkill()
    {
        mergeNotification.SetActive(true);
    }
}
