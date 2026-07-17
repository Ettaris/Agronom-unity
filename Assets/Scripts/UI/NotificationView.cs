using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class NotificationView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Animator _animator;

    private float _duration;
    private Sequence _sequence;

    public bool IsActive { get; private set; }

    private void Awake()
    {
        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Настройка уведомления и его показ.
    /// </summary>
    public void Show(NotificationData data)
    {
        if (data.Icon != null)
        {
            _iconImage.sprite = data.Icon;
            _iconImage.gameObject.SetActive(true);
        }
        else
        {
            _iconImage.gameObject.SetActive(false);
        }

        _messageText.text = data.Message;
        _messageText.color = data.Color;
        _duration = data.Duration;

        IsActive = true;

        // Анимация появления через Animator
        _animator.SetTrigger("Show");

        // Планируем скрытие через Duration
        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.AppendInterval(_duration);
        _sequence.AppendCallback(() => Hide());
        _sequence.Play();
    }

    /// <summary>
    /// Скрывает уведомление с анимацией.
    /// </summary>
    public void Hide()
    {
        if (!IsActive) return;
        IsActive = false;

        _animator.SetTrigger("Hide");
        _sequence?.Kill();

        // Отключаем объект после завершения анимации
        DOVirtual.DelayedCall(0.5f, () => gameObject.SetActive(false));
    }

    /// <summary>
    /// Немедленное скрытие без анимации (для очистки).
    /// </summary>
    public void ForceHide()
    {
        _sequence?.Kill();
        _canvasGroup.alpha = 0;
        gameObject.SetActive(false);
        IsActive = false;
    }
}