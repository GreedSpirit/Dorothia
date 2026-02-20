using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    public int hp = 100;
    public bool isdead;

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.LogWarning($"적 hp = {hp}");
        if (hp <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
