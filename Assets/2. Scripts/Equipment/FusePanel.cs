using UnityEngine;

public class FusePanel : MonoBehaviour
{

    //합성을 진행합니다
    public void Fuse(Equipment mainEquipment, Equipment subEquipmentOne, Equipment subEquipmentTwo)
    {
        if (mainEquipment.equipPart != subEquipmentOne.equipPart ||
            mainEquipment.equipPart != subEquipmentTwo.equipPart ||
            subEquipmentOne.equipPart != subEquipmentTwo.equipPart)
        {
            Debug.Log("합성하기 위한 세 장비의 장착 부위가 일치하지 않습니다.");
            return;
        }

        int randomNumber = Random.Range(1, 101);
        int successNumber = CheckFuseSuccessRate(mainEquipment);
        Debug.Log($"{randomNumber} / {successNumber}");

        if (randomNumber <= successNumber)
        {
            Debug.Log("승급에 성공하였습니다! 장비의 레어도가 상승합니다.");
            int rarity = (int)mainEquipment.equipmentRarity;
            rarity += 1;
            mainEquipment.equipmentRarity = (EquipmentRarity)rarity;
        }
    }

    //합성 성공률을 확인합니다.
    public int CheckFuseSuccessRate(Equipment mainEquipment)
    {
        int successRate = 0;
        switch (mainEquipment.equipmentRarity)
        {
            case EquipmentRarity.Normal:
                successRate = 90;
                break;
            case EquipmentRarity.Uncommon:
                successRate = 50;
                break;
            case EquipmentRarity.Rare:
                successRate = 25;
                break;
            case EquipmentRarity.Legendary:
                successRate = 5;
                break;
            case EquipmentRarity.Mythic:
                successRate = 0;
                break;
        }
        return successRate;
    }
}
