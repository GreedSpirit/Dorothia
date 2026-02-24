using UnityEngine;
using System.Collections;

public abstract class BasePanel : MonoBehaviour
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
        gameObject.SetActive(false);
        IsOpen = false;

        OnClose();
    }

    protected abstract void OnOpen();
    protected abstract void OnClose();
}