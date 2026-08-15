using UnityEngine;
using TMPro;
using DG.Tweening;


public class FloatingText : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private CanvasGroup _canvasGroup;

    private RectTransform _rectTransform;
    private Tween _moveTween;
    private Tween _fadeTween;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(string text, Color color, Vector2 screenPos, float floatHeight = 60f, float duration = 1f)
    {
        _text.text = text;
        _text.color = color;
        _rectTransform.position = screenPos + Vector2.up * floatHeight * 0.5f;

        // Сброс состояния
        _canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;

        Vector3 targetPos = (Vector3)(screenPos + Vector2.up * floatHeight);
        _moveTween = transform.DOMove(targetPos, duration).SetEase(Ease.OutQuad);
        _fadeTween = _canvasGroup.DOFade(0f, duration).OnComplete(() =>
        {
            FloatingTextPool.Instance?.ReturnToPool(this);
        });
    }

    private void OnDestroy()
    {
        _moveTween?.Kill();
        _fadeTween?.Kill();
    }
}