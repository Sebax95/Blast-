using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridTiles: BaseMonoBehaviour, IObservable
{
    public int gridSizeX;
    public int gridSizeY;
    public float cellSize = 1f;
    private Tile[,] _tiles;
    private TileObject[,] _tileObjects;
    
    private ObjectPool<TileObject> _tileObjectPool;
    [SerializeField]
    private TileObject _tileObjectPrefab;
    
    private List<IObserver> _allObservers = new();

    private void Awake()
    {
        _tileObjectPool = new ObjectPool<TileObject>(FactoryTiles, TileObject.TurnOn, TileObject.TurnOff, gridSizeX * gridSizeY);
    }
    protected override void Start()
    {
        base.Start();
        CreateGrid();
    }
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
    
    public void CreateGrid()
    {
        _tiles = new Tile[gridSizeX, gridSizeY];
        _tileObjects = new TileObject[gridSizeX, gridSizeY];
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                var values = (ColorTile[])System.Enum.GetValues(typeof(ColorTile));
                var randomColor = values[Random.Range(0, values.Length)];
                _tiles[x,y] = new Tile(x+y, randomColor, new Vector2(x,y));
                var obj = _tileObjectPool.GetObject();
                obj.transform.SetParent(transform);
                obj.Tile = _tiles[x,y];
                obj.grid = this;
                obj.UpdatePosition();
                obj.UpdateSprite();
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

    private TileObject FactoryTiles() => Instantiate(_tileObjectPrefab);
    
    public Vector2 GetTilePosition(int x, int y) => transform.position + new Vector3(x * cellSize, y * cellSize, 0f);

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
        if (_tiles == null) return;
        if (xRow < 0 || xRow >= gridSizeX) return;

        Tile bottomTile = _tiles[xRow, 0];
        if (bottomTile != null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var to = transform.GetChild(i).GetComponent<TileObject>();
                if (to != null && to.Tile == bottomTile)
                {
                    _tileObjectPool.ReturnObject(to);
                    break;
                }
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
            to.UpdatePosition();
            to.UpdateSprite();
        }

        foreach (var item in _allObservers)
            item.OnNotify();
    }
    

    private void OnDrawGizmosSelected()
    {
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
