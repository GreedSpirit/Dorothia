using System.Numerics;
using UnityEngine;

public class CheatManager : MonoBehaviour
{
    [SerializeField] private PlayerStats _playerStats;

    //버튼에서 호출
    public void AddExpCheat()
    {
        BigInteger cheatExp = new BigInteger(1000000); // 100만 exp
        _playerStats.AddExp(cheatExp);

        Debug.Log($"[Cheat] Exp 지급: {cheatExp}");
    }
}
