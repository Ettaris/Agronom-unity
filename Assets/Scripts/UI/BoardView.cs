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
using Managers;
using System;

public class BoardView : MonoBehaviour, IGameSystem
{
    [Header("References")]
    [SerializeField] private BoardRoot _boardRoot;
    [SerializeField] private GameObject _mutationViewPrefab; // префаб с MutationView
    [SerializeField] private Transform _plantContainer;

    private RunData _runData;
    private GridBoard _board;
    private bool _isPointerDown;
    private Vector2Int _lastHarvestedCell = new Vector2Int(-1, -1);
    private float _lastHarvestTime;
    private ItemInstance _selectedItem;
    private float _harvestCooldown = 0.05f;
    public Vector2Int _previewPosition;
    public bool _previewCanPlace;

    private Dictionary<PlantInstance, MutationView> _plantViews = new Dictionary<PlantInstance, MutationView>();
    private List<CellView> _previewCells = new List<CellView>();

    public void Initialize()
    {
        EventBus.Subscribe<PlantPlacedEvent>(OnPlantPlaced);
        EventBus.Subscribe<PlantHarvestedEvent>(OnPlantHarvested);
        EventBus.Subscribe<PlantKilledEvent>(OnPlantKilled);
        EventBus.Subscribe<CardSelectedEvent>(OnCardSelected);
        EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
    }

    public void Dispose()
    {
        EventBus.Unsubscribe<PlantPlacedEvent>(OnPlantPlaced);
        EventBus.Unsubscribe<PlantHarvestedEvent>(OnPlantHarvested);
        EventBus.Unsubscribe<PlantKilledEvent>(OnPlantKilled);
        EventBus.Unsubscribe<CardSelectedEvent>(OnCardSelected);
    }

    private void OnRunStarted(RunStartedEvent @event)
    {
        Debug.Log("RunStartedEvent from boardView");
        _runData = ServiceLocator.Get<RunManager>().CurrentRunData;
        if (_runData == null) return;

        if (_boardRoot == null) _boardRoot = ServiceLocator.TryGet(out BoardRoot br) ? br : null;
        if (_boardRoot == null) { Debug.LogError("BoardView: BoardRoot not found!"); return; }
        _board = _boardRoot.GridBoard;
        if (_board == null) { Debug.LogError("BoardView: GridBoard is null!"); return; }
    }

    // ---------- Обработчики событий ----------
    private void OnPlantPlaced(PlantPlacedEvent evt)
    {
        Debug.Log("OnPlantPlaced from boardView");
        UpdateCellView(evt.X, evt.Y);
        CreatePlantVisual(evt.Plant);
    }

    private void OnPlantHarvested(PlantHarvestedEvent evt)
    {
        RemovePlantVisual(evt.Plant);
        UpdateCellView(evt.X, evt.Y);
    }

    private void OnPlantKilled(PlantKilledEvent evt)
    {
        RemovePlantVisual(evt.Plant);
        UpdateCellView(evt.X, evt.Y);
    }

    private void OnCardSelected(CardSelectedEvent evt)
    {
        _selectedItem = evt.Item;
    }

    // ---------- Визуал растения ----------
    private void CreatePlantVisual(PlantInstance plant)
    {
        if (plant == null) return;
        if (_plantViews.ContainsKey(plant)) return;

        Vector2Int pos = plant.Position;
        var anchorCell = _boardRoot.GetCellView(pos.x, pos.y);
        if (anchorCell == null)
        {
            Debug.LogWarning($"BoardView: No cell view at ({pos.x},{pos.y})");
            return;
        }

        // Создаём как дочерний объект опорной клетки
        GameObject go = Instantiate(_mutationViewPrefab, anchorCell.transform);
        MutationView view = go.GetComponent<MutationView>();
        view.Initialize(plant);
        _plantViews[plant] = view;
    }


    public void OnCellPointerEnter(int x, int y)
    {
        if (_isPointerDown)
        {
            if (Time.time - _lastHarvestTime >= _harvestCooldown)
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

    private void HighlightCell(int x, int y, bool highlight)
    {
        var cell = _board.GetCell(x, y);
        if (cell == null) return;
        var view = _boardRoot.GetCellView(x, y);
        if (view == null) return;

        if (highlight)
            view.SetState(cell.Plant != null ? CellState.Occupied : CellState.Highlighted);
        else
            view.SetState(CellState.Default);
    }

    // ----- Preview (Drag&Drop) -----
    public void UpdatePreviewFromScreen(Vector2 screenPos, PlantData plantData)
    {
        if (FindBestPlacement(screenPos, plantData.size, out var pos, out var canPlace))
        {
            _previewPosition = pos;
            _previewCanPlace = canPlace;
            ShowPreview(pos, plantData.size, canPlace);
        }
        else
        {
            ClearPreview();
        }
    }

    public void ClearPreviewFromDrag()
    {
        ClearPreview();
    }

    private void ShowPreview(Vector2Int position, Vector2Int size, bool canPlace)
    {
        ClearPreview();
        for (int dx = 0; dx < size.x; dx++)
            for (int dy = 0; dy < size.y; dy++)
            {
                int cx = position.x + dx, cy = position.y + dy;
                var cellView = _boardRoot.GetCellView(cx, cy);
                if (cellView != null)
                {
                    cellView.SetState(canPlace ? CellState.Highlighted : CellState.Unavailable);
                    _previewCells.Add(cellView);
                }
            }
    }

    private void ClearPreview()
    {
        foreach (var cell in _previewCells)
            cell.SetState(CellState.Default);
        _previewCells.Clear();
    }

    // ----- FindBestPlacement -----
    /// <summary>
    /// Находит наилучшую опорную позицию (левый верхний угол) для растения заданного размера,
    /// ближайшую к экранной позиции курсора.
    /// </summary>
    public bool FindBestPlacement(Vector2 screenPos, Vector2Int size, out Vector2Int position, out bool canPlace)
    {
        position = Vector2Int.zero;
        canPlace = false;

        var cellUnderCursor = GetCellFromScreen(screenPos);
        if (cellUnderCursor == null)
        {
            Debug.Log("FindBestPlacement: No cell under cursor");
            return false;
        }
        Debug.Log($"FindBestPlacement: Cell under cursor = ({cellUnderCursor.X}, {cellUnderCursor.Y}), size = {size}");

        if (size.x == 1 && size.y == 1)
        {
            position = new Vector2Int(cellUnderCursor.X, cellUnderCursor.Y);
            canPlace = _board.CanPlace(position, size);
            Debug.Log($"FindBestPlacement: 1x1 placement at {position}, canPlace = {canPlace}");
            return true;
        }

        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int dx = -(size.x - 1); dx <= 0; dx++)
            for (int dy = -(size.y - 1); dy <= 0; dy++)
            {
                int px = cellUnderCursor.X + dx;
                int py = cellUnderCursor.Y + dy;
                if (px >= 0 && py >= 0 && px + size.x <= _board.Width && py + size.y <= _board.Height)
                    candidates.Add(new Vector2Int(px, py));
            }

        if (candidates.Count == 0)
        {
            Debug.Log("FindBestPlacement: No valid candidates");
            return false;
        }

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        Vector2Int bestPos = candidates[0];
        float bestDist = float.MaxValue;

        foreach (var pos in candidates)
        {
            Vector2 center = GetCenterOfArea(pos, size);
            float dist = Vector2.Distance(worldPos, center);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestPos = pos;
            }
        }

        position = bestPos;
        canPlace = _board.CanPlace(position, size);
        Debug.Log($"FindBestPlacement: Best position = {position}, canPlace = {canPlace}");
        return true;
    }

    private Vector2 GetCenterOfArea(Vector2Int pos, Vector2Int size)
    {
        float sumX = 0, sumY = 0;
        int count = 0;
        for (int dx = 0; dx < size.x; dx++)
            for (int dy = 0; dy < size.y; dy++)
            {
                var cellView = _boardRoot.GetCellView(pos.x + dx, pos.y + dy);
                if (cellView != null)
                {
                    Vector3 cellPos = cellView.transform.position;
                    sumX += cellPos.x;
                    sumY += cellPos.y;
                    count++;
                }
            }
        return new Vector2(count > 0 ? sumX / count : 0, count > 0 ? sumY / count : 0);
    }

    private CellView GetCellFromScreen(Vector2 screenPos)
    {
        var results = new List<RaycastResult>();
        PointerEventData ped = new PointerEventData(EventSystem.current) { position = screenPos };
        EventSystem.current.RaycastAll(ped, results);
        foreach (var res in results)
        {
            var cell = res.gameObject.GetComponent<CellView>();
            if (cell != null) return cell;
        }
        return null;
    }

    private void RemovePlantVisual(PlantInstance plant)
    {
        if (plant == null) return;
        if (_plantViews.TryGetValue(plant, out var view))
        {
            Destroy(view.gameObject);
            _plantViews.Remove(plant);
        }
    }

    public void RefreshPlantVisual(PlantInstance plant)
    {
        if (_plantViews.TryGetValue(plant, out var view))
            view.Refresh();
    }

    // ---------- Обновление клеток ----------
    private void UpdateCellView(int x, int y)
    {
        var cellView = _boardRoot.GetCellView(x, y);
        if (cellView != null)
        {
            var plant = _board.GetCell(x, y)?.Plant;
            cellView.SetState(plant != null ? CellState.Occupied : CellState.Default);
        }
    }


    private void SetCellState(int x, int y, CellState state)
    {
        var view = _boardRoot.GetCellView(x, y);
        if (view != null) view.SetState(state);
    }

    // ---------- Превью при Drag&Drop ----------

    // ---------- Ввод ----------
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

    // ---------- Посадка и сбор ----------
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
            Vector2Int pos = new Vector2Int(x, y);
            if (_board.CanPlace(pos, plant.PlantData.size))
            {
                CommandProcessor.Execute(new PlacePlantCommand { Plant = plant, X = x, Y = y });
                _selectedItem = null;
            }
            else
            {
                // Подсветить красным на мгновение
                ShowPreview(pos, plant.PlantData.size, false);
                DOVirtual.DelayedCall(0.5f, ClearPreview);
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


}