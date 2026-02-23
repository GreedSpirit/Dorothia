using UnityEngine;

public class HitBox : MonoBehaviour
{    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out TestEnemy enemy))
        {
            enemy.TakeDamage(10);
        }
    }
}
