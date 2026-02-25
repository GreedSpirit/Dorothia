using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private Stack<BaseUI> uiStack = new Stack<BaseUI>();

    public void OpenPanel(BaseUI baseUI)
    {
        if (baseUI == null) return;

        if (baseUI.IsOpen) return;

        baseUI.Open();
        uiStack.Push(baseUI);
    }

    public void CloseTopPanel()
    {
        if (uiStack.Count > 0)
        {
            var top = uiStack.Pop();
            top.Close();
        }
    }
}