using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using Gameplay;

public class CellView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Coordinates")]
    [SerializeField] private int _row;
    [SerializeField] private int _column;

    [Header("Visuals")]
    [SerializeField] private Image _background;
    [SerializeField] private Image _plantImage;
    [SerializeField] private TMP_Text _growthText;
    [SerializeField] private GameObject _highlightObject;

    [Header("Animator")]
    [SerializeField] private Animator _cellAnimator;

    [Header("Mutation")]
    [SerializeField] private MutationView _mutationView;

    private Cell _logicCell;
    private PlantInstance _currentPlant;
    private BoardView _boardView;

    public int Row => _row;
    public int Column => _column;
    public int X => _column;
    public int Y => _row;
    public Cell LogicCell => _logicCell;
    public PlantInstance CurrentPlant => _currentPlant;

    private void Awake()
    {
        _boardView = FindAnyObjectByType<BoardView>();
    }

    public void Initialize(Cell logicCell)
    {
        _logicCell = logicCell;
        UpdateView();
    }

    public void SetPlant(PlantInstance plant)
    {
        Debug.Log($"CellView.SetPlant: plant={(plant?.PlantData?.itemName ?? "null")} at ({_column},{_row})");
        _currentPlant = plant;
        if (_logicCell != null)
            _logicCell.Plant = plant;

        if (_mutationView != null)
        {
            Debug.Log("CellView: Using MutationView");
            _mutationView.Initialize(plant);
            if (plant != null)
                _mutationView.Refresh();
        }
        else
        {
            Debug.Log("CellView: MutationView is null, using fallback");
            if (plant != null)
            {
                _plantImage.sprite = plant.PlantData.icon;
                _plantImage.gameObject.SetActive(true);
                _plantImage.color = Color.white;
                _plantImage.transform.localScale = Vector3.one;
                _growthText.text = Mathf.RoundToInt(plant.GrowthProgress * 100f) + "%";
            }
            else
            {
                _plantImage.sprite = null;
                _plantImage.gameObject.SetActive(false);
                _growthText.text = "";
            }
        }

        UpdateGrowthText();
    }

    private void UpdateGrowthText()
    {
        if (_currentPlant != null)
        {
            int growthPercent = Mathf.RoundToInt(_currentPlant.GrowthProgress * 100f);
            _growthText.text = growthPercent + "%";
        }
        else
        {
            _growthText.text = "";
        }
    }

    public void SetState(CellState state)
    {
        _cellAnimator?.SetInteger("State", (int)state);
    }

    public void SetHighlight(bool active, Color? color = null)
    {
        if (_highlightObject != null)
        {
            _highlightObject.SetActive(active);
            if (color.HasValue)
                _highlightObject.GetComponent<Image>().color = color.Value;
        }
    }

    private void UpdateView()
    {
        if (_currentPlant != null)
        {
            _plantImage.sprite = _currentPlant.PlantData.icon; // или growthSprites[0]
            _plantImage.gameObject.SetActive(true);
            _plantImage.transform.localScale = Vector3.one;
            int growthPercent = Mathf.RoundToInt(_currentPlant.GrowthProgress * 100f);
            _growthText.text = growthPercent + "%";
        }
        else
        {
            _plantImage.sprite = null;
            _plantImage.gameObject.SetActive(false);
            _growthText.text = "";
        }
    }

    // ---------- IPointer Handlers ----------
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_boardView != null)
            _boardView.OnCellPointerDown(_column, _row, eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_boardView != null)
            _boardView.OnCellPointerUp(_column, _row, eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_boardView != null)
            _boardView.OnCellPointerEnter(_column, _row);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_boardView != null)
            _boardView.OnCellPointerExit(_column, _row);
    }
}

public enum CellState
{
    Default,
    Highlighted,
    Occupied,
    Unavailable
}