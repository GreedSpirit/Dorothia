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

    private bool _isReleased;

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

        _isReleased = false;
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
        if (_isReleased)
            return;

        _isReleased = true;

        if (_owner != null && _poolKey != null)
        {
            _owner.ReleaseProjectile(this, _poolKey);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
