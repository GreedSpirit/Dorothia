using UnityEngine;
using UnityEngine.UI;

public abstract class BaseUI: MonoBehaviour
{
    public bool IsOpen { get; private set; }

    public virtual void Open()
    {
        gameObject.SetActive(true);
        IsOpen = true;

        OnOpen();
    }

    // 모든 UI가 닫힐 때 공통적으로 실행할 로직
    public virtual void Close()
    {
        OnClose();

        gameObject.SetActive(false);
        IsOpen = false;
    }

    protected abstract void OnOpen();
    protected abstract void OnClose();
}