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

    public void GameStart()
    {
        SceneManager.LoadScene("InGameScene");
    }

}
