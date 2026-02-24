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

    private Stack<BasePanel> popupStack = new Stack<BasePanel>();

    public void OpenPanel(BasePanel panel)
    {
        if (panel == null) return;

        if (panel.IsOpen) return;

        panel.Open();
        popupStack.Push(panel);
    }

    public void CloseTopPanel()
    {
        if (popupStack.Count > 0)
        {
            var top = popupStack.Pop();
            top.Close();
        }
    }
}