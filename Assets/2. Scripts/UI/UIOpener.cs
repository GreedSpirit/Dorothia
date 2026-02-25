using UnityEngine;
using UnityEngine.UI;

public class UIOpener : MonoBehaviour
{
    [SerializeField] private BaseUI targetPanel;

    void Awake()
    {
        var btn = GetComponent<Button>();

        btn.onClick.AddListener(() => UIManager.Instance.OpenPanel(targetPanel));

    }
}