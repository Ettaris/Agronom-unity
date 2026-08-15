using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using Gameplay;

public class BoardCellView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{

    //TODO: Delete this script.
    public enum CellState
    {
        Default,
        Highlighted, // подсвечена (возможна посадка)
        Occupied,    // занята (растение)
        Unavailable  // недоступна
    }

    [Header("Visuals")]
    [SerializeField] private UnityEngine.UI.Image _backgroundImage;
    [SerializeField] private UnityEngine.UI.Image _plantImage; // изображение растения (спрайт)
    [SerializeField] private TMP_Text _growthText; // процент роста или геном
    [SerializeField] private GameObject _highlightObject; // объект подсветки

    [Header("Animator")]
    [SerializeField] private Animator _cellAnimator;

    private int _x, _y;
    private BoardView _boardView;
    private PlantInstance _currentPlant;
    private CellState _currentState = CellState.Default;

    private MutationView _mutationView;

    public int X => _x;
    public int Y => _y;

    private void Awake()
    {
        _mutationView = GetComponent<MutationView>();
        if (_mutationView == null)
        {
            _mutationView = gameObject.AddComponent<MutationView>();
        }
    }

    public void Initialize(int x, int y, BoardView boardView)
    {
        _x = x;
        _y = y;
        _boardView = boardView;
        SetState(CellState.Default);
        _plantImage.gameObject.SetActive(false);
        _growthText.text = "";
        _highlightObject.SetActive(false);
    }

    public void SetPlant(PlantInstance plant)
    {
        _currentPlant = plant;
        if (plant != null)
        {
            _mutationView.Initialize(plant);
            _plantImage.gameObject.SetActive(true); // гарантия
            UpdateGrowthText();
        }
        else
        {
            _plantImage.sprite = null;
            _plantImage.gameObject.SetActive(false);
            _growthText.text = "";
        }
        _mutationView.Initialize(plant);
    }

    public void SetState(CellState state)
    {
        _currentState = state;
        _cellAnimator.SetInteger("State", (int)state);
        // Подсветка TODO: убрать ненужный хайлайты
        switch (state)
        {
            case CellState.Highlighted:
                _highlightObject.SetActive(true);
                // цвет зелёный
                _highlightObject.GetComponent<UnityEngine.UI.Image>().color = Color.darkOliveGreen;
                break;
            case CellState.Occupied:
                //_highlightObject.SetActive(true);
                //_highlightObject.GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 0f);
                break;
            case CellState.Unavailable:
                //_highlightObject.SetActive(true);
                //_highlightObject.GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 0f, 0f);
                break;
            default:
                _highlightObject.SetActive(false);
                break;
        }
    }

    public void PlayHarvestAnimation()
    {
        // Анимация сбора: исчезновение, частицы (можно через DOTween)
        _plantImage.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
        {
            _plantImage.gameObject.SetActive(false);
        });
        // Также можно сделать вспышку или движение вверх
        transform.DOPunchScale(Vector3.one * 0.2f, 0.15f);
    }

    private void UpdateGrowthText()
    {
        if (_currentPlant != null)
        {
            int percent = Mathf.RoundToInt(_currentPlant.GrowthProgress * 100f);
            _growthText.text = percent + "%";
        }
    }

    // Обработка ввода (передаём в BoardView)
    public void OnPointerDown(PointerEventData eventData)
    {
        _boardView.OnCellPointerDown(_x, _y, eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _boardView.OnCellPointerUp(_x, _y, eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.dragging) return;
        _boardView.OnCellPointerEnter(_x, _y);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.dragging) return;
        _boardView.OnCellPointerExit(_x, _y);
    }

    // Обновление роста (может вызываться по событию)
    public void RefreshGrowth()
    {
        if (_currentPlant != null)
            UpdateGrowthText();
    }
}