using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using Infrastructure;
using Infrastructure.Events;
using Commands;
using Gameplay;
using Systems;
using Managers;

public class BoardView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoardRoot _boardRoot; // можно назначить в инспекторе или найти на сцене

    private RunData _runData;
    private GridBoard _board;
    private bool _isPointerDown;
    private Vector2Int _lastHarvestedCell = new Vector2Int(-1, -1);
    private float _lastHarvestTime;
    private ItemInstance _selectedItem;

    private bool _useLegacy;

    private void Start()
    {
        if (_boardRoot == null)
            _boardRoot = FindAnyObjectByType<BoardRoot>();

        if (_boardRoot == null)
        {
            Debug.LogError("BoardView: BoardRoot not found in scene!");
            return;
        }

        _runData = ServiceLocator.Get<RunManager>().CurrentRunData;
        if (_runData == null) return;

        _board = _boardRoot.GridBoard;
        if (_board == null)
        {
            Debug.LogError("BoardView: GridBoard is null!");
            return;
        }

        EventBus.Subscribe<PlantPlacedEvent>(OnPlantPlaced);
        EventBus.Subscribe<PlantHarvestedEvent>(OnPlantHarvested);
        EventBus.Subscribe<PlantKilledEvent>(OnPlantKilled);
        EventBus.Subscribe<CardSelectedEvent>(OnCardSelected);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<PlantPlacedEvent>(OnPlantPlaced);
        EventBus.Unsubscribe<PlantHarvestedEvent>(OnPlantHarvested);
        EventBus.Unsubscribe<PlantKilledEvent>(OnPlantKilled);
        EventBus.Unsubscribe<CardSelectedEvent>(OnCardSelected);
    }

    // Обработчики событий
    private void OnPlantPlaced(PlantPlacedEvent evt)
    {
        UpdateCellView(evt.X, evt.Y);
    }

    private void OnPlantHarvested(PlantHarvestedEvent evt)
    {
        UpdateCellView(evt.X, evt.Y);
    }

    private void OnPlantKilled(PlantKilledEvent evt)
    {
        UpdateCellView(evt.X, evt.Y);
    }

    private void OnCardSelected(CardSelectedEvent evt)
    {
        _selectedItem = evt.Item;
    }

    private void UpdateCellView(int x, int y)
    {
        Debug.Log($"BoardView: UpdateCellView at ({x},{y})");
        var cellView = _boardRoot.GetCellView(x, y);
        if (cellView != null)
        {
            var plant = _board.GetCell(x, y)?.Plant;
            Debug.Log($"BoardView: Setting plant on cell view: {(plant != null ? plant.PlantData.itemName : "null")}");
            cellView.SetPlant(plant);
        }
        else
        {
            Debug.LogWarning($"BoardView: CellView not found at ({x},{y})");
        }
    }

    // Методы ввода (вызываются из CellView через события или через IPointer)
    public void OnCellPointerDown(int x, int y, PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        _isPointerDown = true;
        _lastHarvestedCell = new Vector2Int(-1, -1);
        TryPlantOrHarvest(x, y);
    }

    public void OnCellPointerUp(int x, int y, PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        _isPointerDown = false;
        _lastHarvestedCell = new Vector2Int(-1, -1);
    }

    public void OnCellPointerEnter(int x, int y)
    {
        if (_isPointerDown)
        {
            if (Time.time - _lastHarvestTime >= 0.05f)
                TryHarvest(x, y);
        }
        else
        {
            HighlightCell(x, y, true);
        }
    }

    public void OnCellPointerExit(int x, int y)
    {
        HighlightCell(x, y, false);
    }

    private void TryPlantOrHarvest(int x, int y)
    {
        var cell = _board.GetCell(x, y);
        if (cell != null && cell.Plant != null)
        {
            if (cell.Plant.IsGrown)
            {
                TryHarvest(x, y);
                return;
            }
            else
            {
                SetCellState(x, y, CellState.Unavailable);
                return;
            }
        }

        if (_selectedItem is PlantInstance plant)
        {
            if (_board.IsFree(x, y))
            {
                CommandProcessor.Execute(new PlacePlantCommand { Plant = plant, X = x, Y = y });
                _selectedItem = null;
            }
        }
    }

    private void TryHarvest(int x, int y)
    {
        if (_lastHarvestedCell.x == x && _lastHarvestedCell.y == y) return;
        _lastHarvestedCell = new Vector2Int(x, y);
        _lastHarvestTime = Time.time;

        var harvestSystem = ServiceLocator.Get<HarvestSystem>();
        harvestSystem.HarvestPlantAt(x, y);
    }

    private void HighlightCell(int x, int y, bool highlight)
    {
        var cell = _board.GetCell(x, y);
        if (cell == null) return;
        var cellView = _boardRoot.GetCellView(x, y);
        if (cellView == null) return;

        if (highlight)
        {
            cellView.SetState(cell.Plant != null ? CellState.Occupied : CellState.Highlighted);
        }
        else
        {
            cellView.SetState(CellState.Default);
        }
    }

    private void SetCellState(int x, int y, CellState state)
    {
        var cellView = _boardRoot.GetCellView(x, y);
        if (cellView != null) cellView.SetState(state);
    }

    // Метод для дропа с карточки
    public void HandleDropOnCell(int x, int y, ItemInstance item)
    {
        if (item is PlantInstance plant)
        {
            if (_board.IsFree(x, y))
            {
                CommandProcessor.Execute(new PlacePlantCommand { Plant = plant, X = x, Y = y });
            }
        }
    }
}