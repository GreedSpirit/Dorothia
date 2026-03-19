using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class GremlinUpgradePanel : BaseUI
{
    [SerializeField] Gremlin targetGremlin;
    //현재 그렘린이 들어갈 이미지
    [SerializeField] Image _currentGremlinImage;
    //그 그렘린과 일치하는 조각
    [SerializeField] Image _gremlinShardImage;
    //그 그렘린과 일치하는 각각 등급의 이미지
    [SerializeField] Image[] _gremlinRarityImage;
    //조각 및 각 등급별 소지 개수 표기용 텍스트
    [SerializeField] TextMeshProUGUI _gremlinShardText;
    [SerializeField] TextMeshProUGUI _gremlinNormalText;
    [SerializeField] TextMeshProUGUI _gremlinUncommonText;
    [SerializeField] TextMeshProUGUI _gremlinRareText;
    [SerializeField] TextMeshProUGUI _gremlinLegendaryText;
    [SerializeField] TextMeshProUGUI _gremlinMythticText;
    //합성 버튼
    [SerializeField] Button _fuseButton;

    private int 

    //열었을 때
    private void Awake()
    {

    }

    //메서드 - 합성 대상 체크
    public IEnumerator GetFuseTarget(Gremlin gremlin)
    {
        //대상 그렘린으로서 인자값으로 받은 그렘린을 받아옵니다.
        targetGremlin = gremlin;

        //그렘린 데이터에서 어드레서블의 스프라이트를 가져옵니다.
        var gSprite = Addressables.LoadAssetAsync<Sprite>(gremlin._gremlinData.PrefabName);
        yield return gSprite.Task;

        //현재 그렘린의 이미지 스프라이트를 변경합니다.
        _currentGremlinImage.sprite = gSprite.Result;

        //각 레어도별 이미지도 해당 스프라이트를 반영합니다.
        foreach(Image image in _gremlinRarityImage)
        {
            image.sprite = gSprite.Result;
        }
    }
    //메서드 - 합성
    public void FuseGremlin()
    {
        //대상이 될 그렘린이 없으면 반환합니다.
        if (targetGremlin == null) return;
        
        
    }
    //메서드 - 초기화
    protected override void OnClose()
    {
        
    }

    protected override void OnOpen()
    {
        
    }
}
