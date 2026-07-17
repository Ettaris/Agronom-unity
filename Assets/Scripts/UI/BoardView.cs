using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Infrastructure;
using Infrastructure.Events;
using Commands;
using Gameplay;
using Managers;

public class BoardView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private Transform _cellsContainer;
    [SerializeField] private RectTransform _boardRect;

    [Header("DOTween Settings")]
    [SerializeField] private float _cellAppearDuration = 0.3f;
    [SerializeField] private float _cellAppearBounceAmplitude = 0.2f;

    [Header("Harvest Settings")]
    [SerializeField] private float _harvestCooldown = 0.05f;

    private RunData _runData;
    private GridBoard _board;
    private BoardCellView[,] _cellViews;
    private Vector2Int _boardSize;
    private float _cellSize;
    private bool _isPointerDown;
    private Vector2Int _lastHarvestedCell = new Vector2Int(-1, -1);
    private float _lastHarvestTime;
    private ItemInstance _selectedItem; // выбранная карточка из руки

    private void Awake()
    {
        _runData = ServiceLocator.Get<RunManager>().CurrentRunData;
        if (_runData == null)
        {
            Debug.LogError("BoardView: RunData is null!");
            return;
        }
        _board = _runData.Board;
        _boardSize = new Vector2Int(_board.Width, _board.Height);

        // Подписка на события
        EventBus.Subscribe<PlantPlacedEvent>(OnPlantPlaced);
        EventBus.Subscribe<PlantHarvestedEvent>(OnPlantHarvested);
        EventBus.Subscribe<PlantKilledEvent>(OnPlantKilled);
        EventBus.Subscribe<CardSelectedEvent>(OnCardSelected);

        InitializeBoard();
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<PlantPlacedEvent>(OnPlantPlaced);
        EventBus.Unsubscribe<PlantHarvestedEvent>(OnPlantHarvested);
        EventBus.Unsubscribe<PlantKilledEvent>(OnPlantKilled);
        EventBus.Unsubscribe<CardSelectedEvent>(OnCardSelected);
    }

    private void InitializeBoard()
    {
        foreach (Transform child in _cellsContainer)
            Destroy(child.gameObject);

        _cellViews = new BoardCellView[_boardSize.x, _boardSize.y];

        Rect rect = _boardRect.rect;
        float cellWidth = rect.width / _boardSize.x;
        float cellHeight = rect.height / _boardSize.y;
        _cellSize = Mathf.Min(cellWidth, cellHeight);

        for (int x = 0; x < _boardSize.x; x++)
        {
            for (int y = 0; y < _boardSize.y; y++)
            {
                GameObject cellObj = Instantiate(_cellPrefab, _cellsContainer);
                RectTransform cellRect = cellObj.GetComponent<RectTransform>();
                cellRect.sizeDelta = new Vector2(_cellSize, _cellSize);

                float posX = -rect.width / 2f + x * _cellSize + _cellSize / 2f;
                float posY = rect.height / 2f - y * _cellSize - _cellSize / 2f;
                cellRect.anchoredPosition = new Vector2(posX, posY);

                BoardCellView cellView = cellObj.GetComponent<BoardCellView>();
                cellView.Initialize(x, y, this);
                _cellViews[x, y] = cellView;

                cellObj.transform.localScale = Vector3.zero;
                cellObj.transform.DOScale(Vector3.one, _cellAppearDuration)
                    .SetDelay((x + y) * 0.02f)
                    .SetEase(Ease.OutBack, _cellAppearBounceAmplitude);

                UpdateCellView(x, y);
            }
        }
    }

    public void UpdateCellView(int x, int y)
    {
        if (x < 0 || x >= _boardSize.x || y < 0 || y >= _boardSize.y) return;
        Cell cell = _board.GetCell(x, y);
        BoardCellView view = _cellViews[x, y];
        view.SetPlant(cell.Plant);
        view.SetState(BoardCellView.CellState.Default);
    }

    // Обработчики событий – адресное обновление
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

    // Методы ввода (вызываются из BoardCellView)
    public void OnCellPointerDown(int x, int y)
    {
        _isPointerDown = true;
        _lastHarvestedCell = new Vector2Int(-1, -1);
        TryPlantOrHarvest(x, y);
    }

    public void OnCellPointerUp(int x, int y)
    {
        _isPointerDown = false;
        _lastHarvestedCell = new Vector2Int(-1, -1);
    }

    public void OnCellPointerEnter(int x, int y)
    {
        if (_isPointerDown)
        {
            if (Time.time - _lastHarvestTime >= _harvestCooldown)
            {
                TryHarvest(x, y);
            }
        }
        else
        {
            HighlightCell(x, y, true);
        }
    }

    public void OnCellPointerExit(int x, int y)
    {
        if (!_isPointerDown)
        {
            HighlightCell(x, y, false);
        }
    }

    private void TryPlantOrHarvest(int x, int y)
    {
        Cell cell = _board.GetCell(x, y);
        if (cell != null && cell.Plant != null)
        {
            if (cell.Plant.IsGrown)
            {
                TryHarvest(x, y);
                return;
            }
            else
            {
                _cellViews[x, y].SetState(BoardCellView.CellState.Unavailable);
                return;
            }
        }

        // Посадка: проверяем, что выбран предмет и он растение
        if (_selectedItem is PlantInstance plant)
        {
            // Проверяем, что клетка свободна
            if (_board.IsFree(x, y))
            {
                // Удаляем карточку из руки (это делает HandView, но мы отправляем команду)
                CommandProcessor.Execute(new PlacePlantCommand
                {
                    Plant = plant,
                    X = x,
                    Y = y
                });
                // После успешной посадки HandView получит событие обновления руки и удалит карточку
            }
        }
    }

    private void TryHarvest(int x, int y)
    {
        if (_lastHarvestedCell.x == x && _lastHarvestedCell.y == y) return;
        _lastHarvestedCell = new Vector2Int(x, y);
        _lastHarvestTime = Time.time;

        var harvestSystem = ServiceLocator.Get<Systems.HarvestSystem>();
        int calories = harvestSystem.HarvestPlantAt(x, y);
        if (calories > 0)
        {
            _cellViews[x, y].PlayHarvestAnimation();
        }
    }

    private void HighlightCell(int x, int y, bool highlight)
    {
        if (x < 0 || x >= _boardSize.x || y < 0 || y >= _boardSize.y) return;
        var cell = _board.GetCell(x, y);
        if (cell == null) return;

        if (highlight)
        {
            if (cell.Plant != null)
                _cellViews[x, y].SetState(BoardCellView.CellState.Occupied);
            else
                _cellViews[x, y].SetState(BoardCellView.CellState.Highlighted);
        }
        else
        {
            _cellViews[x, y].SetState(BoardCellView.CellState.Default);
        }
    }

    // Для Drag&Drop (карточка брошена на клетку) – вызывается из CardView через HandView
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