using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    public void GameStart()
    {
        SceneManager.LoadScene("TInGameScene");
    }

}
