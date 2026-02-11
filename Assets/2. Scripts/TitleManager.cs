using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    static private TitleManager instance;
    static public TitleManager Instance { get => instance; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    [SerializeField] private OptionPopup optionPopup;
    public void OnClickOptionPopup()
    {
        optionPopup.OpenPopup();    
    }
}
