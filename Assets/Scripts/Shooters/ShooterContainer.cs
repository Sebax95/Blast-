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
    [SerializeField]
    private float _marginWithSlots = 0.4f;

    private Shooter[,] _grid;
    private int _cols;
    private int _rows;
    
    public static Action<Shooter> OnShooterSelected;
    private SlotsContainers _slotsContainers;
    protected override void Start()
    {
        base.Start();
        _slotsContainers = FindAnyObjectByType<SlotsContainers>();
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
        if (quantity <= 0)
        {
            cols = rows = 0;
            return;
        }
        switch (quantity)
        {
            case <= 2:    cols = 2; break;
            case <= 6:    cols = 3; break;
            case <= 12:   cols = 4; break;
            default:      cols = 5; break;
        }

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
        
        /*int filled = quantity;
        if (filled < _cols * _rows)
        {
            for (int i = filled; i < _cols * _rows; i++)
            {
                int x = i % _cols;
                int y = i / _cols;
                var src = list[Random.Range(0, list.Count)];
                var obj = Instantiate(_slotPrefab, transform);
                obj.color = src.color;
                obj.quantityBullets = Mathf.Clamp(src.bullets, 1, 9999);
                _grid[x, y] = obj;
            }
        }*/
        
        RandomizeGridFilledOnly();
        CompactGridToEnd();

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
        AlignUnderSlots();
    }

    private void SwapGridElements(int x1, int y1, int x2, int y2) =>
        (_grid[x1, y1], _grid[x2, y2]) = (_grid[x2, y2], _grid[x1, y1]);

    private void RandomizeGridFilledOnly()
    {
        if (_grid == null) return;
        
        var filled = new List<Shooter>(_cols * _rows);
        for (int y = 0; y < _rows; y++)
        for (int x = 0; x < _cols; x++)
            if (_grid[x, y] != null)
                filled.Add(_grid[x, y]);
        
        for (int i = filled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (filled[i], filled[j]) = (filled[j], filled[i]);
        }
        
        int k = 0;
        for (int y = 0; y < _rows; y++)
        for (int x = 0; x < _cols; x++)
            _grid[x, y] = (k < filled.Count) ? filled[k++] : null;
    }
    private void CompactGridToEnd()
    {
        if (_grid == null) return;

        var filled = new List<Shooter>(_cols * _rows);
        
        for (int y = 0; y < _rows; y++)
        for (int x = 0; x < _cols; x++)
            if (_grid[x, y] != null)
                filled.Add(_grid[x, y]);
        
        for (int y = 0; y < _rows; y++)
        for (int x = 0; x < _cols; x++)
            _grid[x, y] = null;
        
        int k = 0;
        for (int y = _rows - 1; y >= 0; y--)
        {
            for (int x = 0; x < _cols; x++)
            {
                if (k < filled.Count)
                    _grid[x, y] = filled[k++];
                else
                    _grid[x, y] = null;
            }
        }
        
        ReorderShooter();
        
        for (int y = 0; y < _rows; y++)
        for (int x = 0; x < _cols; x++)
            if (_grid[x, y] != null)
                _grid[x, y].IsPickeable = (y == _rows - 1);
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
    
    private void AlignUnderSlots()
    {
        if (_grid == null || _slotsContainers == null) 
            return;
        
        float topShooterLocalY = float.NegativeInfinity;
        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _cols; x++)
            {
                var s = _grid[x, y];
                if (s == null) continue;
                topShooterLocalY = Mathf.Max(topShooterLocalY, s.transform.localPosition.y);
            }
        }
        if (float.IsNegativeInfinity(topShooterLocalY)) return;
        
        float topShooterWorldY = transform.TransformPoint(new Vector3(0f, topShooterLocalY, 0f)).y;
        
        float slotsBottomY = _slotsContainers.transform.position.y;
        var r = _slotsContainers.GetComponentInChildren<Renderer>();
        if (r != null) slotsBottomY = r.bounds.min.y;
        
        float neededGap = _marginWithSlots;
        float overlap = (topShooterWorldY + neededGap) - slotsBottomY;
        if (overlap > 0f)
        {
            var pos = transform.position;
            pos.y -= overlap;
            transform.position = pos;
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
