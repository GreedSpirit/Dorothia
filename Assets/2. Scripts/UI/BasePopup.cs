using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class BasePopup : MonoBehaviour
{
    [SerializeField] private Button backgroundBtn;
    [SerializeField] private Button closeBtn;

    private void Awake()
    {
        CheckComponentsByReflection();
    }

    private void Start()
    {
        backgroundBtn.onClick.AddListener(ClosePopup);
        closeBtn.onClick.AddListener(ClosePopup);
    }

    private void ClosePopup() => gameObject.SetActive(false);
    public void OpenPopup() => gameObject.SetActive(true);

    private void CheckComponentsByReflection()
    {
        //현재 클래스에 선언된 모든 필드 정보
        //Public, NonPublic(Private), Instance 필드들을 모두 포함
        FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            //[SerializeField] 어트리뷰트가 붙어있는 필드만 검사
            if (field.GetCustomAttribute<SerializeField>() != null)
            {
                //필드의 실제 값
                object value = field.GetValue(this);

                //Unity 오브젝트는 일반 null 체크보다 == null 체크 (Missing Reference 대응)
                if (value == null || value.Equals(null))
                {
                    Debug.LogError($"<color=red><b>[Missing Reference]</b></color> {gameObject.name}의 <b>{field.Name}</b> 필드가 할당되지 않았습니다.");
                }
            }
        }
    }
}
