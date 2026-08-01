using System.Collections.Generic;
using UnityEngine;
using Gameplay;
using Infrastructure;
using Managers;
using Infrastructure.Events;
using System;

public class BoardRoot : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int _width = 5;
    [SerializeField] private int _height = 5;

    private GridBoard _gridBoard;
    private Dictionary<Vector2Int, CellView> _cellViews = new Dictionary<Vector2Int, CellView>();

    public GridBoard GridBoard => _gridBoard;
    public IReadOnlyDictionary<Vector2Int, CellView> CellViews => _cellViews;

    private void Awake()
    {
        BuildBoard();
        ServiceLocator.Register(this);

        EventBus.Subscribe<RunStartedEvent>(OnRunStarted);

        // ======== —»Ќ’–ќЌ»«ј÷»я ========

    }

    private void OnRunStarted(RunStartedEvent @event)
    {
        @event.RunData.Board = _gridBoard;
        Debug.Log("BoardRoot: Synced RunData.Board with GridBoard");

    }

    private void BuildBoard()
    {
        var childCells = GetComponentsInChildren<CellView>();
        if (childCells.Length == 0)
        {
            Debug.LogWarning("BoardRoot: No CellView children found. Creating default grid (procedural fallback).");
            CreateDefaultGrid();
            return;
        }

        int maxRow = 0, maxCol = 0;
        foreach (var cell in childCells)
        {
            maxRow = Mathf.Max(maxRow, cell.Row);
            maxCol = Mathf.Max(maxCol, cell.Column);
        }

        _gridBoard = new GridBoard(maxCol + 1, maxRow + 1);

        foreach (var cell in childCells)
        {
            var logicCell = _gridBoard.GetCell(cell.Column, cell.Row);
            if (logicCell == null) continue;

            cell.Initialize(logicCell);
            _cellViews[new Vector2Int(cell.Column, cell.Row)] = cell;
        }
    }

    private void CreateDefaultGrid()
    {
        // Fallback Ц процедурна€ сетка (без визуальных клеток)
        _gridBoard = new GridBoard(_width, _height);
        Debug.LogWarning("BoardRoot: No child cells found. GridBoard created but no visual cells exist.");
    }

    public CellView GetCellView(int col, int row)
    {
        _cellViews.TryGetValue(new Vector2Int(col, row), out var view);
        return view;
    }
}