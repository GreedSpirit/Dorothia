using UnityEngine;

public class SimpleProjectile : MonoBehaviour
{
    private Vector3 _direction;
    private float _speed;
    private int _damage;
    private float _lifetime;

    private float _spawnTime;

    private MonsterSpawnManager _owner;
    private GameObject _poolKey;

    public void Initialize(
        Vector3 direction,
        float speed,
        int damage,
        float lifeTime,
        MonsterSpawnManager owner,
        GameObject poolKey)
    {
        _direction = direction;
        _speed = speed;
        _damage = damage;
        _lifetime = lifeTime;
        _spawnTime = Time.time;

        _owner = owner;
        _poolKey = poolKey;
    }

    private void Update()
    {
        transform.position += _direction * (_speed * Time.deltaTime);

        if (Time.time - _spawnTime >= _lifetime)
        {
            Release();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IMonsterTarget>(out var target))
        {
            target.ApplyDamage(_damage);
        }

        Release();
    }

    private void Release()
    {
        _owner.ReleaseProjectile(this, _poolKey);
    }
}
