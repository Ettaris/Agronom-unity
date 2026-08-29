using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using Infrastructure;
using Infrastructure.Events;
using Commands;
using Gameplay;
using Data;
using Systems;
using System;

/// <summary>
/// Отвечает за визуализацию игрового поля, обработку ввода (посадка, сбор, подсветка) и отображение превью.
/// </summary>
public class BoardView : MonoBehaviour, IGameSystem
{
    [Header("References")]
    [SerializeField] private BoardRoot _boardRoot;
    [SerializeField] private PlacementPreview _previewUI;

    [Header("Settings")]
    [SerializeField] private float _harvestCooldown = 0.05f;

    private RunData _runData;
    private GridBoard _board;
    private PlacementPreviewSystem _placementPreviewSystem;
    private ItemInstance _selectedItem;

    private bool _isPointerDown;
    private Vector2Int _lastHarvestedCell = new Vector2Int(-1, -1);
    private float _lastHarvestTime;
    private Vector2Int? _lastPreviewCell;

    private List<CellView> _previewCells = new List<CellView>();
    public Vector2Int PreviewPosition { get; private set; }
    public bool PreviewCanPlace { get; private set; }


    public void Initialize()
    {
        _placementPreviewSystem = ServiceLocator.Get<PlacementPreviewSystem>();

        EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
        EventBus.Subscribe<PlantPlacedEvent>(OnPlantPlaced);
        EventBus.Subscribe<PlantHarvestedEvent>(OnPlantHarvested);
        EventBus.Subscribe<PlantKilledEvent>(OnPlantKilled);
        EventBus.Subscribe<CardSelectedEvent>(OnCardSelected);
    }

    public void Dispose()
    {
        EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
        EventBus.Unsubscribe<PlantPlacedEvent>(OnPlantPlaced);
        EventBus.Unsubscribe<PlantHarvestedEvent>(OnPlantHarvested);
        EventBus.Unsubscribe<PlantKilledEvent>(OnPlantKilled);
        EventBus.Unsubscribe<CardSelectedEvent>(OnCardSelected);
    }


    #region Event Handlers

    private void OnRunStarted(RunStartedEvent evt)
    {
        _runData = evt.RunData;
        if (_runData == null) return;

        _boardRoot = ServiceLocator.TryGet(out BoardRoot br) ? br : _boardRoot;
        if (_boardRoot == null)
        {
            Debug.LogError("BoardView: BoardRoot not found!");
            return;
        }

        _board = _boardRoot.GridBoard;
        if (_board == null)
        {
            Debug.LogError("BoardView: GridBoard is null!");
        }
    }

    private void OnPlantPlaced(PlantPlacedEvent evt) => UpdateCellView(evt.X, evt.Y);
    private void OnPlantHarvested(PlantHarvestedEvent evt) => UpdateCellView(evt.X, evt.Y);
    private void OnPlantKilled(PlantKilledEvent evt) => UpdateCellView(evt.X, evt.Y);
    private void OnCardSelected(CardSelectedEvent evt) => _selectedItem = evt.Item;

    #endregion

    #region Cell Highlight & Preview

    public void OnCellPointerEnter(int x, int y)
    {
        if (_isPointerDown)
        {
            if (Time.time - _lastHarvestTime >= _harvestCooldown)
                TryHarvest(x, y);
            ClearPreviewFromDrag();
        }
        else
        {
            HighlightCell(x, y, true);
        }
    }

    public void OnCellPointerExit(int x, int y) => HighlightCell(x, y, false);

    private void HighlightCell(int x, int y, bool highlight)
    {
        if (_board == null || _boardRoot == null) return;

        var cell = _board.GetCell(x, y);
        if (cell == null) return;

        var view = _boardRoot.GetCellView(x, y);
        if (view == null) return;

        view.SetState(highlight ? (cell.Plant != null ? CellState.Default : CellState.Highlighted) : CellState.Default);
    }

    // ---------- Preview (Drag&Drop) ----------
    public void UpdatePreviewFromScreen(Vector2 screenPos, PlantData plantData)
    {
        if (plantData == null || _board == null || _boardRoot == null)
        {
            ClearPreviewUI();
            _lastPreviewCell = null;
            return;
        }

        var cell = GetCellFromScreen(screenPos);
        if (cell == null)
        {
            ClearPreviewUI();
            _lastPreviewCell = null;
            return;
        }

        Vector2Int pos = new Vector2Int(cell.X, cell.Y);
        PreviewPosition = pos;

        if (_lastPreviewCell.HasValue && _lastPreviewCell.Value == pos) return;

        _lastPreviewCell = pos;
        bool canPlace = _board.CanPlace(pos, Vector2Int.one);
        PreviewCanPlace = canPlace;

        ClearPreviewCells();
        cell.SetState(canPlace ? CellState.Highlighted : CellState.Default);
        if (canPlace)
            _previewCells.Add(cell);

        if (canPlace && _placementPreviewSystem != null && _previewUI != null)
        {
            var result = _placementPreviewSystem.GetPreview(pos.x, pos.y);
            if (result != null)
                _previewUI.Show(result, cell.transform.position);
            else
                _previewUI?.Hide();
        }
        else
        {
            _previewUI?.Hide();
        }
    }

    private void ClearPreviewUI()
    {
        _previewUI?.Hide();
        _lastPreviewCell = null;
    }

    public void ClearPreviewFromDrag()
    {
        ClearPreviewCells();
        _previewUI?.Hide();
        _lastPreviewCell = null;
        _placementPreviewSystem?.EndDrag();
    }

    private void ClearPreviewCells()
    {
        foreach (var cell in _previewCells)
            cell.SetState(CellState.Default);
        _previewCells.Clear();
    }


    public CellView GetCellFromScreen(Vector2 screenPos)
    {
        var results = new List<RaycastResult>();
        var ped = new PointerEventData(EventSystem.current) { position = screenPos };
        EventSystem.current.RaycastAll(ped, results);

        foreach (var result in results)
        {
            var cell = result.gameObject.GetComponent<CellView>();
            if (cell != null) return cell;
        }
        return null;
    }

    #endregion

    #region Cell Updates

    public void UpdateCellView(int x, int y)
    {
        if (_boardRoot == null || _board == null) return;

        var cellView = _boardRoot.GetCellView(x, y);
        if (cellView != null)
        {
            var plant = _board.GetCell(x, y)?.Plant;
            cellView.SetPlant(plant);
        }
    }

    private void SetCellState(int x, int y, CellState state)
    {
        var view = _boardRoot?.GetCellView(x, y);
        view?.SetState(state);
    }

    #endregion

    #region Input & Actions

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

    private void TryPlantOrHarvest(int x, int y)
    {
        if (_board == null || _boardRoot == null) return;
        _previewUI?.Hide();

        var cell = _board.GetCell(x, y);
        if (cell != null && cell.Plant != null)
        {
            if (cell.Plant.IsGrown)
                TryHarvest(x, y);
            else
                SetCellState(x, y, CellState.Default);
            return;
        }

        if (_selectedItem is PlantInstance plant)
        {
            Vector2Int pos = new Vector2Int(x, y);
            if (_board.CanPlace(pos, Vector2Int.one))
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

        var cellView = _boardRoot?.GetCellView(x, y);
        Vector3 worldPos = cellView != null ? cellView.transform.position : Vector3.zero;

        CommandProcessor.Execute(new HarvestCommand
        {
            X = x,
            Y = y,
            ScreenPos = worldPos
        });
    }

    #endregion

    #region Board Cleanup

    public void ClearBoard()
    {
        if (_boardRoot != null)
        {
            foreach (var cell in _boardRoot.CellViews.Values)
            {
                cell.ClearPlant();
                cell.SetState(CellState.Default);
            }
        }

        _selectedItem = null;
        _isPointerDown = false;
        _lastHarvestedCell = new Vector2Int(-1, -1);
        ClearPreviewCells();
        _previewUI?.Hide();
    }

    public void ClearGridBoard()
    {
        _boardRoot?.GridBoard.Clear();
    }

    #endregion
}