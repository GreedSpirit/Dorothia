using UnityEngine;

public enum Rarity { Normal = 1, Uncommon, Rare, Legendary, Mythtic }
//인스펙터에 들어가는 클래스가 아닌, 규칙을 위한 클래스이므로 MonoBehaviour을 사용하지 않습니다.
public static class RarityColor
{
    private static Color _normalColor = new Color32(188,188,188,255);
    private static Color _uncommonColor = new Color32(0,181,0,255);
    private static Color _rareColor = new Color32(39,107,188,255);
    private static Color _legendaryColor = new Color32(255,104,0,255);
    private static Color _mythicColor = new Color32(255,0,0,255);

    /// <summary>
    /// 레어도를 입력받았을 때, 해당 레어도에 맞는 색상을 반환하는 메서드입니다.
    /// </summary>
    /// <param name="rarity">장비의 레어도</param>
    /// <returns>해당 레어도에 맞는 색상</returns>
    public static Color GetColor(Rarity rarity)
    {
        switch(rarity)
        {
            case Rarity.Normal:
                return _normalColor;

            case Rarity.Uncommon:
                return _uncommonColor;

            case Rarity.Rare:
                return _rareColor;

            case Rarity.Legendary:
                return _legendaryColor;

            case Rarity.Mythtic:
                return _mythicColor;

            default:
                return Color.white;
        }
    }
}
