using UnityEngine;
using UnityEngine.Pool;
using TMPro;
using GameUtility;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance;

    [SerializeField] private GameObject _textPrefab; // TMP가 붙은 프리팹
    private IObjectPool<FloatingText> _pool;

    private Camera _mainCamera;

    private void Awake()
    {
        Instance = this;
        _mainCamera = Camera.main;

        _pool = new ObjectPool<FloatingText>(
            createFunc: () =>
            {
                var obj = Instantiate(_textPrefab, transform);
                return obj.GetComponent<FloatingText>(); // 생성 시 한 번만
            },
            actionOnGet: ft => ft.gameObject.SetActive(true),
            actionOnRelease: ft => ft.gameObject.SetActive(false),
            actionOnDestroy: ft => Destroy(ft.gameObject),
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
        FloatingText ft = _pool.Get(); 
        ft.transform.position = worldPos + Vector3.up * 1.5f;
        ft.transform.forward = _mainCamera.transform.forward;

        Color color = isCritical ? Color.red : Color.white;
        float size = isCritical ? 6f : 4f;
        string formatted = NumberFormatterBigInt.Format(new System.Numerics.BigInteger(damage));
        ft.Setup(formatted, color, size, _ => _pool.Release(ft));
    }
}