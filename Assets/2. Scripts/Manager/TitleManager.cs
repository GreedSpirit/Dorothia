using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    static private TitleManager instance;
    static public TitleManager Instance { get => instance; }

    [SerializeField] Image image;

    [Header("UI")]
    [SerializeField] private GameObject _buttonUI;       // 기존 버튼 UI
    [SerializeField] private GameObject _loadingRoot;  // 로딩 UI
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TMP_Text _progressText;

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
        if (_loadingRoot != null)
            _loadingRoot.SetActive(false);
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
        StartCoroutine(CoLoadGame());
    }

    private IEnumerator CoLoadGame()
    {
        //1. 기존 UI 숨김
        _buttonUI.SetActive(false);

        // 2. 로딩 UI 표시
        _loadingRoot.SetActive(true);

        //3. Async 로딩 시작
        AsyncOperation operation = SceneManager.LoadSceneAsync("InGameScene");
        operation.allowSceneActivation = false;

        float fakeProgress = 0f;

        while (!operation.isDone)
        {
            //0 ~ 0.9 → 0 ~ 1로 보정
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

            //부드러운 증가
            fakeProgress = Mathf.Lerp(fakeProgress, realProgress, Time.deltaTime * 5f);

            //UI 반영
            _progressBar.value = fakeProgress;
            _progressText.text = $"{(fakeProgress * 100f):0}%";

            //로딩 완료
            if (fakeProgress >= 0.99f)
            {
                yield return new WaitForSeconds(0.2f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
