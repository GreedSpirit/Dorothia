using UnityEngine;

[CreateAssetMenu(fileName = "DungeonUIData", menuName = "Scriptable Objects/DungeonUIData")]
public class DungeonUIData : ScriptableObject
{
    public int[] Id;
    public int[] RewardId;
    public Sprite[] Image;
    public Sprite[] RewardImage;


    //스프라이트반환함수
    public Sprite GetSprite(int dungeonId)
    {
        //id순회돌면서 들어온id값이랑 같은애 찾기
        for (int i = 0; i < Id.Length; i++)
        {
            if (Id[i] == dungeonId)
            {
                //찾으면 해당 이미지 반환
                return Image[i];
            }
        }
        //일치하지않으면 널반환
        return null;
    }


    public Sprite GetRewardSprite(int dungeonId)
    {
        //id순회돌면서 들어온id값이랑 같은애 찾기
        for (int i = 0; i < RewardId.Length; i++)
        {
            if (RewardId[i] == dungeonId)
            {
                //찾으면 해당 이미지 반환
                return RewardImage[i];
            }
        }
        //일치하지않으면 널반환
        return null;
    }
}
