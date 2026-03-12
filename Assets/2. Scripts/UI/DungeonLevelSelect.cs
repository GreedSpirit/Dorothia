using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonLevelSelect : MonoBehaviour
{
    [SerializeField] DungeonInfo _dungeonInfo;

    //던전인포에서 값을 셋팅해줌
    public int _dungeonId;
    public int _dungeonLevel;



    public void OnClick()
    {
        //클릭하면 셋팅된값 그대로 전달
        _dungeonInfo.DungeonLevelClick(_dungeonId, _dungeonLevel);
    }
}
