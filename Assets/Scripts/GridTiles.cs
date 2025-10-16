using System.Collections.Generic;
using System.Linq;
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
    
    private List<IObserver> _allObservers = new();

    [SerializeField]
    private bool _showGizmos;
    
    private void Awake()
    {
        _tileObjectPool = new ObjectPool<TileObject>(FactoryTiles, TileObject.TurnOn, TileObject.TurnOff, gridSizeX * gridSizeY);
    }
    protected override void Start()
    {
        base.Start();
        //CreateGrid();
    }

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
    private void ReturnTileObjectToPool(TileObject obj) => _tileObjectPool.ReturnObject(obj);
    

    #endregion
    
    public void BuildFromLevel(LevelGenerator level)
    {
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
            }
        }
    }
    
    public void ClearAll()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var to = transform.GetChild(i).GetComponent<TileObject>();
            if (to != null) Destroy(to.gameObject);
        }
        _tiles = null;
        _tileObjects = null;
    }
    
    private void CreateGrid()
    {
        _tiles = new Tile[gridSizeX, gridSizeY];
        _tileObjects = new TileObject[gridSizeX, gridSizeY];
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                var values = (ColorTile[])System.Enum.GetValues(typeof(ColorTile));
                _tiles[x,y] = new Tile(x+y, ColorTile.Green, new Vector2(x,y));
                var obj = GetTileObjectFromPool();
                BindTileObject(obj, _tiles[x, y]);
                CreateGridTileObjects(obj, x, y);
            }
        }
    }

    private void CreateGridTileObjects(TileObject to, int x, int y) => _tileObjects[x, y] = to;

    public TileObject GetTileObjectFromTile(Tile tile)
    {
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                if(_tiles[x,y] == tile)
                    return _tileObjects[x,y];
            }
        }
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
            var t = _tiles[x, y];
            if (t != null) result.Add(t);
        }
        return result;
    }
    public void RemoveFirstLayerAtColumn(int xRow)
    {
        if (_tiles == null) 
            return;
        if (xRow < 0 || xRow >= gridSizeX) 
            return;

        Tile bottomTile = _tiles[xRow, 0];
        if (bottomTile != null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var to = transform.GetChild(i).GetComponent<TileObject>();
                if (to == null || to.Tile != bottomTile)
                    continue;
                ReturnTileObjectToPool(to);
                break;
            }
        }
        for (int y = 1; y < gridSizeY; y++)
        {
            _tiles[xRow, y - 1] = _tiles[xRow, y];
            if (_tiles[xRow, y - 1] != null)
                _tiles[xRow, y - 1].positionGrid = new Vector2(xRow, y - 1);
        }
        
        _tiles[xRow, gridSizeY - 1] = null;
        
        for (int i = 0; i < transform.childCount; i++)
        {
            var to = transform.GetChild(i).GetComponent<TileObject>();
            if (to == null || !to.gameObject.activeSelf || to.Tile == null) continue;
            to.UpdatePosition(NotifyObservers);
            to.UpdateSprite();
        }
        
    }

    private void NotifyObservers()
    {
        foreach (var item in _allObservers)
            item.OnNotify(ObserverMessage.UpdateRow);
    }

    private void BindTileObject(TileObject obj, Tile tile)
    {
        obj.Tile = tile;
        obj.grid = this;
        obj.UpdatePosition();
        obj.UpdateSprite();
        obj.OrderLayer();
    }
    

    private void OnDrawGizmosSelected()
    {
        if(!_showGizmos) return;
        float cell = cellSize;
        Vector3 offset = new Vector3(cell * 0.5f, cell * 0.5f, 0f);

        Vector3 gridCenter = transform.position + offset +
                             new Vector3((gridSizeX - 1) * cell * 0.5f, (gridSizeY - 1) * cell * 0.5f, 0f);
        
        Vector3 gridSize = new Vector3(gridSizeX * cell, gridSizeY * cell, 1f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(gridCenter, gridSize);

        if (_tiles == null) return;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(transform.position + new Vector3(x * cell, y * cell, 0f) + offset,
                    new Vector3(cell, cell, 1f));
            }
        }
    }

   
}
