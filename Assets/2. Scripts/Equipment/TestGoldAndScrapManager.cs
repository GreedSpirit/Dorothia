using System.Collections.Generic;
using UnityEngine;

public class TestGoldAndScrapManager : MonoBehaviour
{
    public static TestGoldAndScrapManager Instance;

    public int testGold = 0;
    public int testScrap = 0;

    private void Awake()
    {
        //이미 인스턴스가 존재하며 그것이 자신이 아닌 경우 삭제.
        if (Instance != null && Instance != this)
            Destroy(gameObject);

        //인스턴스에 자신을 넣고, 게임 전반에서 유지되어야 하므로 파괴 방지.
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GoldCheat()
    {
        testGold = 9999999;
    }

    public void ScrapCheat()
    {
        testScrap = 999999;
    }
}
