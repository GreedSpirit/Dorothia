using UnityEngine;

/// <summary>
/// 런타임 오브젝트 정리
/// 몬스터, 투사체, 오브 등
/// </summary>
public class RuntimeRootManager : MonoBehaviour
{
    public static Transform Monsters { get; private set; }
    public static Transform Projectiles { get; private set; }
    public static Transform Orbs { get; private set; }
    public static Transform Maps { get; private set; }
    public static Transform Slots { get; private set; }

    private void Awake()
    {
        //중복 생성 방지
        if (Monsters == null)
            Monsters = CreateRoot("Monsters");          // 몬스터

        if (Projectiles == null)
            Projectiles = CreateRoot("Projectiles");    // 투사체

        if (Orbs == null)
            Orbs = CreateRoot("Orbs");                  // 오버드라이브 오브

        if (Slots == null)                              // 전투슬롯
            Slots = CreateRoot("Slots");
    }

    private Transform CreateRoot(string name)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(transform);
        return gameObject.transform;
    }
}
