using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingText : MonoBehaviour
{
    private TextMeshPro _textMesh;
    private System.Action<GameObject> _returnAction;
    private Coroutine _animationCoroutine;

    private void Awake()
    {
        _textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(string text, Color color, float size, System.Action<GameObject> returnAction)
    {
        _textMesh.text = text;
        _textMesh.color = color;
        _textMesh.fontSize = size;
        _returnAction = returnAction;

        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        _animationCoroutine = StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        float duration = 0.6f;     
        float timer = 0f;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * 0.5f;

        Color startColor = _textMesh.color;
        // 시작 시 알파값 초기화
        startColor.a = 1f; 

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // 위치 이동 (위로 솟구침)
            // 서서히 느려지는 느낌을 위해 거듭제곱(Ease Out) 적용
            float curve = 1f - Mathf.Pow(1f - progress, 3f);
            transform.position = Vector3.Lerp(startPos, targetPos, curve);

            if (progress > 0.5f)
            {
                float alphaProgress = (progress - 0.5f) / 0.5f;
                startColor.a = Mathf.Lerp(1f, 0f, alphaProgress);
                _textMesh.color = startColor;
            }

            yield return null;
        }

        // 완료 시 풀로 반환
        _animationCoroutine = null;
        _returnAction?.Invoke(gameObject);
    }

    //private void LateUpdate()
    //{
    //    if (Camera.main != null)
    //    {
    //        transform.forward = Camera.main.transform.forward;
    //    }
    //}
}