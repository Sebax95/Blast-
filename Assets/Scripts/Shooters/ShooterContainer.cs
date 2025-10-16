using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShooterContainer : BaseMonoBehaviour
{
    [SerializeField]
    private Shooter _slotPrefab;
    [SerializeField]
    private float _spaceing;

    private Shooter[,] _grid; // [col,row]
    private int _cols;
    private int _rows;
    
    public static Action<Shooter> OnShooterSelected;
    
    protected override void Start()
    {
        base.Start();
      //  CreateShooters(6);
        OnShooterSelected += MoveShooter;
    }

    private int GetIndex(Shooter shooter)
    {
        if (_grid == null || shooter == null) 
            return -1;
        for (int y = 0; y < _rows; y++)
            for (int x = 0; x < _cols; x++)
                if (_grid[x, y] == shooter)
                    return y * _cols + x;
        return -1;
    }
    
    private void ComputeGridSize(int quantity, out int cols, out int rows)
    {
        if (quantity <= 0) { cols = rows = 0; return; }
        cols = Mathf.Min(quantity, 5);
        rows = Mathf.CeilToInt((float)quantity / cols);
    }

    public void CreateShootersFromLevel(LevelGenerator level)
    {
        if (_grid != null)
        {
            for (int y = 0; y < _rows; y++)
                for (int x = 0; x < _cols; x++)
                    if (_grid[x, y] != null)
                        Destroy(_grid[x, y].gameObject);
        }

        var list = level.shooters;
        int quantity = list?.Count ?? 0;
        if (quantity <= 0) { _grid = null; _cols = _rows = 0; return; }

        ComputeGridSize(quantity, out _cols, out _rows);
        _grid = new Shooter[_cols, _rows];

        for (int i = 0; i < quantity; i++)
        {
            int x = i % _cols;
            int y = i / _cols;
            var src = list[i];
            var obj = Instantiate(_slotPrefab, transform);
            obj.color = src.color;
            obj.quantityBullets = Mathf.Clamp(src.bullets, 1, 9999);
            _grid[x, y] = obj;
        }

        for (int y = 0; y < _rows; y++)
        {
            bool isBottomRow = y == _rows - 1;
            for (int x = 0; x < _cols; x++)
            {
                var s = _grid[x, y];
                if (s == null) continue;
                s.IsPickeable = isBottomRow;
            }
        }

        ReorderShooter();
    }
    
    public void CreateShooters(int quantity = 2)
    {
        if (_grid != null)
        {
            for (int y = 0; y < _rows; y++)
                for (int x = 0; x < _cols; x++)
                    if (_grid[x, y] != null)
                        Destroy(_grid[x, y].gameObject);
        }

        ComputeGridSize(quantity, out _cols, out _rows);
        if (_cols == 0 || _rows == 0) return;

        _grid = new Shooter[_cols, _rows];

        int created = 0;
        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _cols; x++)
            {
                if (created >= quantity) 
                    break;
                var obj = Instantiate(_slotPrefab, transform);
                var randomColor = (ColorTile)Random.Range(0, Enum.GetValues(typeof(ColorTile)).Length);
                obj.color = randomColor;
                _grid[x, y] = obj;
                created++;
                obj.IsPickeable = y == _rows-1;
            }
            if (created >= quantity)
                break;
        }
        ReorderShooter();
    }

    private void ReorderShooter()
    {
        if (_grid == null) return;

        float totalWidth = (_cols - 1) * _spaceing;
        float totalHeight = (_rows - 1) * _spaceing;
        float startX = -totalWidth * 0.5f;
        float startY = -totalHeight * 0.5f;

        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _cols; x++)
            {
                var slot = _grid[x, y];
                if (slot == null) continue;

                float px = startX + x * _spaceing;
                float py = startY + y * _spaceing;
                slot.transform.localPosition = new Vector3(px, py, 0f);
            }
        }
    }
    
    private void IndexToCoord(int index, out int col, out int row)
    {
        row = index / _cols;
        col = index % _cols;
    }

    private void MoveShooter(Shooter shooter)
    {
        if (_grid == null || shooter == null) 
            return;

        int idx = GetIndex(shooter);
        if (idx < 0) 
            return;

        IndexToCoord(idx, out int col, out int row);
        
        float totalW = (_cols - 1) * _spaceing;
        float totalH = (_rows - 1) * _spaceing;
        float startX = -totalW * 0.5f;
        float startY = -totalH * 0.5f;
        Vector3 TargetPos(int x, int y) => new(startX + x * _spaceing, startY + y * _spaceing, 0f);
        
        _grid[col, row] = null;
        
        for (int y = row - 1; y >= 0; y--)
        {
            var s = _grid[col, y];
            _grid[col, y + 1] = s;
            _grid[col, y] = null;
            if (s != null) s.transform.DOLocalMove(TargetPos(col, y + 1), 0.35f).SetEase(Ease.InCubic);
        }
        
        for (int y = 0; y < _rows; y++)
            if (_grid[col, y] != null) 
                _grid[col, y].IsPickeable = false;

        for (int y = _rows - 1; y >= 0; y--)
            if (_grid[col, y] != null) 
            { 
                _grid[col, y].IsPickeable = true;
                break; 
            }
    }
    
}
