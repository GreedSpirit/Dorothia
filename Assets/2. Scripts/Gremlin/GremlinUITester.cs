using System.Collections.Generic;
using UnityEngine;

public class GremlinUITester : MonoBehaviour
{
    public Sprite dummyIconNormal;
    public Sprite dummyIconRare;
    public Sprite dummyIconLegendary;

    [SerializeField] private GremlinUIPanel _gremlinUIPanel;

    private List<GremlinItemData> _dummyGremlins;

    private void Start()
    {
        GenerateDummyData();
        
        if (_gremlinUIPanel != null)
        {
            // UI 패널 열기 및 더미 데이터 전달
            _gremlinUIPanel.OpenPanel(_dummyGremlins);
        }
        else
        {
            Debug.LogError("[GremlinUITester] _gremlinUIPanel 필요");
        }
    }

    private void GenerateDummyData()
    {
        _dummyGremlins = new List<GremlinItemData>();

        // 장착 중인 그렘린 (빨간 테두리 테스트용)
        _dummyGremlins.Add(new GremlinItemData
        {
            id = 1,
            gremlinName = "녹슨 톱니바퀴",
            currentLevel = 5,
            currentStat = 12.5f,
            tier = Rarity.Normal,
            iconSprite = dummyIconNormal,
            isEquipped = true 
        });

        _dummyGremlins.Add(new GremlinItemData
        {
            id = 2,
            gremlinName = "황금 렌치 포드",
            currentLevel = 25,
            currentStat = 350.0f,
            tier = Rarity.Rare,
            iconSprite = dummyIconRare,
            isEquipped = false
        });

        for (int i = 3; i <= 14; i++)
        {
            _dummyGremlins.Add(new GremlinItemData
            {
                id = i,
                gremlinName = $"실험용 그렘린 MK-{i}",
                currentLevel = Random.Range(1, 50),
                currentStat = Random.Range(10f, 1000f),
                tier = (Rarity)Random.Range(1, 4), // Enum 인덱스 랜덤
                iconSprite = dummyIconLegendary, // 테스트용 통일
                isEquipped = false
            });
        }
        
        Debug.Log($"[GremlinUITester] 총 {_dummyGremlins.Count}개의 더미 데이터가 생성되었습니다.");
    }
}