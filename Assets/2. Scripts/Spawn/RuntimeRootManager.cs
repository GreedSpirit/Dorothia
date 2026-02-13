using UnityEngine;

/// <summary>
/// 런타임 오브젝트 정리
/// 몬스터, 투사체, 이펙트 등
/// </summary>
public class RuntimeRootManager : MonoBehaviour
{
    public static Transform Monsters { get; private set; }
    public static Transform Projectiles { get; private set; }
    //추후 이펙트 파티클 추가가능

    private void Awake()
    {
        //중복 생성 방지
        if (Monsters == null)
            Monsters = CreateRoot("Monsters");          // 몬스터

        if (Projectiles == null)
            Projectiles = CreateRoot("Projectiles");    // 투사체

    }

    private Transform CreateRoot(string name)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(transform);
        return gameObject.transform;
    }
}
