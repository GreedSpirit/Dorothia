using TMPro;
using UnityEngine;

public class TotalPowerText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI[] targetText;       // 범용성을 위한 배열

    private void Start()
    {
        PlayerStats.Instance.OnTotalPowerChanged += ChangeTargetText;
    }

    private void OnDestroy()
    {
        PlayerStats.Instance.OnTotalPowerChanged -= ChangeTargetText;
    }

    /// <summary>
    /// 대상 텍스트의 내용을 변경합니다.
    /// </summary>
    /// <param name="str">변경하고자 하는 내용</param>
    public void ChangeTargetText(string str)
    {
        if (targetText.Length <= 0) return;             // 배열의 길이가 0이라면 반환
        if (PlayerStats.Instance == null) return;       // 플레이어스탯이 존재하지 않으면 반환

        //배열 내의 모든 텍스트 대상으로 다음 코드를 실행
        foreach(var text in targetText)
        {
            //TMP 내 텍스트의 내용 변경
            text.text = ($"전투력 : {str}");
        }
    }
}
