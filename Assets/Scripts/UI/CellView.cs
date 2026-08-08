using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Infrastructure.Events;
using Infrastructure;
using Gameplay;

public class CellView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Coordinates")]
    [SerializeField] private int _row;
    [SerializeField] private int _column;

    [Header("Visuals")]
    [SerializeField] private Image _background;
    [SerializeField] private Image _highlightImage;
    [SerializeField] private Animator _cellAnimator;

    private Cell _logicCell;
    private BoardView _boardView;
    private bool _servicesReady;

    public int Row => _row;
    public int Column => _column;
    public int X => _column;
    public int Y => _row;

    private void Awake()
    {
        if (_highlightImage != null)
            _highlightImage.gameObject.SetActive(false);
    }

    private void Start()
    {
        Debug.Log("CellView Services Initialized");
        _servicesReady = true;
        _boardView = ServiceLocator.TryGet<BoardView>(out var bv) ? bv : null;
        if (_boardView == null)
            Debug.LogError("CellView: BoardView not found!");
    }


    private void OnDestroy()
    {
    }

    public void Initialize(Cell logicCell)
    {
        _logicCell = logicCell;
    }

    public void SetState(CellState state)
    {
        Debug.Log("Cellview SetState");
        if (_cellAnimator != null)
        {
            _cellAnimator.SetInteger("State", (int)state);
        }
        else if (_highlightImage != null)
        {
            switch (state)
            {
                case CellState.Highlighted:
                    Debug.Log("Cellview SetState highlited");
                    _highlightImage.gameObject.SetActive(true);
                    _highlightImage.color = Color.green;
                    break;
                case CellState.Unavailable:
                    _highlightImage.gameObject.SetActive(true);
                    _highlightImage.color = Color.red;
                    break;
                case CellState.Occupied:
                    _highlightImage.gameObject.SetActive(true);
                    _highlightImage.color = Color.yellow;
                    break;
                default:
                    _highlightImage.gameObject.SetActive(false);
                    break;
            }
        }
    }

    // ---------- IPointer Handlers ----------
    public void OnPointerDown(PointerEventData eventData)
    {
        _boardView?.OnCellPointerDown(_column, _row, eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _boardView?.OnCellPointerUp(_column, _row, eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _boardView?.OnCellPointerEnter(_column, _row);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _boardView?.OnCellPointerExit(_column, _row);
    }
}

public enum CellState
{
    Default,
    Highlighted,
    Occupied,
    Unavailable
}