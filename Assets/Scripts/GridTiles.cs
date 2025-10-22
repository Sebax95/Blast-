using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class GridTiles: BaseMonoBehaviour, IObservable
{
    public int gridSizeX;
    public int gridSizeY;
    public float cellSize = 1f;
    public float tileSpacing = 0f;

    private Tile[,] _tiles;
    private TileObject[,] _tileObjects;
    
    private ObjectPool<TileObject> _tileObjectPool;
    [SerializeField]
    private TileObject _tileObjectPrefab;

    private int _totalTiles;
    private List<IObserver> _allObservers = new();
    private Dictionary<int, bool> _columnBusy = new();
    private Dictionary<int, Queue<Action>> _columnQueues = new();
    

    [SerializeField]
    private bool _showGizmos;

    private void Awake() => _tileObjectPool =
        new ObjectPool<TileObject>(FactoryTiles, TileObject.TurnOn, TileObject.TurnOff, gridSizeX * gridSizeY);

    #region Observer
    
    public void Subscribe(IObserver observer)
    {
        if(!_allObservers.Contains(observer))
            _allObservers.Add(observer);
    }

    public void Unsubscribe(IObserver observer)
    {
        if(_allObservers.Contains(observer))
            _allObservers.Remove(observer);
    }

    #endregion

    #region Object Pooling

    private TileObject FactoryTiles() => Instantiate(_tileObjectPrefab);
    private TileObject GetTileObjectFromPool()
    {
        var obj = _tileObjectPool.GetObject();
        obj.transform.SetParent(transform);
        return obj;
    }
    private void ReturnTileObjectToPool(TileObject obj)
    {
        _tileObjectPool.ReturnObject(obj);
        _totalTiles--;
        GameManager.OnGridTilesChanged?.Invoke(_totalTiles);
        
    }
    

    #endregion

    #region Level Creation

    public void BuildFromLevel(LevelGenerator level)
    {
        _totalTiles = 0;
        gridSizeX = level.cols;
        gridSizeY = level.rows;
        ClearAll();
        _tiles = new Tile[gridSizeX, gridSizeY];
        _tileObjects = new TileObject[gridSizeX, gridSizeY];
        var src = level.To2DArray();
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                var s = src[y, x];
                var t = new Tile(s.id, s.color, new Vector2(x, y));
                _tiles[x, y] = t;
                var obj = GetTileObjectFromPool();
                BindTileObject(obj, t);
                _tileObjects[x, y] = obj;
                _totalTiles++;
            }
        }
    }

    private void ClearAll()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var to = transform.GetChild(i).GetComponent<TileObject>();
            if (to != null) Destroy(to.gameObject);
        }
        _tiles = null;
        _tileObjects = null;
    }
    private void BindTileObject(TileObject obj, Tile tile)
    {
        obj.transform.SetParent(transform);
        obj.Tile = tile;
        obj.grid = this;
        obj.UpdatePosition();
        obj.UpdateSprite();
        obj.OrderLayer();
    }
    #endregion
    
    public TileObject GetTileObjectFromTile(Tile tile)
    {
        for (int y = 0; y < gridSizeY; y++)
            for (int x = 0; x < gridSizeX; x++)
                if(_tiles[x,y] == tile)
                    return _tileObjects[x,y];
        return null;
    }
    public Vector2 GetTilePosition(float x, float y)
    {
        float rowOffsetX = (y % 2 == 1) ? 3.3f : 0f;
        float stepX = cellSize + tileSpacing;
        float stepY = cellSize + tileSpacing;
        Vector3 basePos = transform.position + new Vector3(x * stepX, y * stepY, 0f);
        return basePos + new Vector3(rowOffsetX, 0f, 0f);
    }

    public List<Tile> GetFirstLayer()
    {
        var result = new List<Tile>(gridSizeX);
        if (_tiles == null) return result;

        int y = 0;
        for (int x = 0; x < gridSizeX; x++)
        {
            if (_columnBusy.TryGetValue(x, out var busy) && busy) 
                continue;

            var t = _tiles[x, y];
            if (t != null) result.Add(t);
        }
        return result;
    }
    public void RemoveFirstLayerAtColumn(int xRow)
    {
        if (_tiles == null || xRow < 0 || xRow >= gridSizeX)
        {
            ReleaseColumn(xRow);
            return;
        }

        _columnBusy.TryAdd(xRow, false);
        if (!_columnQueues.ContainsKey(xRow)) 
            _columnQueues[xRow] = new();

        _columnQueues[xRow].Enqueue(() => ProcessColumnRemoval(xRow));

        DequeueAndRun(xRow);
    }

    private void ProcessColumnRemoval(int xRow)
    {
        float delayAfterDeath = 0.1f;

        Tile bottomTile = _tiles[xRow, 0];
        if (bottomTile != null)
        {
            bottomTile.isBusy = true;

            for (int i = 0; i < transform.childCount; i++)
            {
                var to = transform.GetChild(i).GetComponent<TileObject>();
                if (to == null || to.Tile != bottomTile)
                    continue;

                to.DestroyTile(() =>
                {
                    ReturnTileObjectToPool(to);
                    DOVirtual.DelayedCall(delayAfterDeath, () =>
                    {
                        ShiftColumnDown(xRow);
                        
                        var newFirst = _tiles[xRow, 0];
                        if (newFirst != null)
                            newFirst.isBusy = false;

                        UpdateAllTileObjects();
                        ReleaseColumn(xRow);
                        DequeueAndRun(xRow);
                    });
                });
                return;
            }
        }

        ReleaseColumn(xRow);
        DequeueAndRun(xRow);
    }

    private void DequeueAndRun(int xRow)
    {
        if (_columnQueues[xRow].Count == 0) return;

        var op = _columnQueues[xRow].Dequeue();
        op.Invoke();
    }

    private void ShiftColumnDown(int xRow)
    {
        for (int y = 1; y < gridSizeY; y++)
        {
            _tiles[xRow, y - 1] = _tiles[xRow, y];
            if (_tiles[xRow, y - 1] != null)
                _tiles[xRow, y - 1].positionGrid = new Vector2(xRow, y - 1);
        }
        _tiles[xRow, gridSizeY - 1] = null;
    }

    private void UpdateAllTileObjects()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var to = transform.GetChild(i).GetComponent<TileObject>();
            if (to == null || !to.gameObject.activeSelf || to.Tile == null) continue;
            to.UpdatePosition(NotifyObservers);
            to.UpdateSprite();
        }
    }

    public bool TryClaimColumn(int xRow)
    {
        if (xRow < 0 || xRow >= gridSizeX) return false;
        _columnBusy.TryAdd(xRow, false);
        if (_columnBusy[xRow]) return false;
        _columnBusy[xRow] = true;
        return true;
    }

    public void ReleaseColumn(int xRow)
    {
        if (xRow < 0 || xRow >= gridSizeX) return;
        _columnBusy.TryAdd(xRow, false);
        _columnBusy[xRow] = false;
    }
    private void NotifyObservers()
    {
        foreach (var item in _allObservers)
            item.OnNotify(ObserverMessage.UpdateRow);
    }

 
}
