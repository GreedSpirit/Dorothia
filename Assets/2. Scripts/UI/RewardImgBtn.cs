using UnityEngine;
using UnityEngine.UI;

public class RewardImgBtn : MonoBehaviour
{
    Sprite _rewardImage;
    [SerializeField] string _rewardText;
    [SerializeField] string _rewardMessage;
    [SerializeField] RewardPopup _targetPopup;

    Button _button;

    private void Awake()
    {
        _rewardImage = GetComponent<Image>().sprite;
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OpenPopup);
    }

    void OpenPopup()
    {
        UIManager.Instance.OpenPanel(_targetPopup);

        _targetPopup.SetInfo(_rewardImage, _rewardText, _rewardMessage);
    }

    
}
