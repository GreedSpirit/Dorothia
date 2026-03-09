using System.Collections.Generic;
using UnityEngine;

public class GremlinUITester : MonoBehaviour
{
    public Sprite dummyIconNormal;
    public Sprite dummyIconRare;
    public Sprite dummyIconLegendary;

    [SerializeField] private GremlinUIPanel _gremlinUIPanel;
    [SerializeField] GremlinInventory _gremlinList;

    private List<GremlinItemData> _dummyGremlins;

    private void Start()
    {
        //더미데이터 생성
        GenerateDummyData();
        
        //그렘린 UI 패널이 존재할 경우
        if (_gremlinUIPanel != null)
        {
            // UI 패널 열기 및 더미 데이터 전달
            //_gremlinUIPanel.OpenPanel(_gremlinList._gremlinInventory);
        }
        else
        {
            Debug.LogError("[GremlinUITester] _gremlinUIPanel 필요");
        }
    }

    //더미데이터 생성
    private void GenerateDummyData()
    {
        //그렘린아이템데이터의 집합체
        //_dummyGremlins = _gremlinList._gremlinInventory;

        // 장착 중인 그렘린 (빨간 테두리 테스트용)
        //_gremlinList.AddGremlin(new GremlinItemData
        //{
        //    id = 1,                               // 아이디값
        //    gremlinName = "녹슨 톱니바퀴",         // 이름
        //    currentLevel = 5,                     // 현재레벨
        //    currentStat = 12.5f,                  // 현재스텟
        //    tier = Rarity.Normal,                 // 현재등급
        //    iconSprite = dummyIconNormal,         // 아이콘스프라이트
        //    isEquipped = true                     // 장착여부
        //});
        //
        //_gremlinList.AddGremlin(new GremlinItemData
        //{
        //    id = 2,
        //    gremlinName = "황금 렌치 포드",
        //    currentLevel = 25,
        //    currentStat = 350.0f,
        //    tier = Rarity.Rare,
        //    iconSprite = dummyIconRare,
        //    isEquipped = false
        //});
        //
        //for (int i = 3; i <= 14; i++)
        //{
        //    _gremlinList.AddGremlin(new GremlinItemData
        //    {
        //        id = i,
        //        gremlinName = $"실험용 그렘린 MK-{i}",
        //        currentLevel = Random.Range(1, 50),
        //        currentStat = Random.Range(10f, 1000f),
        //        tier = (Rarity)Random.Range(1, 4), // Enum 인덱스 랜덤
        //        iconSprite = dummyIconLegendary, // 테스트용 통일
        //        isEquipped = false
        //    });
        //}
        //
        //Debug.Log($"[GremlinUITester] 총 {_gremlinList._gremlinInventory.Count}개의 더미 데이터가 생성되었습니다.");
    }
}