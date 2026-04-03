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
    [SerializeField] CanvasGroup font;

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
        StartCoroutine(StartButtonFadeInOut());
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
        _buttonUI.SetActive(false); // 기존 UI 숨김
        _loadingRoot.SetActive(true); // 로딩 UI 표시

        //Async 로딩 시작
        AsyncOperation operation = SceneManager.LoadSceneAsync("InGameScene");
        operation.allowSceneActivation = false;

        float fakeProgress = 0f;

        while (fakeProgress < 1f)
        {
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

            //랜덤 멈춤 (2% 확률)
            if (Random.value < 0.02f)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            }

            //구간별 속도 + 랜덤
            float speed = GetRandomSpeed(fakeProgress);
            fakeProgress += Time.deltaTime * speed;

            //가짜 점프 (5% 확률)
            if (Random.value < 0.05f)
            {
                fakeProgress += Random.Range(0.01f, 0.03f);
            }

            fakeProgress = Mathf.Clamp01(fakeProgress);
            //실제보다 앞서가지 않게 제한
            fakeProgress = Mathf.Min(fakeProgress, realProgress);

            _progressBar.value = fakeProgress;
            int percent = Mathf.Clamp(Mathf.RoundToInt(fakeProgress * 100f), 0, 100);
            _progressText.text = $"{percent}%";

            //완료 조건
            if (realProgress >= 1f && fakeProgress >= 0.99f)
                break;

            yield return null;
        }

        // 마지막 연출
        while (fakeProgress < 1f)
        {
            fakeProgress += Time.deltaTime * 1.2f;
            fakeProgress = Mathf.Clamp01(fakeProgress);

            int percent = Mathf.Clamp(Mathf.RoundToInt(fakeProgress * 100f), 0, 100);
            _progressText.text = $"{percent}%";

            yield return null;
        }

        operation.allowSceneActivation = true;
    }

    private float GetRandomSpeed(float progress)
    {
        //초반: 빠르게
        if (progress < 0.3f)
            return Random.Range(1.2f, 2.0f);

        //중반: 들쭉날쭉
        if (progress < 0.8f)
            return Random.Range(0.3f, 1.2f);

        //후반: 느리게
        return Random.Range(0.1f, 0.4f);
    }


    private IEnumerator StartButtonFadeInOut()
    {
        //페이드 속도
        float speed = 0.8f;
        bool fadingIn = true;

        font.alpha = 0f;

        while (true)
        {
            if (fadingIn)
            {
                font.alpha += Time.deltaTime * speed;
                if (font.alpha >= 1f)
                {
                    font.alpha = 1f;
                    fadingIn = false;
                }
            }
            else
            {
                font.alpha -= Time.deltaTime * speed;
                if (font.alpha <= 0.2f)
                {
                    font.alpha = 0.2f;
                    fadingIn = true;
                }
            }

            yield return null;
        }
    }
}
