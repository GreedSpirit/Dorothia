using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    static private TitleManager instance;
    static public TitleManager Instance { get => instance; }

    [SerializeField] Image image;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

    }

    private void Start()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(0.5f);

        for(int i = 0; image.color.a > 0; i++)
        {
            FadeOutEffect();
            yield return new WaitForSeconds(0.015f);
        }
    }

    private void FadeOutEffect()
    {
        Color imageColor = image.color;
        imageColor = new Color(imageColor.r, imageColor.g, imageColor.b, imageColor.a - 0.05f);
        image.color = imageColor;
        if(imageColor.a >= 0.04f)
        {
            image.raycastTarget = false;
        }
    }

    public void GameStart()
    {
        SceneManager.LoadScene("InGameScene");
    }

}
