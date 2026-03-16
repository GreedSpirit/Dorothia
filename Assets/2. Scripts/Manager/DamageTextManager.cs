using UnityEngine;
using UnityEngine.Pool;
using TMPro;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance;

    [SerializeField] private GameObject _textPrefab; // TMP가 붙은 프리팹
    private IObjectPool<GameObject> _pool;

    private void Awake()
    {
        Instance = this;

        // 유니티 내장 오브젝트 풀 초기화
        _pool = new ObjectPool<GameObject>(
            createFunc: CreateText,
            actionOnGet: OnGetText,
            actionOnRelease: OnReleaseText,
            actionOnDestroy: OnDestroyText,
            defaultCapacity: 20,
            maxSize: 50
        );
    }

    private GameObject CreateText()
    {
        GameObject obj = Instantiate(_textPrefab, transform);
        return obj;
    }

    private void OnGetText(GameObject obj) => obj.SetActive(true);
    private void OnReleaseText(GameObject obj) => obj.SetActive(false);
    private void OnDestroyText(GameObject obj) => Destroy(obj);

    public void ShowDamage(int damage, Vector3 worldPos, bool isCritical = false)
    {
        GameObject obj = _pool.Get();
        obj.transform.position = worldPos + Vector3.up * 1.5f; // 몬스터 머리 위쯤
        transform.LookAt(Camera.main.transform);
        transform.forward = Camera.main.transform.forward;

        FloatingText ft = obj.GetComponent<FloatingText>();

        // 크리티컬 여부에 따른 설정
        Color color = isCritical ? Color.red : Color.white;
        float size = isCritical ? 6f : 4f;

        ft.Setup(damage.ToString(), color, size, (target) => _pool.Release(target));
    }
}