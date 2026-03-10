using UnityEngine;

public class ScrollMergePanel : MonoBehaviour
{
    [SerializeField] private GameObject mergeNotification;

    public void Click_IsCraftSkill()
    {
        mergeNotification.SetActive(true);
    }
}
