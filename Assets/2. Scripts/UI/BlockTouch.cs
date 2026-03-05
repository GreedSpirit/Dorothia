using UnityEngine;

public class BlockTouch : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);

        RefreshNotchArea();
    }

    void Update()
    {
        if (lastSafeArea != Screen.safeArea)
        {
            RefreshNotchArea();
        }
    }

    void RefreshNotchArea()
    {
        Rect safeArea = Screen.safeArea;

        float widthRatio = rectTransform.root.GetComponent<RectTransform>().rect.width / Screen.width;
        float heightRatio = rectTransform.root.GetComponent<RectTransform>().rect.height / Screen.height;

        float newWidth = Screen.width * widthRatio;

        // 노치 높이 계산: (화면 전체 높이 - 세이프 에어리어의 상단 좌표값)
        float notchHeight = (Screen.height - safeArea.yMax) * heightRatio;

        rectTransform.sizeDelta = new Vector2(newWidth, notchHeight);

        // 위치 고정 (상단 앵커이므로 0,0이면 화면 맨 끝에 붙음)
        rectTransform.anchoredPosition = Vector2.zero;

        lastSafeArea = safeArea;
    }
}