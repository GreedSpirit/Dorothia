using UnityEngine;

public class GremlinRarityModelingManager : MonoBehaviour
{
    [SerializeField] GameObject _normalJetpack;           // 일반 등급 제트팩
    [SerializeField] GameObject _uncommonJetpack;         // 희귀 등급 제트팩
    [SerializeField] GameObject _legendaryJetpack;        // 전설 등급 제트팩
    [SerializeField] GameObject _normalWeapon;            // 일반 등급 무기
    [SerializeField] GameObject _rareWeapon;              // 레어 등급 무기
    [SerializeField] GameObject _mythticWeapon;           // 신화 등급 무기

    /// <summary>
    /// 모델의 등급에 따라 활성화하는 오브젝트를 정합니다.
    /// </summary>
    /// <param name="rarity">등급</param>
    public void ChangeModel(Rarity rarity)
    {
        switch(rarity)
        {
            //해당 등급 - 1까지만 활성화. 단, 가장 높은 것만 사용.
            case Rarity.Normal:
                _normalJetpack.SetActive(true);
                _normalWeapon.SetActive(true);
                _uncommonJetpack.SetActive(false);
                _rareWeapon.SetActive(false);
                _legendaryJetpack.SetActive(false);
                _mythticWeapon.SetActive(false);
                break;
            case Rarity.Uncommon:
                _normalJetpack.SetActive(false);
                _normalWeapon.SetActive(true);
                _uncommonJetpack.SetActive(true);
                _rareWeapon.SetActive(false);
                _legendaryJetpack.SetActive(false);
                _mythticWeapon.SetActive(false);
                break;
            case Rarity.Rare:
                _normalJetpack.SetActive(false);
                _normalWeapon.SetActive(false);
                _uncommonJetpack.SetActive(true);
                _rareWeapon.SetActive(true);
                _legendaryJetpack.SetActive(false);
                _mythticWeapon.SetActive(false);
                break;
            case Rarity.Legendary:
                _normalJetpack.SetActive(false);
                _normalWeapon.SetActive(false);
                _uncommonJetpack.SetActive(false);
                _rareWeapon.SetActive(true);
                _legendaryJetpack.SetActive(true);
                _mythticWeapon.SetActive(false);
                break;
            case Rarity.Mythtic:
                _normalJetpack.SetActive(false);
                _normalWeapon.SetActive(false);
                _uncommonJetpack.SetActive(false);
                _rareWeapon.SetActive(false);
                _legendaryJetpack.SetActive(true);
                _mythticWeapon.SetActive(true);
                break;
        }
    }
}
