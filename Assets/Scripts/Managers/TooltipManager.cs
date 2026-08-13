using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Data;
using Infrastructure;

public class TooltipManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private Image _iconImage;

    [Header("Settings")]
    [SerializeField] private float _fadeDuration = 0.15f;
    [SerializeField] private Vector2 _offset = new Vector2(15, -15);

    private static TooltipManager _instance;
    private bool _isVisible;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
        _isVisible = false;
    }

    public static void Show(GenomePropertyData data, Vector2 position)
    {
        if (_instance == null)
        {
            Debug.LogError("TooltipManager not found!");
            return;
        }
        _instance.ShowInternal(data, position);
    }

    public static void Hide()
    {
        if (_instance == null) return;
        _instance.HideInternal();
    }

    private void ShowInternal(GenomePropertyData data, Vector2 position)
    {
        if (data == null)
        {
            HideInternal();
            return;
        }

        _titleText.text = data.propertyName;
        _descriptionText.text = data.description;
        _costText.text = $"Цена: {data.genomeCost}";
        if (_iconImage != null) _iconImage.sprite = data.icon;

        // Позиционирование с учётом смещения
        Vector2 finalPos = position + _offset;
        // Ограничиваем позицию, чтобы тултип не выходил за экран
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 size = rect.rect.size;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        finalPos.x = Mathf.Clamp(finalPos.x, size.x / 2, screenSize.x - size.x / 2);
        finalPos.y = Mathf.Clamp(finalPos.y, size.y / 2, screenSize.y - size.y / 2);
        rect.position = finalPos;

        if (!_isVisible)
        {
            _isVisible = true;
            _canvasGroup.DOFade(1, _fadeDuration);
        }
        else
        {
            _canvasGroup.alpha = 1;
        }
    }

    private void HideInternal()
    {
        if (!_isVisible) return;
        _isVisible = false;
        _canvasGroup.DOFade(0, _fadeDuration).OnComplete(() =>
        {
            _canvasGroup.alpha = 0;
        });
    }

    public static void UpdatePosition(Vector2 position)
    {
        if (_instance == null || !_instance._isVisible) return;
        Vector2 finalPos = position + _instance._offset;
        RectTransform rect = _instance.GetComponent<RectTransform>();
        Vector2 size = rect.rect.size;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        finalPos.x = Mathf.Clamp(finalPos.x, size.x / 2, screenSize.x - size.x / 2);
        finalPos.y = Mathf.Clamp(finalPos.y, size.y / 2, screenSize.y - size.y / 2);
        rect.position = finalPos;
    }
}